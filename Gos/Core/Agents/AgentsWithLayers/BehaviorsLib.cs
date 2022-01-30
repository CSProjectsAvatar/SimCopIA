using System;
using System.Collections.Generic;
using System.Linq;
namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        #region Worker
        public static Behavior Worker = new Behavior(WorkerBehav, WorkerBehavInit);

        private static string inProcess_Worker = "inProcess";
        private static void WorkerBehavInit(Dictionary<string, object> vars){
            vars[inProcess_Worker] = new Utils.Heap<Request>();
        }
        private static void WorkerBehav(Status status, Perception perce, Dictionary<string, object> vars){
            // Checking Tasks Done
            var heap = vars[inProcess_Worker] as Utils.Heap<Request>;
            while(heap.First.Item1 <= Env.Time) { // first elem is done

                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildResponse(status, req);
                status.Subscribe(response); // Subscribe response
            }

            // Checking Tasks to do
            while(status.HasCapacity && status.aceptedRequests.Count != 0){
                
                var req = status.aceptedRequests.Dequeue(); // elijo request
                var rtime = GetRequiredTimeToProcess(req);
                
                heap.Add(rtime, req); // comienzo a procesar la tarea
                status.Subscribe(Env.Time + rtime, new Observer(status.serverID)); // 
            }
        }
        private static int GetRequiredTimeToProcess(Request req)
        {// Returns the max of all the required time to process the resources of the request
            var max = 0;
            foreach(var r in req.Asking)
                max = Math.Max(max, r.RequiredTime);
            return max;
        }

        #endregion

        #region Contractor
        public static Behavior Contractor = new Behavior(ContractorBehav);

        private static void ContractorBehav(Status status, Perception r, Dictionary<string, object> vars)
        {
            if (r is not Request)
                return;
            Request req = r as Request;

            switch (req.Type)
            {
                case RequestType.AskSomething when Accept(req):

                    Response response = BuildResponse(status, req);
                    status.Subscribe(response);
                    break;

                case RequestType.DoSomething:

                    status.aceptedRequests.Enqueue(req);
                    break;

                case RequestType.Ping:

                    status.aceptedRequests.Enqueue(req);
                    break;

                default:
                    break;
            }
        }

        // Builds a response to: asking, imperative and ping request; in the same way
        private static Response BuildResponse(Status status, Request req)
        {
            Dictionary<string, object> data = new();
            foreach (var item in req.Asking)
            {
                if (status.availableResources.Contains(item))
                {
                    data[item.Name] = true;
                    continue;
                }
                data[item.Name] = false;
            }
            var response = req.MakeResponse(data);
            return response;
        }
        // Accepts a request under certain conditions
        private  static bool Accept(Request req)
        {
            return true;
        }
        #endregion

        #region Boss

        public static Behavior BossAnnounceBehievor = new Behavior(BossAnnounce,BossAnnounceInit);

        private static void BossAnnounceInit(Dictionary<string, object> vars){
            vars["reviewTime"] = 5; //cambiar cualquier cosa 
        } 
        private static void BossAnnounce(Status status, Perception p,Dictionary<string,object> variables)
        {
            var request = p as Request;
            if (request == null)
                return;

            Dictionary<string, Request> server_Request = new Dictionary<string, Request>();

            //Asumamos que ya no estan aqui los recursos que puede solucionar el propio jefe...    
            foreach(var resource in request.Asking)
            {
                var servers = status.MicroService.Dir.YellowPages[resource.Name];
                foreach(var s in servers){
                    if(!server_Request.Keys.Contains(s)){
                        server_Request[s] = new Request(status.serverID,s,RequestType.AskSomething);
                        status.Subscribe(server_Request[s]);
                    }
                    server_Request[s].Asking.Add(resource);
                }
            }
            int reviewTime = (int)variables["reviewTime"];
            status.Subscribe(Env.Time + reviewTime ,new Observer(status.serverID));
        }



        #endregion
    }
} 
