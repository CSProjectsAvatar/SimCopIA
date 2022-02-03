using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace ServersWithLayers.Behaviors
{
    public static class BossBehav
    {
        public static Behavior BossBehavior = new Behavior(BossAnnounce,BossAnnounceInit);
        private static string reviewTimeB = "reviewTime";
        private static string nextReviewB = "nextReview";
        private static string askResponsesB = "askResponses";
        private static string solutionResponsesAsocietedIDB = "solutionResponsesAsocietedID";

        private static void BossAnnounceInit(Status status,Dictionary<string, object> vars){
            vars[reviewTimeB] = 5; //cambiar cualquier cosa 
            vars[nextReviewB]  = new Utils.Heap<int>(); // la proxima revision a que request pertenece
            vars[askResponsesB] = new Dictionary<int, List<Response>>();
            vars[solutionResponsesAsocietedIDB] = new Dictionary<int, int>();
        } 
        private static void BossAnnounce(Status status, Perception p, Dictionary<string,object> variables)
        {
            var askResponses = variables[askResponsesB] as Dictionary<int,List<Response>>;
            var nextReview = variables[nextReviewB] as Utils.Heap<int>;
            var solutionResponsesAsocietedID  = variables[solutionResponsesAsocietedIDB] as Dictionary<int, int>;
            int reviewTime = (int)variables[reviewTimeB];


            switch (p) {
                case Request request when request.Type is ReqType.Asking:

                    BuildLeaderResponse(status, request);
                    break;

                case Request request when request.Type is ReqType.DoIt:

                    //buscamos los recursos que no puede solucionar este servidor.
                    ProcessDoItRequest(request, status, askResponses, nextReview, reviewTime);
                    break;

                case Response response when response.Type is ReqType.Asking:
                    if (askResponses.ContainsKey(response.ReqID))
                        askResponses[response.ReqID].Add(response);  //Agregamos a el request por el cual se mando...
                    break;

                case Response response when response.Type is ReqType.DoIt:

                    ProcessDoItResp(response, status, askResponses, solutionResponsesAsocietedID);
                    break;

                case Observer observer:

                    // (tiempo actual  != timepo de recogida de responses) <=> (observer no asociado a esta capa)
                    if( Env.Time != nextReview.First.Item1)
                        return;

                    (_,int current_request_ID) = nextReview.RemoveMin();

                    var responses =  askResponses[current_request_ID];

                    var requestsToDo = ResponseSelectionFunction(status,responses);

                    foreach(Request r in requestsToDo){
                        status.Subscribe(r);
                        solutionResponsesAsocietedID.Add(r.ID, current_request_ID);
                    }
                    askResponses.Remove(current_request_ID);

                    break;
            }
        }

        private static void ProcessDoItResp(Response response, Status status, Dictionary<int, List<Response>> askResponses, Dictionary<int, int> solutionResponsesAsocietedID)
        {
            if (solutionResponsesAsocietedID.Keys.Contains(response.ReqID))
            {
                var request_id = response.ReqID;

                //cambiamos el id del response que acaba de llegar para usarlo con AddPartialResponse
                response.Reassign(solutionResponsesAsocietedID[response.ReqID]);
                status.AddPartialRpnse(response);

                //quitamos el id del request asociado al response que acaba de llegar ya que este ha sido respondido.
                solutionResponsesAsocietedID.Remove(request_id);
            }
        }

        /// <summary>
        /// Procesa los Request de tipo DoIt, se encarga de repartir la carga de trabajo entre los servidores del microservicio
        /// </summary>
        private static void ProcessDoItRequest(Request request, Status status, Dictionary<int, List<Response>> askResponses, Heap<int> nextReview, int reviewTime)
        {
            var resourcesToFind = FilterNotAvailableRscs(status, request.AskingRscs);

            var server_Request = new Dictionary<string, Request>();

            askResponses.Add(request.ID, new List<Response>());   

            foreach (var resource in resourcesToFind)
            {
                var servers = status.MicroService.GetProviders(resource.Name);
                foreach (var s in servers)
                {

                    if (!server_Request.ContainsKey(s))
                    {

                        server_Request[s] = new Request(status.serverID, s, ReqType.Asking);
                        status.Subscribe(server_Request[s]);  //suscribimos para el evironment

                        //askResponses.Add(server_Request[s].ID, new List<Response>()); // error

                        //lista de los responses asosciados a un request originalmente hecho a este server
                        //askResponses.Add(request.ID, new List<Response>());   
                    }
                    server_Request[s].AskingRscs.Add(resource);   // agregamos a los recursos que se van a pedir a un server espesifico
                }
            }
            status.SubscribeIn(reviewTime, new Observer(status.serverID));
            nextReview.Add(Env.Time + reviewTime, request.ID);
        }
        

        internal static List<Resource> FilterNotAvailableRscs(Status status,List<Resource> resources){
            var availList = status.AvailableResources;
            var res = resources.Where(x => !availList.Contains(x)).ToList();

            return res;
        }

        private static List<Request> ResponseSelectionFunction(Status status,IEnumerable<Response> responses){
            //throw new NotImplementedException("IMPLEMENTAR FUNCION DE SELECCION EN BOSS");

            ////funcion por defecto momentaniamente.
            var sol = new List<Request>();
            foreach(var r in responses){
                Request req=new Request(status.serverID,r.Sender,ReqType.DoIt);
                sol.Add(req);
            }
            return sol;

        }

        // Builds a response AS A LEADER to: asking, imperative and ping request; in the same way
        // takes into account the available resources in the microservice
        private static Response BuildLeaderResponse(Status status, Request req)
        {
            // Gets the resources that are available in the Microservice 
            var availInMicro = status.MicroService.GetAllResourcesAvailable();

            Dictionary<string, bool> data = BehaviorsLib.GetAvailablesRscs(req, availInMicro);
            var response = req.MakeResponse(data);
            return response;
        }

        // No ejecutar los tests al mismo tiempo :D
        [TestClass]
        public class BossDoItRequestTests {
            #region  vars
            private Server s1;
            private Server s2;
            private Server s3;
            private Resource r1;
            private Resource r2;
            private Resource r3;
            
            #endregion
    
            [TestInitialize]
            public void Init() {                
                Env env = new Env();
            
                var bossLayer = new Layer();
                var loggerLayer = new Layer();
                bossLayer.behaviors = new List<Behavior>{BossBehavior};
                loggerLayer.behaviors = new List<Behavior>{LoggerBehav.LoggerBehavior};
    
                s1 = new Server("s1");
                s1.AddLayers(new List<Layer>{bossLayer,loggerLayer});
                s2 = new Server("s2");
                s2.AddLayers(new List<Layer>{loggerLayer});
                s3 = new Server("s3");
                s3.AddLayers(new List<Layer>{loggerLayer});
    
                s2.SetResources(new List<Resource>{
                    new Resource("img"),              
                    new Resource("index"),              
                    new Resource("random"),              
                });
    
                s3.SetResources(new List<Resource>{
                    new Resource("database"),              
                    new Resource("random2"),              
                });
    
                new Resource("gold"); //nadie lo tiene :D
                
    
                env.AddServerList(new List<Server>{s1,s2,s3});
    
    
            }
            
    
            //Envio de requests tipo DoIt (ejemplo super simple)

            [TestMethod]
            public void ProcessDoItRequestTest(){                
    
                Request req1= new Request("0", "s1", ReqType.DoIt);                                
                req1.AskingRscs = new List<Resource>{
                    Resource.Resources["img"],
                    Resource.Resources["index"]
                };
    
    
                Env.CurrentEnv.SubsribeEvent(0,req1);
                //Env.CurrentEnv.SubsribeEvent(0,req2);
                
                Env.CurrentEnv.Run();
                    
                var requestsS2 = LoggerBehav.GetRequestList(s2,0);
                var requestsS3 = LoggerBehav.GetRequestList(s3,0);

    
                Assert.AreEqual(1,requestsS2?.Count);
    
    
            }
            //Envio de requests tipo DoIt  (ejemplo super simple)
            [TestMethod]
            public void ProcessNDoItRequestTest(){                

                int n = 10;

                for(var i=1; i<= n;i++){
                    
                    Request req1= new Request("0", "s1", ReqType.DoIt);                                
                    if(i%3 == 0)
                        req1.AskingRscs = new List<Resource>{
                            Resource.Resources["img"],
                            Resource.Resources["index"]
                        };
                    else
                        req1.AskingRscs = new List<Resource>{
                            Resource.Resources["database"],
                        };      

                    Env.CurrentEnv.SubsribeEvent(i*10,req1);
                }
                    
                Env.CurrentEnv.Run();
                

                ////Imprimir en la terminal todos los eventos de llegada de cada servidor  :D 
                //System.Console.WriteLine("Llegada a S2:");
                //var logList = LoggerBehav.GetLogList(s2,0);
                //logList.AddRange(LoggerBehav.GetLogList(s3,0));
                //logList.AddRange(LoggerBehav.GetLogList(s1,1));
                //logList.Sort();
                //foreach(var s in logList)
                //    System.Console.WriteLine(s.Item2);


                var requestsS2 = LoggerBehav.GetRequestList(s2,0);
                var requestsS3 = LoggerBehav.GetRequestList(s3,0);
                
                Assert.AreEqual((int)n/3,requestsS2?.Count);
                Assert.AreEqual(n-(int)n/3,requestsS3?.Count);
            }
        }
    }    

} 
