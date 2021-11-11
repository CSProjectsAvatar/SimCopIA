using Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Tests {
    [TestClass]
    public class OneServerSimtorTests : BaseTest {
        private ILogger<OneServerSimulator> _log;

        [TestInitialize]
        public void Init() {
            _log = Container.GetRequiredService<ILogger<OneServerSimulator>>();
        }

        [TestMethod]
        public void ArrivalsBeforeDepartures() {
            var simtor = new OneServerSimulator(_log);
            simtor.Run(1000);

            Assert.IsTrue(simtor.Arrivals.Keys
                .All(k => simtor.Arrivals[k] < simtor.Departures[k]));
        }
    }
}
