using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers.Behaviors
{
    public static class BossBehav
    {
        private static string reviewTimeB = "reviewTime";
        private static string nextReviewB = "nextReview";
        private static string askResponsesB = "askResponses";

        private static void BossAnnounceInit(Status status,Dictionary<string, object> vars){
            vars[reviewTimeB] = 5; //cambiar cualquier cosa 
            vars[nextReviewB]  = new Utils.Heap<int>(); // la proxima revision a que request pertenece
            vars[askResponsesB] = new Dictionary<int, List<Response>>();
        } 
        private static void BossAnnounce(Status status, Perception p, Dictionary<string,object> variables)
        {
            var askResponses = variables[askResponsesB] as Dictionary<int,List<Response>>;
            var nextReview = variables[nextReviewB] as Utils.Heap<int>;
            int reviewTime = (int)variables[reviewTimeB];

            switch (p) {
                case Request:

                    var request = p as Request;
                    var resourcesToFind = FilterNotAvailableRscs(status,request.AskingRscs);

                    var server_Request = new Dictionary<string, Request>();

                    //Asumamos que ya no estan aqui los recursos que puede solucionar el propio jefe...    
                    foreach(var resource in resourcesToFind)
                    {
                        var servers = status.MicroService.GetProviders(resource.Name);
                        foreach(var s in servers){

                            if(!server_Request.ContainsKey(s)){
                                    
                                server_Request[s] = new Request(status.serverID, s, RequestType.AskSomething);
                                status.Subscribe(server_Request[s]);  //suscribimos para el evironment
                                askResponses.Add(server_Request[s].ID,new List<Response>());
                            }
                            server_Request[s].AskingRscs.Add(resource);   // agregamos a los recursos que se van a pedir a un server espesifico
                        }
                    }
                    status.Subscribe(Env.Time + reviewTime, new Observer(status.serverID));
                    nextReview.Add(Env.Time + reviewTime, request.ID);
                    break;
                
                case Response :
                    var response = p as Response;

                    if (askResponses.Keys.Contains(response.ReqID))
                        askResponses[response.ReqID].Add(response);  //Agregamos a el request por el cual se mando...
                        
                    break;
                
                case Observer:
                    var observer = p as Observer;

                    (_,int current_request_ID) = nextReview.RemoveMin();
                    var responses =  askResponses[current_request_ID];

                    var selected_servers = ResponseSelectionFunction(status,responses);

                    //
                    //  Pedir Recursos  :D
                    //
                        
                    foreach(var r in selected_servers)
                        status.Subscribe(r);

                    //
                    //
                    //

                    askResponses.Remove(current_request_ID);

                    break;

            }
        }
        internal static List<Resource> FilterNotAvailableRscs(Status status,List<Resource> resources){
            var availList = status.AvailableResources;
            var res = resources.Where(x => !availList.Contains(x)).ToList();

            return res;
        }

        private static List<Request> ResponseSelectionFunction(Status status,IEnumerable<Response> responses){
            throw new NotImplementedException("IMPLEMENTAR FUNCION DE SELECCION EN BOSS");
        }
    }

    [TestClass]
    public class BossAuxiliarMethodsTests {
        #region  vars
        private Server s1;
        private Server s2;
        private Resource r1;
        private Resource r2;
        private Resource r3;
        private Request p1;
        private Request p2;
        private Request p3;
        #endregion

        [TestInitialize]
        public void Init() {
            s1 = new Server("S1");
            s2 = new Server("S2");
            
            // server2.AddLayer();
            r1 = new Resource("img1");
            r2 = new Resource("img2");
            r3 = new Resource("index");

            p1 = new Request("S1", "S2", RequestType.AskSomething);
            p1.AskingRscs.AddRange(new[] { r1 });

            p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            p3 = new Request("S1", "S2", RequestType.AskSomething);
            p3.AskingRscs.AddRange(new[] { r1, r2, r3 });
        }
        
      
        [TestMethod]
        public void FilterResourcesTest() {
            s2.SetResources(new[] { r1, r2 });
            var resList = BossBehav.FilterNotAvailableRscs(s2.Stats, p3.AskingRscs);

            Assert.AreEqual(1, resList.Count);
            Assert.AreEqual(r3, resList[0]);

            s1.SetResources(new[] { r2 });
            resList = BossBehav.FilterNotAvailableRscs(s1.Stats, p3.AskingRscs);

            Assert.AreEqual(2, resList.Count);
            Assert.IsTrue(resList.Contains(r1));
            Assert.IsTrue(resList.Contains(r3));
        }
      

    }

} 
