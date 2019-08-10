using System;
using System.Threading.Tasks;
using FluentEvents.Attachment;
using FluentEvents.Infrastructure;
using FluentEvents.Model;
using FluentEvents.Pipelines;
using FluentEvents.Routing;
using Moq;
using NUnit.Framework;

namespace FluentEvents.UnitTests.Routing
{
    [TestFixture]
    public class AttachingServiceTests
    {
        private Mock<ISourceModelsService> _sourceModelsServiceMock;
        private Mock<IRoutingService> _routingServiceMock;
        private Mock<IAttachingInterceptor> _attachingInterceptorMock1;
        private Mock<IAttachingInterceptor> _attachingInterceptorMock2;
        private Mock<IEventsScope> _eventsScopeMock;

        private IAttachingService _attachingService;

        [SetUp]
        public void SetUp()
        {
            _sourceModelsServiceMock = new Mock<ISourceModelsService>(MockBehavior.Strict);
            _routingServiceMock = new Mock<IRoutingService>(MockBehavior.Strict);
            _attachingInterceptorMock1 = new Mock<IAttachingInterceptor>(MockBehavior.Strict);
            _attachingInterceptorMock2 = new Mock<IAttachingInterceptor>(MockBehavior.Strict);
            _eventsScopeMock = new Mock<IEventsScope>(MockBehavior.Strict);

            _attachingService = new AttachingService(
                _sourceModelsServiceMock.Object,
                _routingServiceMock.Object,
                new[]
                {
                    _attachingInterceptorMock1.Object,
                    _attachingInterceptorMock2.Object
                }
            );
        }

        [TearDown]
        public void TearDown()
        {
            _sourceModelsServiceMock.Verify();
            _routingServiceMock.Verify();
            _attachingInterceptorMock1.Verify();
            _attachingInterceptorMock2.Verify();
        }

        [Test]
        [Sequential]
        public void Attach_WithNullParameters_ShouldThrow(
            [Values(true, false, true)] bool isSourceNull,
            [Values(false, true, true)] bool isEventsScopeNull
        )
        {
            var source = isSourceNull ? null : new object();
            var eventsScope = isEventsScopeNull ? null : _eventsScopeMock.Object;
            
            Assert.That(() =>
            {
                _attachingService.Attach(source, eventsScope);
            }, Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public async Task Attach_ShouldInvokeInterceptorsAndAddEventHandlers()
        {
            var source = new TestSource();
            SetUpSourceModelsService();
            SetUpInterceptors(source);

            _routingServiceMock
                .Setup(x => x.RouteEventAsync(It.IsAny<PipelineEvent>(), _eventsScopeMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();

            _attachingService.Attach(
                source,
                _eventsScopeMock.Object
            );

            await source.RaiseEvents();

            Assert.That(_routingServiceMock.Invocations, Has.Exactly(2).Items);
        }

        private void SetUpSourceModelsService()
        {
            var sourceModel = new SourceModel(typeof(TestSource));

            _sourceModelsServiceMock
                .Setup(x => x.GetOrCreateSourceModel(typeof(TestSource)))
                .Returns(sourceModel)
                .Verifiable();
        }

        private void SetUpInterceptors(object source)
        {
            _attachingInterceptorMock1
                .Setup(x => x.OnAttaching(It.IsAny<AttachDelegate>(), source, _eventsScopeMock.Object))
                .Verifiable();

            _attachingInterceptorMock2
                .Setup(x => x.OnAttaching(It.IsAny<AttachDelegate>(), source, _eventsScopeMock.Object))
                .Verifiable();
        }

        private class TestSource
        {
            public event EventPublisher<TestEvent> Event;
            public event AsyncEventPublisher<TestEvent> AsyncEvent;

            public async Task RaiseEvents()
            {
                Event?.Invoke(new TestEvent());
                await (AsyncEvent?.Invoke(new TestEvent()) ?? Task.CompletedTask);
            }
        }

        private class TestEvent
        {
            public int Prop { get; set; }
        }
    }
}
