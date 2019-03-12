﻿using System;
using FluentEvents.Plugins;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace FluentEvents.UnitTests
{
    [TestFixture]
    public class EventsContextOptionsTests
    {
        private EventsContextOptions m_EventsContextOptions;

        [SetUp]
        public void SetUp()
        {
            m_EventsContextOptions = new EventsContextOptions();
        }

        [Test]
        public void AddPlugin_WithNullPlugin_ShouldThrow()
        {
            Assert.That(() =>
            {
                ((IFluentEventsPluginOptions) m_EventsContextOptions).AddPlugin(null);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void AddPlugin_CalledTwiceWithSamePluginType_ShouldThrow()
        {
            var plugin1 = new TestPlugin<object>();
            var plugin2 = new TestPlugin<object>();

            Assert.That(() =>
            {
                ((IFluentEventsPluginOptions)m_EventsContextOptions).AddPlugin(plugin1);
                ((IFluentEventsPluginOptions)m_EventsContextOptions).AddPlugin(plugin2);
            }, Throws.TypeOf<DuplicatePluginException>());
        }

        [Test]
        public void AddPlugin_ShouldAdd()
        {
            var plugin = new TestPlugin<object>();

            ((IFluentEventsPluginOptions)m_EventsContextOptions).AddPlugin(plugin);

            Assert.That(((IFluentEventsPluginOptions) m_EventsContextOptions).Plugins, Has.One.Items.EqualTo(plugin));
        }

        private class TestPlugin<T> : IFluentEventsPlugin
        {
            public void ApplyServices(IServiceCollection services)
            {
            }
            
        }
    }
}
