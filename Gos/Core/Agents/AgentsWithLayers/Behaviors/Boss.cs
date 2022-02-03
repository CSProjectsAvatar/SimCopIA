using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utils;

namespace ServersWithLayers.Behaviors
{
    public static class BossBehav
    {
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
                case Request req when req.Type is ReqType.Asking 
                            && BehaviorsLib.IsAccepted(status, req):

                    var resp = BuildLeaderResponse(status, req);
                    status.Subscribe(resp);
                    break;

                case Request request when request.Type is ReqType.DoIt:

                    //buscamos los recursos que no puede solucionar este servidor.
                    ProcessDoItRequest(request, status, askResponses, nextReview, reviewTime);
                    break;

                case Response response when response.Type is ReqType.Asking:

                    if (askResponses.ContainsKey(response.ReqID)) // @warning mira ver que kieras hacer esto
                        askResponses[response.ReqID].Add(response);  //Agregamos a el request por el cual se mando...
                    break;

                case Response response when response.Type is ReqType.DoIt:

                    ProcessDoItResp(response, status, askResponses, solutionResponsesAsocietedID);
                    break;

                case Observer observer:

                    (_,int current_request_ID) = nextReview.RemoveMin();

                    var responses =  askResponses[current_request_ID];

                    var finalResponses = ResponseSelectionFunction(status,responses);

                    foreach(var r in finalResponses){
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

            foreach (var resource in resourcesToFind)
            {
                var servers = status.MicroService.GetProviders(resource.Name);
                foreach (var s in servers)
                {
                    if (!server_Request.ContainsKey(s))
                    {
                        server_Request[s] = new Request(status.serverID, s, ReqType.Asking);
                        status.Subscribe(server_Request[s]);  //suscribimos para el evironment
                        askResponses.Add(server_Request[s].ID, new List<Response>());
                    }
                    server_Request[s].AskingRscs.Add(resource);   // agregamos a los recursos que se van a pedir a un server especifico
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
            throw new NotImplementedException("IMPLEMENTAR FUNCION DE SELECCION EN BOSS");
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

            p1 = new Request("S1", "S2", ReqType.Asking);
            p1.AskingRscs.AddRange(new[] { r1 });

            p2 = new Request("S1", "S2", ReqType.Asking);
            p2.AskingRscs.AddRange(new[] { r1, r2 });

            p3 = new Request("S1", "S2", ReqType.Asking);
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
