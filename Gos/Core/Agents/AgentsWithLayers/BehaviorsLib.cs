using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        #region Worker
        public static Behavior Worker = new Behavior(WorkerBehav, WorkerBehavInit);

        private static string inProcessRequests_Worker = "inProcessRequests";
        private static string respInProcess_Worker = "respInProcess";

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
    }
} 
