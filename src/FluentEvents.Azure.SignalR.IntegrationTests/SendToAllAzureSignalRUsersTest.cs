﻿using System.Threading;
using System.Threading.Tasks;
using FluentEvents.Configuration;
using FluentEvents.Infrastructure;
using FluentEvents.IntegrationTests.Common;
using NUnit.Framework;

namespace FluentEvents.Azure.SignalR.IntegrationTests
{
    [TestFixture]
    public class SendToAllAzureSignalRUsersTest 
        : SendToAzureSignalRTestBase<SendToAllAzureSignalRUsersTest.TestEventsContext>
    {
        [Test]
        public async Task Test()
        {
            var semaphoreSlim = new SemaphoreSlim(3);
            var task1 = CheckEventPublishing(HubConnection1, semaphoreSlim, true);
            var task2 = CheckEventPublishing(HubConnection2, semaphoreSlim, true);
            var task3 = CheckEventPublishing(HubConnection3, semaphoreSlim, true);
            var allTasks = Task.WhenAll(task1, task2, task3);

            TestUtils.AttachAndRaiseEvent(EventsContext, EventsScope);
            semaphoreSlim.Release(3);

            await allTasks;

            Assert.That(ReceivedEventsCount, Is.EqualTo(3));
        }

        public class TestEventsContext : EventsContext
        {
            protected override void OnBuildingPipelines(IPipelinesBuilder pipelinesBuilder)
            {
                pipelinesBuilder
                    .Event<TestEvent>()
                    .IsPiped()
                    .ThenIsSentToAllAzureSignalRUsers(HubName, HubMethodName);
            }

            public TestEventsContext(EventsContextOptions options, IRootAppServiceProvider rootAppServiceProvider) 
                : base(options, rootAppServiceProvider)
            {
            }
        }
    }
}
