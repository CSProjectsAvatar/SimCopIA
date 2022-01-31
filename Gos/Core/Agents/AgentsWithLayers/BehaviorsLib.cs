using System;
using System.Collections.Generic;
using System.Linq;
namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        #region Worker
        public static Behavior Worker = new Behavior(WorkerBehav, WorkerBehavInit);

        private static string inProcessRequests_Worker = "inProcessRequests";

        private static void WorkerBehavInit(Status state, Dictionary<string, object> vars){
            vars[inProcessRequests_Worker] = new Utils.Heap<Request>();
        }
        private static void WorkerBehav(Status st, Perception perce, Dictionary<string, object> vars){
            // Checking Tasks Done
            var heap = vars[inProcessRequests_Worker] as Utils.Heap<Request>;

            while(heap.Count != 0 && heap.First.Item1 <= Env.Time) { // first elem is done

                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildResponse(st, req);

                if(Incomplete(st, response)){ // if incomplete I save it for fill it later
                    st.AddPartialRpnse(response); // Add to dict
                }
                else{
                    st.Subscribe(response); // Subscribe response
                }
            }

            // Checking Tasks to do
            while(st.HasCapacity && st.HasRequests){
                
                var req = st.ExtractAcceptedReq(); // elijo request
                var rtime = GetRequiredTimeToProcess(req);
                
                heap.Add(rtime, req); // comienzo a procesar la tarea
                st.Subscribe(Env.Time + rtime, new Observer(st.serverID)); // 
            }
        }

        private static bool Incomplete(Status st, Response response)
        {
            var req = st.GetRequestById(response.ReqID);
            return response.AnswerRscs.Count < req.AskingRscs.Count;
        }

        private static int GetRequiredTimeToProcess(Request req)
        {// Returns the sum of all the required time to process the resources of the request
            var sum = 0;
            foreach(var r in req.AskingRscs)
                sum += r.RequiredTime;
            return sum;
        }

        #endregion

        #region Contractor
        public static Behavior Contractor = new Behavior(ContractorBehav);

        private static void ContractorBehav(Status state, Perception r, Dictionary<string, object> vars)
        {
            if (r is not Request)
                return;
            Request req = r as Request;

            switch (req.Type)
            {
                case RequestType.AskSomething when IsAccepted(state, req):

                    Response response = BuildResponse(state, req);
                    state.Subscribe(response);
                    break;

                case RequestType.DoSomething or RequestType.Ping:

                    state.AcceptReq(req);
                    break;

                default:
                    break;
            }
        }

        // Builds a response to: asking, imperative and ping request; in the same way
        private static Response BuildResponse(Status status, Request req)
        {
            Dictionary<string, bool> data = new();
            foreach (var item in req.AskingRscs)
            {
                if (status.availableResources.Contains(item))
                {
                    data[item.Name] = true;
                    continue;
                }
                // data[item.Name] = false; // Comentado para no estorbar con el Jefe recibiendo recursos que no tiene
            }
            var response = req.MakeResponse(data);
            return response;
        }
        // Accepts a request under certain conditions
        private  static bool IsAccepted(Status st, Request req)
        {
            return st.HasCapacity;
        }
        #endregion

        #region Boss
        private static string reviewTimeB = "reviewTime";
        private static string nextReviewB = "nextReview";
        private static string askResponsesB = "askResponses";
        public static Behavior BossAnnounceBehievor = new Behavior(BossAnnounce,BossAnnounceInit);

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
                    var resourcesToFind = FilterInvalidResources(status,request.AskingRscs);

                    Dictionary<string, Request> server_Request = new Dictionary<string, Request>();

                    //Asumamos que ya no estan aqui los recursos que puede solucionar el propio jefe...    
                    foreach(var resource in resourcesToFind)
                    {
                        var servers = status.MicroService.GetProviders(resource.Name);

                        foreach(var s in servers){
                            if(!server_Request.Keys.Contains(s)){
                                server_Request[s] = new Request(status.serverID, s, RequestType.AskSomething);
                                status.Subscribe(server_Request[s]);  //suscribimos para el evironment
                                askResponses.Add(server_Request[s].ID,new List<Response>());
                            }
                            server_Request[s].AskingRscs.Add(resource);   // agregamos a los recursos que se van a pedir a un server espesifico
                        }
                    }
                    status.Subscribe(Env.Time + reviewTime ,new Observer(status.serverID));
                    nextReview.Add(Env.Time + reviewTime,request.ID);
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
        static IEnumerable<Resource> FilterInvalidResources(Status status,List<Resource> resources){
            return (
                from rec in resources
                where status.availableResources.Exists(r => r.Name != rec.Name)
                select rec 
            );
        }
        public static Behavior BossWorkBehievor = new Behavior(BossWork) ;
        private static void BossWork(Status status, Perception p,Dictionary<string,object> variables){
           //filtar los requerimientos del request.... que llega   
        }
        #endregion
    }
} 
