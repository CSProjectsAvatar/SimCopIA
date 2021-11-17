using Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests {
    [TestClass]
    public class OneLeaderFollowsSimtorTests : BaseTest {
        private ILogger<OneLeaderFollowersSimulator> _simLog;
        private ILogger<OneLeaderFollowsSimtorTests> _log;

        [TestInitialize]
        public void Init() {
            //_log = Container.GetRequiredService<ILogger<OneLeaderFollowersSimulator>>();
            _simLog = LoggerFact.CreateLogger<OneLeaderFollowersSimulator>();
            _log = LoggerFact.CreateLogger<OneLeaderFollowsSimtorTests>();
        }

        [TestMethod]
        public void ArrivalsBeforeDepartures() {
            var simtor = new OneLeaderFollowersSimulator(5, _simLog);
            simtor.Run(100);

            var deps = simtor.GetDepartures();

            Logger.LogMessage(string.Join('\n', simtor.Arrivals));
            Logger.LogMessage("\n");
            Logger.LogMessage(string.Join('\n', deps));

            Assert.IsTrue(simtor.Arrivals.Keys
                .All(k => simtor.Arrivals[k] < deps[k]));
        }

        [DataTestMethod]
        [DataRow(14u, 5u)]
        [DataRow(13u, 10u)]
        [DataRow(5u, 103u)]
        public void FinishedRun(uint times, uint followers) {
            for (int i = 0; i < times; i++) {
                var simtor = new OneLeaderFollowersSimulator(followers, _simLog);

                Logger.LogMessage($"\nCorrida {i + 1}:\n");

                var t = Task.Run(() => simtor.Run(10));
                Thread.Sleep(2000);

                Logger.LogMessage($"Estado: {t.Status}\n");

                Assert.IsTrue(t.IsCompleted);
                Assert.IsTrue(t.Status == TaskStatus.RanToCompletion); // completada satisfactoriamente
            }
        }

        [TestMethod]
        public void OneFollower() {
            for (int i = 0; i < 5; i++) {
                var simtor = new OneLeaderFollowersSimulator(1, _simLog);

                _log.LogInformation($"\nCorrida {i + 1}:\n");

                simtor.Run(10);
                var deps = simtor.GetDepartures();

                Assert.IsTrue(simtor.Arrivals.All(kv => deps.ContainsKey(kv.Key))); // arribos subconjunto d partidas
                Assert.IsTrue(deps.All(kv => simtor.Arrivals.ContainsKey(kv.Key))); //  partidas subconjunto d arribos
            }
        }

        [TestMethod]
        public void LambdaVariation() {
            var simtor = new OneLeaderFollowersSimulator(5, .5, _simLog);
            simtor.Run(10);
            var arrivals = simtor.Arrivals.Count;

            simtor = new OneLeaderFollowersSimulator(5, 4, _simLog);
            simtor.Run(10);

            Assert.IsTrue(arrivals < simtor.Arrivals.Count); // mientras mayor el lambda, más arribos ocurren
        }
    }
}
