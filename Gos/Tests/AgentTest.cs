using Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Tests {
    [TestClass]
    public class SimpleServerTests : BaseTest {

        [TestInitialize]
        public void Init() {
        }

        [TestMethod]
        public void Test1() {
            var rand = new Random();
            var env = new Agents.Environment();
            var agent = new Agents.SimpleServer(env,"1");
            
            for(var i = 0 ; i < 100 ; i++){
                env.AddRequest("0","1",rand.Next(1000));
            }

            env.Run();

            /* Assert.IsTrue(env.solutionResponses
                .All(k => k. <  ); */
        }
    }
}
