using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        #region Worker
        public static Behavior Worker = new Behavior(WorkerBehav, WorkerBehavInit);

        private static string inProcessRequests_Worker = "inProcessRequests";

        private static void WorkerBehavInit(Status state, Dictionary<string, object> vars)
        {
            vars[inProcessRequests_Worker] = new Utils.Heap<Request>();
        }
        private static void WorkerBehav(Status st, Perception perce, Dictionary<string, object> vars)
        {
            // Checking Tasks Done
            var heap = vars[inProcessRequests_Worker] as Utils.Heap<Request>;

            while (heap.Count != 0 && heap.First.Item1 <= Env.Time)
            { // first elem is done

                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildResponse(st, req);

                if (Incomplete(st, response))
                { // if incomplete I save it for fill it later
                    st.AddPartialRpnse(response); // Add to dict
                }
                else
                {
                    st.Subscribe(response); // Subscribe response
                }
            }

            // Checking Tasks to do
            while (st.HasCapacity && st.HasRequests)
            {

                var req = st.ExtractAcceptedReq(); // elijo request
                var rtime = GetRequiredTimeToProcess(req);

                heap.Add(rtime, req); // comienzo a procesar la tarea
                st.Subscribe(Env.Time + rtime, new Observer(st.serverID)); // 
            }
        }

        internal static bool Incomplete(Status st, Response response)
        {
            var req = st.GetRequestById(response.ReqID);
            return response.AnswerRscs.Count < req.AskingRscs.Count;
        }

        private static int GetRequiredTimeToProcess(Request req)
        {// Returns the sum of all the required time to process the resources of the request
            var sum = 0;
            foreach (var r in req.AskingRscs)
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
                if (status.AvailableResources.Contains(item))
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
        private static bool IsAccepted(Status st, Request req)
        {
            return st.HasCapacity;
        }
        #endregion

        #region FalenLeader

        public static Behavior FalenLeader = new Behavior(FallenLeaderBehav,FallenLeaderInit);

        private static string timeFallenLeaderStr = "timePing";
        private static string countPingStr = "countPing";
        private static string maxPingStr = "maxPing";
        private static string initialPotenceStr = "initialPotence";


        private static void FallenLeaderInit(Status state, Dictionary<string, object> vars)
        {
            vars[initialPotenceStr] = 5;
            vars[maxPingStr] = 3;
            vars[timeFallenLeaderStr] = 3;
            vars[countPingStr] = 0;

        }

        public static void FallenLeaderBehav(Status st, Perception perce, Dictionary<string, object> vars)
        {
            int initP = (int)vars[initialPotenceStr];
            int countPing = (int)vars[countPingStr];
            int maxPing = (int)vars[maxPingStr];
            int timeFallen = (int)vars[timeFallenLeaderStr];

            if (perce is Response resp)
            {
                if (resp.Sender == st.MicroService.LeaderId)
                {
                    vars[timeFallenLeaderStr] = initialPotenceStr;
                    return;
                }

                if (Env.Time >= Math.Pow(2, timeFallen))
                {
                    if (countPing >= maxPing)
                    {
                        //cambiar para lider
                        //MicroService.ChangeLeader(req.Receiber);
                        return;
                    }
                    else
                    {
                        Request pingRequest = new Request(resp.Receiber, MicroService.LeaderId, RequestType.Ping);
                        vars[countPingStr] = countPing + 1;
                        Random r = new Random();
                        int minValue = (int)Math.Pow(2, (int)vars[timeFallenLeaderStr]);
                        vars[timeFallenLeaderStr] = (int)vars[timeFallenLeaderStr] + 1;
                        int maxValue = (int)Math.Pow(2, (int)vars[timeFallenLeaderStr]);
                        int time = r.Next(minValue, maxValue);

                        st.Subscribe(time, pingRequest);
                    }


                }
            }
            #endregion

        }
    }
}
