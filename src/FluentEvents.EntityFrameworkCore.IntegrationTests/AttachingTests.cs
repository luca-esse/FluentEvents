﻿using System.Linq;
using FluentEvents.Configuration;
using FluentEvents.Infrastructure;
using FluentEvents.IntegrationTests.Common;
using FluentEvents.Pipelines.Publication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentEvents.EntityFrameworkCore.IntegrationTests
{
    [TestFixture]
    public class AttachingTests
    {
        private TestDbContext _testDbContext;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddEventsContext<TestEventsContext>(options =>
            {
                options.AttachToDbContextEntities<TestDbContext>();
            });

            services.AddWithEventsAttachedTo<TestEventsContext>(() =>
            {
                services.AddDbContext<TestDbContext>(options =>
                {
                    options.UseInMemoryDatabase("test");
                });
            });
            services.AddSingleton<SubscribingService>();

            _serviceProvider = services.BuildServiceProvider();

            using (var serviceScope = _serviceProvider.CreateScope())
            {
                _testDbContext = serviceScope.ServiceProvider.GetRequiredService<TestDbContext>();

                _testDbContext.TestEntities.Add(new TestEntity());
                _testDbContext.SaveChanges();
            }
        }

        [Test]
        public void AttachFromQueryResultTest()
        {
            var subscribingService = _serviceProvider.GetRequiredService<SubscribingService>();

            using (var serviceScope = _serviceProvider.CreateScope())
            {
                var testDbContext = serviceScope.ServiceProvider.GetService<TestDbContext>();
                var testEntity = testDbContext.TestEntities.First();
                testEntity.RaiseEvent("");
            }

            Assert.That(subscribingService, Has.Property(nameof(SubscribingService.TestEvents)).With.One.Items);
        }

        private class TestDbContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestDbContext(DbContextOptions<TestDbContext> options) 
                : base(options)
            {
                
            }
        }

        private class TestEventsContext : EventsContext
        {
            protected override void OnBuildingSubscriptions(ISubscriptionsBuilder subscriptionsBuilder)
            {
                subscriptionsBuilder
                    .ServiceHandler<SubscribingService, TestEvent>()
                    .HasGlobalSubscription();
            }

            protected override void OnBuildingPipelines(IPipelinesBuilder pipelinesBuilder)
            {
                pipelinesBuilder
                    .Event<TestEvent>()
                    .IsPiped()
                    .ThenIsPublishedToGlobalSubscriptions();
            }

            public TestEventsContext(EventsContextOptions options, IRootAppServiceProvider rootAppServiceProvider) 
                : base(options, rootAppServiceProvider)
            {
            }
        }
    }
}
