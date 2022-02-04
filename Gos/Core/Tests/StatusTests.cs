using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;
using Utils;
namespace Core {

    [TestClass]
    public class STatusTests {

        [TestInitialize]
        public void Init() {
            var env = new Env();

            // var r1 = new Resource("img1");
            // var r2 = new Resource("icon1");

            // var r3 = new Resource("about.html");
            // var r4 = new Resource("table1");

            // var r5 = new Resource("text(caption)");
            // r5.AddReqList(new List<Resource> { r1, r2 });

            // var r6 = new Resource("index.html");
            // r6.AddReqList(new List<Resource>{ 
            //     Resource.Resources["table1"],
            //     Resource.Resources["text(caption)"],
            //  });
            // var r7 = new Resource("sells.html");
            
            var s1 = new Server("S1");
            var s2 = new Server("S2");
            var s3 = new Server("S3");
            var s7 = new Server("S7");
            var s8 = new Server("S8");

            var s4 = new Server("S4");
            var mPagos = new MicroService("API-Pagos");

            var s5 = new Server("S5");
            var mMulta = new MicroService("API-Multa");
            var s6 = new Server("S6");

            // s1.Stats.AvailableResources = new List<Resource> { r1 };
            // s2.Stats.AvailableResources = new List<Resource> { r1, r2 };
            // s3.Stats.AvailableResources = new List<Resource> { r1, r2, r3 };
            // s4.Stats.AvailableResources = new List<Resource> { r4, r5 };
            // s5.Stats.AvailableResources = new List<Resource> { r6, r7 };
            // s6.Stats.AvailableResources = new List<Resource> { r6, r7 };

            var servers = new List<Server> { s1, s2, s3, s4, s7, s8 };

            // s4.SetMService(mPagos.Name);
            // s5.SetMService(mMulta.Name);
            // s6.SetMService(mMulta.Name);
            env.AddServerList(servers);
        }
        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
        }
        
        [TestMethod]
        public void RemoveDuplicatesBeforeSendingTest(){
            var s1 = Env.CurrentEnv.GetServerByID("S1");
            s1.Stats.SubscribeIn(5, new Response(2, null, null, ReqType.DoIt, null));
            s1.Stats.SubscribeAt(17, new Observer("S2"));
            s1.Stats.Subscribe(new Request("S2", "S1", ReqType.Asking));
            s1.Stats.SubscribeIn(7, new Response(2, null, null, ReqType.DoIt, null));
            s1.Stats.SubscribeIn(10, new Response(2, null, null, ReqType.DoIt, null));
            s1.Stats.Subscribe(new Observer("S2"));
            s1.Stats.Subscribe(new Observer("S2"));
            s1.Stats.SubscribeAt(17, new Observer("S2"));

            s1.HandlePerception(new Observer("S2"));
            
            Assert.AreEqual(4, Env.CurrentEnv.GetEventCount);
        }
        
    
    }
}
