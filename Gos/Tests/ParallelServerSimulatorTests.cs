using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class ParallelServerSimulatorTests : BaseTest {
        private ILogger<ParallelServerSimulator> _log;

        [TestInitialize]
        public void Init() {
            _log = LoggerFact.CreateLogger<ParallelServerSimulator>();
        }

        [TestMethod]
        public void ArrivalsBeforeDepartures() {
            var simtor = new ParallelServerSimulator(5,_log);
            simtor.Run(100);

            Assert.IsTrue(simtor.Arrivals.Keys
                .All(k => simtor.Arrivals[k] < simtor.Deapertures[k]));
        }
    }
}
