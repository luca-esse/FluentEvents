﻿using FluentEvents.Model;
using Moq;
using NUnit.Framework;

namespace FluentEvents.UnitTests.Model
{
    [TestFixture]
    public class SourceModelsServiceTests
    {
        private SourceModelsService m_SourceModelsService;
        private Mock<IInfrastructureEventsContext> m_EventsContextMock;

        [SetUp]
        public void SetUp()
        {
            m_EventsContextMock = new Mock<IInfrastructureEventsContext>(MockBehavior.Strict);
            m_SourceModelsService = new SourceModelsService();
        }

        [Test]
        public void GetOrCreateSourceModel_WhenSourceModelDoesNotExists_ShouldCreate()
        {
            var sourceModel = m_SourceModelsService.GetOrCreateSourceModel(typeof(object), m_EventsContextMock.Object);

            Assert.That(m_SourceModelsService.GetSourceModels(), Has.Exactly(1).Items);
            Assert.That(m_SourceModelsService.GetSourceModels(), Has.Exactly(1).Items.EqualTo(sourceModel));
        }

        [Test]
        public void GetOrCreateSourceModel_WhenSourceModelExists_ShouldNotCreate()
        {
            m_SourceModelsService.GetOrCreateSourceModel(typeof(object), m_EventsContextMock.Object);
            var sourceModel = m_SourceModelsService.GetOrCreateSourceModel(typeof(object), m_EventsContextMock.Object);

            Assert.That(m_SourceModelsService.GetSourceModels(), Has.Exactly(1).Items);
            Assert.That(m_SourceModelsService.GetSourceModels(), Has.Exactly(1).Items.EqualTo(sourceModel));
        }

        [Test]
        public void GetOrCreateSourceModel_WhenSourceModelExists_ShouldReturn()
        {
            var createdSourceModel = m_SourceModelsService.GetOrCreateSourceModel(typeof(object), m_EventsContextMock.Object);
            var returnedSourceModel = m_SourceModelsService.GetSourceModel(typeof(object));

            Assert.That(returnedSourceModel, Is.Not.Null);
            Assert.That(returnedSourceModel, Is.EqualTo(createdSourceModel));
        }

        [Test]
        public void GetOrCreateSourceModel_WhenSourceModelDoesNotExists_ShouldReturnNull()
        {
            var returnedSourceModel = m_SourceModelsService.GetSourceModel(typeof(object));

            Assert.That(returnedSourceModel, Is.Null);
        }
    }
}