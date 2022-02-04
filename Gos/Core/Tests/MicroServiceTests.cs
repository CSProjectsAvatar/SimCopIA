using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;
using Utils;
namespace Core {

    [TestClass]
    public class MicroServiceTests {
        
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

            s1.Stats.AvailableResources = new List<Resource> { r1 };
            s2.Stats.AvailableResources = new List<Resource> { r1, r2 };
            s3.Stats.AvailableResources = new List<Resource> { r1, r2, r3 };
            s4.Stats.AvailableResources = new List<Resource> { r4, r5 };
            s5.Stats.AvailableResources = new List<Resource> { r6, r7 };
            s6.Stats.AvailableResources = new List<Resource> { r6, r7 };

            var servers = new List<Server> { s1, s2, s3, s4, s7, s8 };

            s4.SetMService(mPagos.Name);
            s5.SetMService(mMulta.Name);
            s6.SetMService(mMulta.Name);
            env.AddServerList(servers);
        }
        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
        }
        
        [TestMethod]
        public void CheckGetProvidersLocal(){
            var pImg = MicroService.Services["Main"].GetProviders("img1");
            Assert.IsTrue(pImg.Count == 3);
            Assert.IsTrue(pImg.Contains("S1"));
            Assert.IsTrue(pImg.Contains("S2"));
            Assert.IsTrue(pImg.Contains("S3"));

            var pAbout = MicroService.Services["Main"].GetProviders("about.html");
            Assert.IsTrue(pAbout.Count == 1);
            Assert.IsTrue(pAbout.Contains("S3"));
        }
        
        [TestMethod]
        public void CheckGetProvidersGlobal(){
            var pImg = MicroService.Services["API-Pagos"].GetProviders("img1");
            Assert.IsTrue(pImg.Count == 1);
            Assert.IsTrue(pImg.Contains("S1"));

            var pTable = MicroService.Services["Main"].GetProviders("table1");
            Assert.IsTrue(pTable.Count == 1);
            Assert.IsTrue(pTable.Contains("S4"));

            var pIndex = MicroService.Services["Main"].GetProviders("index.html");
            Assert.IsTrue(pIndex.Count == 1);
            Assert.IsTrue(pIndex.Contains("S5"));
        }
        
        [TestMethod]
        public void SetRewardTest(){
            List<Response> list = new List<Response>(){ 
                new Response(5, "S7", "S2", ReqType.DoIt, new Dictionary<string, bool>()),
                new Response(2, "S3", "S2", ReqType.DoIt, new Dictionary<string, bool>()),
                new Response(4, "S1", "S2", ReqType.DoIt, new Dictionary<string, bool>()),
                new Response(3, "S8", "S2", ReqType.DoIt, new Dictionary<string, bool>()),
             };
            var main = MicroService.Services["Main"];

            var s1Bio = main.GetBio("S1");
            var s2Bio = main.GetBio("S2");
            var s3Bio = main.GetBio("S3");
            var s7Bio = main.GetBio("S7");
            var s8Bio = main.GetBio("S8");

            Assert.AreEqual(s1Bio.Reputation, s2Bio.Reputation);
            Assert.AreEqual(s1Bio.Reputation, s3Bio.Reputation);
            Assert.AreEqual(s1Bio.Reputation, s7Bio.Reputation);
            Assert.AreEqual(s1Bio.Reputation, s8Bio.Reputation);

            main.SetReward(list);
            Assert.IsTrue(s7Bio.Reputation > s3Bio.Reputation);
            Assert.IsTrue(s3Bio.Reputation > s1Bio.Reputation);
            Assert.IsTrue(s1Bio.Reputation > s8Bio.Reputation);

            list.RemoveAt(0);
            main.SetReward(list);
            Assert.IsTrue(s3Bio.Reputation > s7Bio.Reputation);
        }

    }
}
