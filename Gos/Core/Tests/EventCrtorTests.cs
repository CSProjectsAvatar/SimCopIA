using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;
using Utils;
namespace Core {

    [TestClass]
    public class EventCrtorTests {
        
        #region  vars
     
        #endregion

        [TestInitialize]
        public void Init() {
            var env = new Env();

            var r1 = new Resource("img1");
            var r2 = new Resource("icon1");

            var r3 = new Resource("about.html");
            var r4 = new Resource("table1");

            var r5 = new Resource("text(caption)");
            r5.AddReqList(new List<Resource> { r1, r2 });

            var r6 = new Resource("index.html");
            r6.AddReqList(new List<Resource>{ 
                Resource.Resources["table1"],
                Resource.Resources["text(caption)"],
             });
            var r7 = new Resource("sells.html");
            
        }
        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
            Env.ClearServersLayers();
        }
        
        [TestMethod]
        public void EventCrtorT(){
            List<Type> eventTypes = new List<Type>(){
                typeof(Request)
            };
            var eCrtor = new EventCreator(eventTypes);
            var events = eCrtor.GetEvents(10).Select(x => x.Item1).ToList();
            
            // Assure thats the sender have ID "0"
            foreach (var e in events) {
                var req = (e as Request);

                Assert.AreEqual("0", req.Sender);
                Assert.AreEqual(ReqType.DoIt, req.Type);
                Assert.IsTrue(1 <= req.AskingRscs.Count);
            }
        }
        [TestMethod]
        public void EventCrtorT_ReqAndFailure(){
            List<Type> eventTypes = new List<Type>(){
                typeof(Request),
                typeof(CritFailure)
            };
            var eCrtor = new EventCreator(eventTypes);
            var events = eCrtor.GetEvents(20).Select(x => x.Item1).ToList();
            
            // at least one failure and one request
            Assert.IsTrue(events.Count(x => x is CritFailure) > 0);
            Assert.IsTrue(events.Count(x => x is Request) > 0);
        }

    }
}
