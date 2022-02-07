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
        private static string decrementRewardsTimeB = "decrementRewardsTime";

        private static void BossAnnounceInit(Status status,Dictionary<string, object> vars){
            vars[reviewTimeB] = 5; //cambiar cualquier cosa 
            vars[nextReviewB]  = new Utils.Heap<Request>(); // la proxima revision a que request pertenece
            vars[askResponsesB] = new Dictionary<int, List<Response>>();
            vars[askResponsesAsocietedIDB] = new Dictionary<int, int>();
            vars[solutionResponseAsocietedRequestB] = new Dictionary<int, (Request, int)>(); //de reqID --> Original Request
            vars[decrementRewardsTimeB] = (500, false); //   (dalay de reduccion, ya esta inicializado)
        } 
        private static void BossAnnounce(Status status, Perception p, Dictionary<string,object> variables)
        {
            var askResponses = variables[askResponsesB] as Dictionary<int,List<Response>>;
            var nextReview = variables[nextReviewB] as Utils.Heap<Request>;
            var solutionResponseAsocietedRequest  = variables[solutionResponseAsocietedRequestB] as Dictionary<int, (Request,int)>; // reqID -> (OriginalRequest, tiempoDeSalida)
            var askResponsesAsocietedID  = variables[askResponsesAsocietedIDB] as Dictionary<int, int>;
            int reviewTime = (int)variables[reviewTimeB];
            (int,bool) decrementRewardsTime = ((int,bool))variables[decrementRewardsTimeB];

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

                    // (tiempo actual  == timepo de recogida de responses) <=> (observer asociado a esta capa)
                    if( nextReview.Count!=0 && Env.Time == nextReview.First.Item1){
                        (int requestExitTime,Request currentOriginalRequest) = nextReview.RemoveMin();

                        var responses =  askResponses[currentOriginalRequest.ID];
                        //status.MicroService.SetReward(responses);
                        var requestsToDo = ResponseSelectionFunction(status,responses);

                        foreach(Request r in requestsToDo){
                            status.Subscribe(r);
                            solutionResponseAsocietedRequest.Add(r.ID, (currentOriginalRequest,requestExitTime));
                        }
                        askResponses.Remove(currentOriginalRequest.ID);
                    }

                    if( status.MicroService.LeaderId == status.serverID && Env.Time % decrementRewardsTime.Item1 == 0 ){
                        status.MicroService.LostRepInMicroS();
                        status.SubscribeAt(Env.Time + decrementRewardsTime.Item1,new Observer(status.serverID));
                    }

                    if (status.MicroService.LeaderId == status.serverID && !decrementRewardsTime.Item2 ){
                        int n = (int)(Env.Time/decrementRewardsTime.Item1);
                        status.SubscribeAt((n+1)*decrementRewardsTime.Item1,new Observer(status.serverID));
                        variables[decrementRewardsTimeB] = (decrementRewardsTime.Item1, true);
                    }

                    break;
            }
        }

        private static void ProcessDoItResp(Response response, Status status, Dictionary<int, List<Response>> askResponses, Dictionary<int, (Request,int)> solutionResponseAsocietedRequest)
        {
            if (solutionResponseAsocietedRequest.Keys.Contains(response.ReqID))
            {
                var request_id = response.ReqID;
                (var originalRequest, int exitTime)=solutionResponseAsocietedRequest[response.ReqID];

                // dar recompensa en base al tiempo de salida del response
                status.MicroService.SetReward(response, exitTime);
                
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
            List<Request> solution = new();

            // resource is Ready 
            HashSet<string> readyResources = new();

            foreach(Response res in status.MicroService.SortByCredibility(responses)){
                Request toSend = new Request(status.serverID,res.Sender,ReqType.DoIt);
                List<Resource> resourcesToSend =new List<Resource>();
                //Agregamos los recursos validos del response al request
                foreach(string resource in res.AnswerRscs.Keys)
                    if(res.AnswerRscs[resource] && !readyResources.Contains(resource)){
                        resourcesToSend.Add(Resource.Resources[resource]);
                        readyResources.Add(resource);
                    }
                //Si tiene algun recurso que mandar, se manda
                if( resourcesToSend.Count != 0 ){
                    toSend.AskingRscs = resourcesToSend;
                    solution.Add(toSend);
                }
            }

            return solution;

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
    
                new Resource("img")   ;           
                new Resource("index") ;             
                new Resource("random");
                new Resource("database");              
                new Resource("gold");

                s2.SetResources(new List<Resource>{
                    Resource.Resources["index"],
                    Resource.Resources["img"] ,
                    Resource.Resources["random"],
                });
    
                s3.SetResources(new List<Resource>{
                    Resource.Resources["database"],
                    Resource.Resources["img"],
                });
    
    
                Env env = new Env();
                env.AddServerList(new List<Server>{s1,s2,s3});
    
    
            }
            [TestCleanup]
            
            public void Clean()
            {
                MicroService.Services.Clear();
                Resource.Resources.Clear();
                Env.ClearServersLayers();
            }

            //Envio de requests tipo DoIt (ejemplo super simple)

            [TestMethod]
            public void ProcessDoItRequestTest(){
   
                Request req1= new Request("0", "s1", ReqType.DoIt);                                
                req1.AskingRscs = new List<Resource>{
                    Resource.Resources["img"],
                    Resource.Resources["index"],
                    Resource.Resources["database"],
                    //Resource.Resources["gold"], // si se pone el response nunca deberia de llegar
                };


                Env.CurrentEnv.SubsribeEvent(0,req1);
                
                Env.CurrentEnv.Run();
                 
                //LoggerBehav.PrintResponses(s1,0);

                //IEnumerable<(int,string)> logList =Env.CurrentEnv.GetAllServersLogs() ;
      
                //System.Console.WriteLine("EVENTOS:");
                //foreach(var s in logList)
                //    System.Console.WriteLine(s.Item2);
      
                //LoggerBehav.PrintRequests(s2,0);
                //LoggerBehav.PrintRequests(s3,0);

               //var responsesS1 = LoggerBehav.GetResponseList(s1,0);
               //var requestsS2 = LoggerBehav.GetRequestList(s2,0);
               //var requestsS3 = LoggerBehav.GetResponseList(s3,0);
                
               //Assert.AreEqual(2,requestsS2?.Count);
               //Assert.AreEqual(2,requestsS2?.Count);
               //Assert.AreEqual(4,responsesS1?.Count);
               Assert.AreEqual(1,Env.CurrentEnv.GetClientResponses().Count());

            }
            //Envio de ciclo completo de varios request tipo DoIt donde no se solapan los recursos  (ejemplo super simple)
            [TestMethod]
            public void ProcessNDoItRequestTest(){                

                int n = 100;

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
    
                    Env.CurrentEnv.SubsribeEvent(i*20,req1);
                }
                    
                Env.CurrentEnv.Run();
                

                //Imprimir en la terminal todos los eventos de llegada de cada servidor  :D 
               //System.Console.WriteLine("\nEventos de llegada:");
               //var logList = LoggerBehav.GetLogList(s2,0);
               //logList.AddRange(LoggerBehav.GetLogList(s3,0));
               //logList.AddRange(LoggerBehav.GetLogList(s1,0));
               //logList.AddRange(Env.CurrentEnv.GetClientReciveLog());
//
               //logList.Sort();
               //foreach(var s in logList)
               //    System.Console.WriteLine(s.Item2);


                //var responsesS1 = LoggerBehav.GetResponseList(s1,0);
                var requestsS2 = LoggerBehav.GetRequestList(s2,0);
                var requestsS3 = LoggerBehav.GetRequestList(s3,0);
                
               //Assert.AreEqual(2*(int)n/3 ,requestsS2?.Count);
               //Assert.AreEqual(2*(n-(int)(n/3)) + (int)n/3 ,requestsS3?.Count);
                Assert.AreEqual(n, Env.CurrentEnv.GetClientResponses().Count());
            }

            [TestMethod]
            public void TestAskRequest(){
                Request req1= new Request("0", "s1", ReqType.Asking);                                
                req1.AskingRscs = new List<Resource>{
                    Resource.Resources["img"],
                    Resource.Resources["index"],
                    Resource.Resources["database"],
                   // Resource.Resources["gold"], 
                };                    
                Request req2= new Request("0", "s1", ReqType.Asking);                                
                req2.AskingRscs = new List<Resource>{
                    Resource.Resources["img"],
                    Resource.Resources["index"],
                    Resource.Resources["database"],
                    Resource.Resources["gold"],   
                };       

                Env.CurrentEnv.SubsribeEvent(10,req1);
                Env.CurrentEnv.SubsribeEvent(20,req2);

                Env.CurrentEnv.Run();
                 
                var solution_responses = Env.CurrentEnv.GetClientResponses();

                Assert.AreEqual(2,solution_responses.Count());
                Assert.AreEqual(3,solution_responses.ToList()[0].Item2.AnswerRscs.Count);
                Assert.AreEqual(3,solution_responses.ToList()[1].Item2.AnswerRscs.Count);

                //var sortedEvents = Env.CurrentEnv.GetAllServersLogs().ToList();
//
                //System.Console.WriteLine("Logs:");
                //sortedEvents.Sort();
                //foreach (var item in sortedEvents)
                //    System.Console.WriteLine(item.Item2);             
//
                //System.Console.WriteLine("Responses:");
                //foreach(var s in Env.CurrentEnv.solutionResponses){
                //    foreach(var k in s.AnswerRscs.Keys)
                //        System.Console.WriteLine("  "+k+": "+s.AnswerRscs[k]);
                //    System.Console.WriteLine();
                //}
                
            }
            [TestMethod]
            public void testingggg(){
                Env.RunDefaultCurrentEnv() ;
                var output=Env.CurrentEnv.Output;

                System.Console.WriteLine(output.Average);
                System.Console.WriteLine(output.ResponsePercent);
                System.Console.WriteLine(output.NotResponsePercent);
                System.Console.WriteLine(output.SlowestResponse);
                System.Console.WriteLine(output.FastestResponse);
                System.Console.WriteLine(output.TotalMonthlyCost);
            }
        }
    }    

} 
