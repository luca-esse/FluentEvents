﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentEvents.Pipelines;
using FluentEvents.Queues;
using FluentEvents.Subscriptions;
using Moq;
using NUnit.Framework;

namespace FluentEvents.UnitTests
{
    [TestFixture]
    public class EventsScopeTests
    {
        private Mock<IInfrastructureEventsContext> m_EventsContext1;
        private Mock<IInfrastructureEventsContext> m_EventsContext2;
        private Mock<IServiceProvider> m_AppServiceProviderMock;
        private Mock<IServiceProvider> m_InternalServiceProviderMock1;
        private Mock<IServiceProvider> m_InternalServiceProviderMock2;
        private Mock<IScopedSubscriptionsService> m_ScopedSubscriptionsServiceMock1;
        private Mock<IScopedSubscriptionsService> m_ScopedSubscriptionsServiceMock2;
        private Mock<IEventsQueuesService> m_EventsQueuesServiceMock;

        private IInfrastructureEventsContext[] m_EventsContexts;
        private EventsScope m_EventsScope;

        [SetUp]
        public void SetUp()
        {
            m_EventsContext1 = new Mock<IInfrastructureEventsContext>(MockBehavior.Strict);
            m_EventsContext2 = new Mock<IInfrastructureEventsContext>(MockBehavior.Strict);
            m_AppServiceProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            m_InternalServiceProviderMock1 = new Mock<IServiceProvider>(MockBehavior.Strict);
            m_InternalServiceProviderMock2 = new Mock<IServiceProvider>(MockBehavior.Strict);
            m_ScopedSubscriptionsServiceMock1 = new Mock<IScopedSubscriptionsService>(MockBehavior.Strict);
            m_ScopedSubscriptionsServiceMock2 = new Mock<IScopedSubscriptionsService>(MockBehavior.Strict);
            m_EventsQueuesServiceMock = new Mock<IEventsQueuesService>(MockBehavior.Strict);

            m_EventsContexts = new[]
            {
                m_EventsContext1.Object,
                m_EventsContext2.Object
            };

            m_EventsScope = new EventsScope(
                m_EventsContexts,
                m_AppServiceProviderMock.Object,
                m_EventsQueuesServiceMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            m_EventsContext1.Verify();
            m_EventsContext2.Verify();
            m_AppServiceProviderMock.Verify();
            m_InternalServiceProviderMock1.Verify();
            m_InternalServiceProviderMock2.Verify();
            m_ScopedSubscriptionsServiceMock1.Verify();
            m_ScopedSubscriptionsServiceMock2.Verify();
            m_EventsQueuesServiceMock.Verify();
        }

        [Test]
        public void GetSubscriptions_OnFirstCall_ShouldCreateSubscriptionsForEveryContext()
        {
            var allSubscriptions = SetUpSubscriptionsCreation().ToArray();

            var createdSubscriptions = m_EventsScope.GetSubscriptions().ToArray();

            Assert.That(createdSubscriptions, Is.EquivalentTo(allSubscriptions));
        }

        [Test]
        public void GetSubscriptions_OnSecondCall_ShouldNotCreateSubscriptions()
        {
            var allSubscriptions = SetUpSubscriptionsCreation().ToArray();

            var createdSubscriptions = m_EventsScope.GetSubscriptions().ToArray();

            var storedSubscriptions = m_EventsScope.GetSubscriptions().ToArray();

            Assert.That(m_EventsContext1.Invocations, Has.One.Items);
            Assert.That(m_EventsContext2.Invocations, Has.One.Items);
            Assert.That(createdSubscriptions, Is.EquivalentTo(allSubscriptions));
            Assert.That(storedSubscriptions, Is.EquivalentTo(createdSubscriptions));
        }

        [Test]
        public async Task ProcessQueuedEventsAsync_ShouldCallEventsQueuesService()
        {
            const string queueName = "queueName";

            m_EventsQueuesServiceMock
                .Setup(x => x.ProcessQueuedEventsAsync(m_EventsScope, m_EventsContext1.Object, queueName))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await m_EventsScope.ProcessQueuedEventsAsync(m_EventsContext1.Object, queueName);
        }

        [Test]
        public void DiscardQueuedEvents_ShouldCallEventsQueuesService()
        {
            const string queueName = "queueName";

            m_EventsQueuesServiceMock
                .Setup(x => x.DiscardQueuedEvents(m_EventsContext1.Object, queueName))
                .Verifiable();

            m_EventsScope.DiscardQueuedEvents(m_EventsContext1.Object, queueName);
        }

        [Test]
        public void EnqueueEvent_ShouldCallEventsQueuesService()
        {
            const string queueName = "queueName";
            var pipelineEvent = new PipelineEvent("a", new object(), new object());
            var pipeline = new Pipeline(queueName, null, m_InternalServiceProviderMock1.Object);

            m_EventsQueuesServiceMock
                .Setup(x => x.EnqueueEvent(pipelineEvent, pipeline))
                .Verifiable();

            m_EventsScope.EnqueueEvent(pipelineEvent, pipeline);
        }

        private IEnumerable<Subscription> SetUpSubscriptionsCreation()
        {
            var scopedSubscriptionsFactory1Subscriptions = new[] { new Subscription(typeof(object)), new Subscription(typeof(object)) };
            var scopedSubscriptionsFactory2Subscriptions = new[] { new Subscription(typeof(object)) };
            var allSubscriptions = scopedSubscriptionsFactory1Subscriptions.Concat(scopedSubscriptionsFactory2Subscriptions);

            m_EventsContext1
                .Setup(x => x.Instance)
                .Returns(m_InternalServiceProviderMock1.Object)
                .Verifiable();

            m_InternalServiceProviderMock1
                .Setup(x => x.GetService(typeof(IScopedSubscriptionsService)))
                .Returns(m_ScopedSubscriptionsServiceMock1.Object)
                .Verifiable();

            m_ScopedSubscriptionsServiceMock1
                .Setup(x => x.CreateScopedSubscriptionsForServices(m_AppServiceProviderMock.Object))
                .Returns(scopedSubscriptionsFactory1Subscriptions)
                .Verifiable();

            m_EventsContext2
                .Setup(x => x.Instance)
                .Returns(m_InternalServiceProviderMock2.Object)
                .Verifiable();

            m_InternalServiceProviderMock2
                .Setup(x => x.GetService(typeof(IScopedSubscriptionsService)))
                .Returns(m_ScopedSubscriptionsServiceMock2.Object)
                .Verifiable();

            m_ScopedSubscriptionsServiceMock2
                .Setup(x => x.CreateScopedSubscriptionsForServices(m_AppServiceProviderMock.Object))
                .Returns(scopedSubscriptionsFactory2Subscriptions)
                .Verifiable();

            return allSubscriptions;
        }
    }
}