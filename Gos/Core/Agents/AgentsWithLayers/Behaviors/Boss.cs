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
        private static string solutionResponseAsocietedRequestB = "solutionResponseAsocietedRequest";
        private static string askResponsesAsocietedIDB = "askResponsesAsocietedID";

        private static void BossAnnounceInit(Status status,Dictionary<string, object> vars){
            vars[reviewTimeB] = 5; //cambiar cualquier cosa 
            vars[nextReviewB]  = new Utils.Heap<Request>(); // la proxima revision a que request pertenece
            vars[askResponsesB] = new Dictionary<int, List<Response>>();
            vars[askResponsesAsocietedIDB] = new Dictionary<int, int>();
            vars[solutionResponseAsocietedRequestB] = new Dictionary<int, Request>(); //de reqID --> Original Request
        } 
        private static void BossAnnounce(Status status, Perception p, Dictionary<string,object> variables)
        {
            var askResponses = variables[askResponsesB] as Dictionary<int,List<Response>>;
            var nextReview = variables[nextReviewB] as Utils.Heap<Request>;
            var solutionResponseAsocietedRequest  = variables[solutionResponseAsocietedRequestB] as Dictionary<int, Request>;
            var askResponsesAsocietedID  = variables[askResponsesAsocietedIDB] as Dictionary<int, int>;
            int reviewTime = (int)variables[reviewTimeB];


            switch (p) {
                case Request req when req.Type is ReqType.Asking 
                            && BehaviorsLib.IsAccepted(status, req):

                    var resp = BuildLeaderResponse(status, req);
                    status.Subscribe(resp);
                    break;

                case Request request when request.Type is ReqType.DoIt:

                    //buscamos los recursos que no puede solucionar este servidor.
                    ProcessDoItRequest(request, status, askResponses,askResponsesAsocietedID, nextReview, reviewTime);
                    break;

                case Response response when response.Type is ReqType.Asking:
                    if(!askResponsesAsocietedID.ContainsKey(response.ReqID)) 
                        return;
                    if (askResponses.ContainsKey(askResponsesAsocietedID[response.ReqID])){
                        askResponses[askResponsesAsocietedID[response.ReqID]].Add(response);  //Agregamos a el request por el cual se mando...
                    }
                    break;

                case Response response when response.Type is ReqType.DoIt:

                    ProcessDoItResp(response, status, askResponses, solutionResponseAsocietedRequest);
                    break;

                case Observer observer:

                    // (tiempo actual  != timepo de recogida de responses) <=> (observer no asociado a esta capa)
                    if( Env.Time != nextReview.First.Item1)
                        return;
                    
                    (_,Request currentOriginalRequest) = nextReview.RemoveMin();

                    var responses =  askResponses[currentOriginalRequest.ID];

                    var requestsToDo = ResponseSelectionFunction(status,responses);

                    foreach(Request r in requestsToDo){
                        status.Subscribe(r);
                        solutionResponseAsocietedRequest.Add(r.ID, currentOriginalRequest);
                    }
                    askResponses.Remove(currentOriginalRequest.ID);

                    break;
            }
        }

        private static void ProcessDoItResp(Response response, Status status, Dictionary<int, List<Response>> askResponses, Dictionary<int, Request> solutionResponseAsocietedRequest)
        {
            if (solutionResponseAsocietedRequest.Keys.Contains(response.ReqID))
            {
                var request_id = response.ReqID;
                var originalRequest=solutionResponseAsocietedRequest[response.ReqID];

                ///// Esta sucesion de pasos comentados me llevan a un error logico probablemente asociado con referencias
                ///// tuve que crear un Response nuevo y entonces llamar a AddPartialResponse para que funcionara
                ///// En estos pasos comentados debe haber algun bug que no logro encontrar ahora mismo :D

                //cambiamos el id del response que acaba de llegar para usarlo con AddPartialResponse
               // response.Reassign(originalRequest.ID);
                //cambiamos el sender y el recieber
               // response.ReassignDirections(status.serverID, originalRequest.Sender);

                Response solutionResponse = new Response(originalRequest.ID,status.serverID,originalRequest.Sender,ReqType.DoIt,response.AnswerRscs);
                status.AddPartialRpnse(solutionResponse);

                //quitamos el id del request asociado al response que acaba de llegar ya que este ha sido respondido.
                solutionResponseAsocietedRequest.Remove(request_id);
            }
        }

        /// <summary>
        /// Procesa los Request de tipo DoIt, se encarga de repartir la carga de trabajo entre los servidores del microservicio
        /// </summary>
        private static void ProcessDoItRequest(Request request, Status status, Dictionary<int, List<Response>> askResponses,Dictionary<int,int> askResponseAsocietedID, Heap<Request> nextReview, int reviewTime)
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

                        askResponseAsocietedID.Add(server_Request[s].ID,request.ID);
                    }
                    server_Request[s].AskingRscs.Add(resource);   // agregamos a los recursos que se van a pedir a un server especifico
                }
            }
            status.SubscribeIn(reviewTime, new Observer(status.serverID));
            nextReview.Add(Env.Time + reviewTime, request);
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

                var li = (from res in r.AnswerRscs.Keys
                            select Resource.Resources[res]).ToList();
                //System.Console.WriteLine(li.Count);
                if(li.Count == 0)
                    continue;

                Request req= new Request(status.serverID,r.Sender,ReqType.DoIt);
                req.AskingRscs = li;

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
                var contractorLayer = new Layer();
                var workerLayer = new Layer();
                bossLayer.behaviors = new List<Behavior>{BossBehavior};
                loggerLayer.behaviors = new List<Behavior>{LoggerBehav.LoggerBehavior};
                contractorLayer.behaviors = new List<Behavior>{BehaviorsLib.Contractor};
                workerLayer.behaviors = new List<Behavior>{BehaviorsLib.Worker};

                s1 = new Server("s1");
                s1.AddLayers(new List<Layer>{loggerLayer,bossLayer});
                s2 = new Server("s2");
                s2.AddLayers(new List<Layer>{loggerLayer,contractorLayer,workerLayer});
                s3 = new Server("s3");
                s3.AddLayers(new List<Layer>{loggerLayer,contractorLayer,workerLayer});
    
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
                    Resource.Resources["index"],
                    Resource.Resources["database"],
                   // Resource.Resources["gold"],   // de ser asi el request no llega al cliente.
                };


                Env.CurrentEnv.SubsribeEvent(0,req1);
                
                Env.CurrentEnv.Run();
                 
                LoggerBehav.PrintResponses(s1,0);

                List<(int,string)> logList =new();

                var logList1 = LoggerBehav.GetLogList(s1,0);
                var logList2 = LoggerBehav.GetLogList(s2,0);
                var logList3 = LoggerBehav.GetLogList(s3,0);

                logList.AddRange(logList1);
                logList.AddRange(logList2); 
                logList.AddRange(logList3);
                logList.AddRange(Env.CurrentEnv.GetClientReciveLog());
                
                logList.Sort();
                System.Console.WriteLine("EVENTOS:");
                foreach(var s in logList)
                    System.Console.WriteLine(s.Item2);

                var responsesS1 = LoggerBehav.GetResponseList(s1,0);
                var requestsS2 = LoggerBehav.GetRequestList(s2,0);
                var requestsS3 = LoggerBehav.GetResponseList(s3,0);
                
                Assert.AreEqual(2,requestsS2?.Count);
                Assert.AreEqual(2,requestsS2?.Count);
                Assert.AreEqual(4,responsesS1?.Count);
                Assert.AreEqual(1,Env.CurrentEnv.GetClientResponses().Count());

            }
            //Envio de ciclo completo de varios request tipo DoIt donde no se solapan los recursos  (ejemplo super simple)
            [TestMethod]
            public void ProcessNDoItRequestTest(){                

                int n = 10;

                for(var i=1; i<= n;i++){
                    
                    Request req1= new Request("0", "s1", ReqType.DoIt);                                
                    if(i%3 == 0)
                        req1.AskingRscs = new List<Resource>{
                            Resource.Resources["img"],
                            Resource.Resources["index"],
                        };
                    else
                        req1.AskingRscs = new List<Resource>{
                            Resource.Resources["database"],
                        };      
    
                    Env.CurrentEnv.SubsribeEvent(i*10,req1);
                }
                    
                Env.CurrentEnv.Run();
                

                //Imprimir en la terminal todos los eventos de llegada de cada servidor  :D 
                System.Console.WriteLine("\nEventos de llegada:");
                var logList = LoggerBehav.GetLogList(s2,0);
                logList.AddRange(LoggerBehav.GetLogList(s3,0));
                logList.AddRange(LoggerBehav.GetLogList(s1,0));
                logList.AddRange(Env.CurrentEnv.GetClientReciveLog());

                logList.Sort();
                foreach(var s in logList)
                    System.Console.WriteLine(s.Item2);


                var responsesS1 = LoggerBehav.GetResponseList(s1,0);
                var requestsS2 = LoggerBehav.GetRequestList(s2,0);
                var requestsS3 = LoggerBehav.GetRequestList(s3,0);
                
                Assert.AreEqual(2*(int)n/3 ,requestsS2?.Count);
                Assert.AreEqual(2*(n-(int)n/3),requestsS3?.Count);
                Assert.AreEqual(2*n,responsesS1?.Count);
                Assert.AreEqual(n,Env.CurrentEnv.GetClientResponses().Count());
            }
        }
    }    

} 
