﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;

namespace DotQueue.Client.Tests
{
    [TestFixture]
    public class MessageQueueTests
    {
        private MessageQueue<Profile> _messageQueue;
        private DotQueueAddress _defaultAddress = new DotQueueAddress();
        private IListenerAdapter<Profile> _listenerAdapter;
        private IHttpAdapter<Profile> _httpAdapter;
        private ILocalPortResolver _portResolver;
        private int _defaultLocalPort = 1122;
        private IWaitDurationHelper _durationHelper;

        [SetUp]
        public void Setup()
        {
            _durationHelper = Substitute.For<IWaitDurationHelper>();
            _durationHelper.SubscribtionRenewalSpan().Returns(TimeSpan.FromMilliseconds(100));
            _portResolver = Substitute.For<ILocalPortResolver>();
            _portResolver.FindFreePort().Returns(_defaultLocalPort);
            _listenerAdapter = Substitute.For<IListenerAdapter<Profile>>();
            _httpAdapter = Substitute.For<IHttpAdapter<Profile>>();
            _messageQueue = new MessageQueue<Profile>(_defaultAddress, 
                _httpAdapter, _listenerAdapter, _portResolver, _durationHelper);    
        }

        [Test]
        public void Start_Listener_With_Correct_Port()
        {
            Thread.Sleep(1000);
            _listenerAdapter.Received().StartListener(_defaultLocalPort);
        }

        [Test]
        public void Subscribe_To_Queue()
        {
            _httpAdapter.Received().Subscribe(_defaultLocalPort);
        }

        [Test]
        public void Renew_Subscription_After_Period()
        {
            var count = 0;
            _httpAdapter.When(a => a.Subscribe(_defaultLocalPort)).Do(_ =>
            {
                count++;
            });

            Thread.Sleep(1000);
            Assert.That(count, Is.GreaterThan(1));
            _httpAdapter.Received(count+1).Subscribe(_defaultLocalPort);
        }

        [Test]
        public void Loop_Yields_Results_If_Count_Greater_Than_0()
        {
            _durationHelper.NewMessageWaitDuration().Returns(TimeSpan.FromMilliseconds(50));
            _messageQueue.Count().Returns(3, 2, 1, 0);
            var item = new Profile();
            _messageQueue.Pull().Returns(item);
            int count = 0;

            Task.Run(() =>
            {
                foreach (var profile in _messageQueue)
                {
                    count++;
                }
            });

            Thread.Sleep(1000);

            _httpAdapter.Received(count+1).Count();
        }

        [Test]
        public void Add_Adds_Message_To_Queue()
        {
            var message = new Profile();
            _messageQueue.Add(message);
            _httpAdapter.Received().Add(message);
        }

        [Test]
        public void Add_Throw_If_Message_Is_null()
        {
            Profile profile = null;
            Assert.Throws<ArgumentNullException>(() => _messageQueue.Add(profile));
        }

        [Test]
        public void Pull_Returns_Message_From_Adapter()
        {
            var expected = new Profile();
            _httpAdapter.Pull().Returns(expected);
            var actual = _messageQueue.Pull();
            Assert.That(actual, Is.EqualTo(expected));
        }
        [Test]
        public void Count_Returns_Message_From_Adapter()
        {
            var expected = 5;
            _httpAdapter.Count().Returns(expected);
            var actual = _messageQueue.Count();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}