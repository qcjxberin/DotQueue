﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotQueue.Client
{
    internal class ListenerAdapter<T> : IListenerAdapter<T>
    {
        private int _localPort;
        private bool _listenerStarted;

        public void StartListener(int port)
        {
            if (_listenerStarted)
                return;

            _localPort = port;
            _listenerStarted = true;
            Task.Run(() => StartListener());
        }

        public event EventHandler<QueueNotification> NotificationReceived;
        
        private void StartListener()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{_localPort}/");
            listener.Start();
            while (true)
                try
                {
                    {
                        HttpListenerContext context = listener.GetContext();
                        HttpListenerRequest request = context.Request;
                        ProcessRequest(request.Url.LocalPath);
                        HttpListenerResponse response = context.Response;
                        string responseString = "OK";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                        response.ContentLength64 = buffer.Length;
                        Stream output = response.OutputStream;
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                    }

                }
                catch (Exception)
                {
                    Thread.Sleep(1000);
                }
            listener.Stop();
        }

        private void ProcessRequest(string message)
        {
            Console.WriteLine($"Message received: {message}");
            if (message.Contains("subscribtion_added"))
            {
                Notify(QueueNotification.SubscriptionConfirmed);
            }
            if (message.Contains("new_message"))
            {
                Notify(QueueNotification.NewMessage);
            }
        }

        private void Notify(QueueNotification notification)
        {
            if (NotificationReceived != null)
            {
                NotificationReceived(this, notification);
            }
        }
    }
}