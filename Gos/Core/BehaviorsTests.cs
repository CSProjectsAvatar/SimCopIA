using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;

namespace Core {
    [TestClass]
    public class BehaviorsTests {

        [TestInitialize]
        public void Init() {

        }

        [TestMethod]
        public void WorkerBehavTest_1() {
            var worker = BehaviorsLib.Worker;
            var server1 = new Server("S1");
            var server2 = new Server("S2");
            
            var r1 = new Resource("img1");
            var r2 = new Resource("index.html");

            var p = new Request("S1", "S2", RequestType.AskSomething);
            p.AskingRscs.AddRange(new[] { r1, r2 });


            // worker.Run(server1.Stats, p);
        }

        [TestMethod]
        public void ContractorBehavTest_1() {
            // var contractor = BehaviorsLib.Contractor;
            // hallarle el inverso al subconjunto, y crear un nuevo dict<string, List<string>> 
            //de agentes a recursos que proveen donde cada item de los values son una llave del nuevo dict
            var subDict = new Dictionary<string, List<string>>();
            subDict.Add("img1", new List<string>() { "s1", "s2" });
            subDict.Add("img2", new List<string>() { "s2" });
            subDict.Add("img3", new List<string>() { "s1" });

            var inverseDict = from kv in subDict
                            group kv by kv.Value into gr
                            select new
                            {
                                Key = gr.Key,
                                Value = gr.Select(x => x.Key).ToList()
                            };

            //  agrupa los key de inverseDict y le asigna sus valores
            var grouped = from kv in inverseDict
                            group kv by kv.Key into gr
                            select new
                            {
                                Key = gr.Key,
                                Value = gr.Select(x => x.Value).ToList()
                            };

            foreach (var item in grouped) {
                Console.WriteLine($"{item.Key} => {string.Join(",", item.Value)}");
            }
        }

    }
}
