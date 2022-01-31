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
                    askResponses[response.ReqID].Add(response);  //Agregamos a el request por el cual se mando...
                    break;
                
                case Observer:
                    var observer = p as Observer;

                    (_,int current_request_ID) = nextReview.RemoveMin();
                    var responses =  askResponses[current_request_ID];
                    Func<Status, List<Response>, List<Response>> selectionFunction = (Status status, List<Response> listResponses) => listResponses;
                    var selected_servers = selectionFunction(status,responses);

                    //
                    //  Pedir Recursos  :D
                    //

                    askResponses.Remove(current_request_ID);

                    break;

            }
        }
        internal static IEnumerable<Resource> FilterNotAvailableRscs(Status status,List<Resource> resources){
            var availList = status.availableResources;
            var res = resources.Where(x => !availList.Contains(x)).ToList();

            return res;
        }
        public static Behavior BossWorkBehievor = new Behavior(BossWork) ;
        private static void BossWork(Status status, Perception p,Dictionary<string,object> variables){
           //filtar los requerimientos del request.... que llega   
        }
    }

    [TestClass]
    public class BossAuxiliarMethodsTests {

        [TestInitialize]
        public void Init() {
            var worker = BehaviorsLib.Worker;
            var server1 = new Server("S1");
            var server2 = new Server("S2");
            
            // server2.AddLayer();
            var r1 = new Resource("img1");
            var r2 = new Resource("img2");
            var r3 = new Resource("index");

            var p1 = new Request("S1", "S2", RequestType.AskSomething);
            p1.AskingRscs.AddRange(new[] { r1 });

            var p2 = new Request("S1", "S2", RequestType.AskSomething);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            var p3 = new Request("S1", "S2", RequestType.AskSomething);
            p3.AskingRscs.AddRange(new[] { r1, r2, r3 });
        }
        
      
        [TestMethod]
        public void FilterResourcesTest() {
            

            server2.Stats.availableResources.AddRange(new[] { r1, r2 });



            server2.HandlePerception(p1);


            // worker.Run(server1.Stats, p);
        }
      

    }

} 
