﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentEvents.Config;
using FluentEvents.IntegrationTests.Common;
using FluentEvents.Pipelines;
using FluentEvents.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentEvents.IntegrationTests
{
    [TestFixture]
    public class UnknownPipelineModuleTest
    {
        private TestEventsContext m_TestEventsContext;
        private EventsScope m_EventsScope;

        [SetUp]
        public void SetUp()
        {
            var services = new ServiceCollection();

            services.AddEventsContext<TestEventsContext>(options => { });

            var serviceProvider = services.BuildServiceProvider();
            m_TestEventsContext = serviceProvider.GetService<TestEventsContext>();
            m_EventsScope = serviceProvider.CreateScope().ServiceProvider.GetService<EventsScope>();
        }

        [Test]
        public void WhenPipelineModuleIsNotRegistered_Publishing_ShouldThrow()
        {
            Assert.That(() =>
            {
                TestUtils.AttachAndRaiseEvent(m_TestEventsContext, m_EventsScope);
            }, Throws.TypeOf<PipelineModuleNotFoundException>());
        }

        private class TestEventsContext : EventsContext
        {
            protected override void OnBuildingPipelines(PipelinesBuilder pipelinesBuilder)
            {
                var pipelineBuilder = pipelinesBuilder
                    .Event<TestEntity, TestEventArgs>(nameof(TestEntity.Test))
                    .IsForwardedToPipeline();

                var pipeline = pipelineBuilder.Get<IPipeline>();

                pipeline.AddModule<TestPipelineModule, TestPipelineModuleConfig>(new TestPipelineModuleConfig());
            }
        }

        private class TestPipelineModule : IPipelineModule<TestPipelineModuleConfig>
        {
            public Task InvokeAsync(TestPipelineModuleConfig config, PipelineContext pipelineContext, NextModuleDelegate invokeNextModule) 
                => Task.CompletedTask;
        }

        private class TestPipelineModuleConfig
        {
        }
    }
}