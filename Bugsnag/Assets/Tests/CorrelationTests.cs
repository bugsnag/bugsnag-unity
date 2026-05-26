using NUnit.Framework;
using BugsnagUnity.Payload;

namespace BugsnagUnityTests
{
    [TestFixture]
    public class CorrelationTests
    {
        [Test]
        public void Constructor_SetsTraceIdAndSpanId()
        {
            var correlation = new Correlation("trace-123", "span-456");
            Assert.AreEqual("trace-123", correlation.TraceId);
            Assert.AreEqual("span-456", correlation.SpanId);
        }

        [Test]
        public void TraceId_CanBeUpdated()
        {
            var correlation = new Correlation("trace-1", "span-1");
            correlation.TraceId = "trace-2";
            Assert.AreEqual("trace-2", correlation.TraceId);
        }

        [Test]
        public void SpanId_CanBeUpdated()
        {
            var correlation = new Correlation("trace-1", "span-1");
            correlation.SpanId = "span-2";
            Assert.AreEqual("span-2", correlation.SpanId);
        }

        [Test]
        public void NullTraceId_ReturnsEmptyString()
        {
            var correlation = new Correlation(null, "span-1");
            Assert.AreEqual(string.Empty, correlation.TraceId);
        }

        [Test]
        public void NullSpanId_ReturnsEmptyString()
        {
            var correlation = new Correlation("trace-1", null);
            Assert.AreEqual(string.Empty, correlation.SpanId);
        }
    }
}
