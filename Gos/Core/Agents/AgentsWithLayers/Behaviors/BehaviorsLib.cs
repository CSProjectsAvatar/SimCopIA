using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        public static Behavior Worker = new Behavior("worker", WorkerBehav.Behavior, WorkerBehav.BehavInit);
        public static Behavior Contractor = new Behavior("contractor",ContractorBehav.Behavior);
        public static Behavior FallenLeader = new Behavior("Fallenleader",FallenLeaderBehav.Behavior, FallenLeaderBehav.BehavInit);
        public static Behavior Boss = new Behavior("Boss",BossBehav.BossBehavior, BossBehav.BossBehavInit);

        
        
        public static bool Incomplete(Status st, Response response)
        {
            var req = st.GetMsgById(response.ReqID) as Request;
            return response.AnswerRscs.Count < req.AskingRscs.Count;
        }

        public static int GetRequiredTimeToProcess(Request req)
        {// Returns the sum of all the required time to process the resources of the request
            var sum = 0;
            foreach (var r in req.AskingRscs)
                sum += r.RequiredTime;

            sum += (int)Utils.UtilsT.GenTimeOffset(lambda: 0.2); // Random Noise, prom: 4
            return sum;
        }

        // Builds a response to: asking, imperative and ping request; in the same way
        public static Response BuildResponse(Status status, Request req)
        {
            Dictionary<string, bool> data = new();
            
            if (req.Type is not ReqType.Ping)
                data = GetAvailablesRscs(req, status.AvailableResources);

            var response = req.MakeResponse(data);
            return response;
        }
        
        internal static Dictionary<string, bool> GetAvailablesRscs(Request req, List<Resource> availableResources)
        {
            Dictionary<string, bool> data = new();
            foreach (var item in req.AskingRscs)
            {
                if (availableResources.Contains(item))
                {
                    data[item.Name] = true;
                    continue;
                }
                //data[item.Name] = false; // Comentado para no estorbar con el Jefe recibiendo recursos que no tiene
            }
            return data;
        }

        // Returns true if the request is accepted and encoled
        internal static bool IsAccepted(Status st, Request req)
        {
            return true;
        }

        internal static List<Request> CreatePingRequests (Status st)
        {
            List<string> servers = st.MicroService.GetServers();

            List<Request> requests = new();
            foreach (var item in servers) {
                requests.Add(new Request(st.serverID, item, ReqType.Ping));
            }
            return requests;
        }

    }

}