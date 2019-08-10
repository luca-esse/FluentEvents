using System;
using System.Threading.Tasks;
using FluentEvents.Infrastructure;
using FluentEvents.Pipelines;
using FluentEvents.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FluentEvents.UnitTests.Routing
{
    [TestFixture]
    public class RoutingServiceTests
    {
        private Mock<ILogger<RoutingService>> _loggerMock;
        private Mock<IDisposable> _loggerScopeMock;
        private Mock<IPipelinesService> _pipelinesServiceMock;
        private Mock<IPipeline> _pipelineMock;
        private Mock<IEventsScope> _eventsScopeMock;

        private RoutingService _routingService;
        private PipelineEvent _pipelineEvent;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<RoutingService>>(MockBehavior.Strict);
            _loggerScopeMock = new Mock<IDisposable>(MockBehavior.Strict);
            _pipelineMock = new Mock<IPipeline>(MockBehavior.Strict);
            _pipelinesServiceMock = new Mock<IPipelinesService>(MockBehavior.Strict);
            _eventsScopeMock = new Mock<IEventsScope>(MockBehavior.Strict);

            _routingService = new RoutingService(
                _loggerMock.Object,
                _pipelinesServiceMock.Object
            );
            _pipelineEvent = new PipelineEvent(new object());
        }

        [TearDown]
        public void TearDown()
        {
            _loggerMock.Verify();
            _loggerScopeMock.Verify();
            _pipelineMock.Verify();
            _pipelinesServiceMock.Verify();
        }

        [Test]
        public async Task RouteEventAsync_ShouldProcessEvent()
        {
            _pipelineMock
                .Setup(x => x.ProcessEventAsync(_pipelineEvent, _eventsScopeMock.Object))
                .Returns(Task.CompletedTask)
                .Verifiable();
            
            _pipelinesServiceMock
                .Setup(x => x.GetPipelines(typeof(object)))
                .Returns(new [] { _pipelineMock.Object })
                .Verifiable();
            
            _loggerScopeMock
                .Setup(x => x.Dispose())
                .Verifiable();

            _loggerMock
                .Setup(x => x.BeginScope(It.IsAny<object>()))
                .Returns(_loggerScopeMock.Object)
                .Verifiable();

            _loggerMock
                .Setup(x => x.IsEnabled(LogLevel.Information))
                .Returns(true)
                .Verifiable();

            _loggerMock
                .Setup(x => x.Log(
                    LogLevel.Information,
                    RoutingLoggerMessages.EventIds.EventRoutedToPipeline,
                    It.IsAny<object>(),
                    null,
                    It.IsAny<Func<object, Exception, string>>()
                ))
                .Verifiable();

            await _routingService.RouteEventAsync(_pipelineEvent, _eventsScopeMock.Object);
        }
    }
}
