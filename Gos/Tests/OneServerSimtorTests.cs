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
    public class OneServerSimtorTests : BaseTest {
        private ILogger<OneServerSimulator> _log;

        [TestInitialize]
        public void Init() {
            //_log = Container.GetRequiredService<ILogger<OneServerSimulator>>();
            _log = LoggerFact.CreateLogger<OneServerSimulator>();
        }

        [TestMethod]
        public void ArrivalsBeforeDepartures() {
            var simtor = new OneServerSimulator(_log);
            simtor.Run(1000);

            Assert.IsTrue(simtor.Arrivals.Keys
                .All(k => simtor.Arrivals[k] < simtor.Departures[k]));
        }

        [DataTestMethod]
        [DataRow(13u)]
        public void FinishedRun(uint times) {
            for (int i = 0; i < times; i++) {
                var simtor = new OneServerSimulator(_log);

                Logger.LogMessage($"\nCorrida {i + 1}:\n");

                var t = Task.Run(() => simtor.Run(10));
                Thread.Sleep(2000);

                Logger.LogMessage($"Estado: {t.Status}\n");

                Assert.IsTrue(t.IsCompleted);
                Assert.IsTrue(t.Status == TaskStatus.RanToCompletion); // completada satisfactoriamente
            }
        }
    }
}
