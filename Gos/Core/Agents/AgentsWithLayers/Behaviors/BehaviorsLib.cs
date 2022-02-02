using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        public static Behavior Worker = new Behavior(WorkerBehav.Behavior, WorkerBehav.BehavInit);
        public static Behavior Contractor = new Behavior(ContractorBehav.Behavior);
        public static Behavior FallenLeader = new Behavior(FallenLeaderBehav.Behavior);
      
        public static bool Incomplete(Status st, Response response)
        {
            var req = st.GetRequestById(response.ReqID);
            return response.AnswerRscs.Count < req.AskingRscs.Count;
        }

        public static int GetRequiredTimeToProcess(Request req)
        {// Returns the sum of all the required time to process the resources of the request
            var sum = 0;
            foreach (var r in req.AskingRscs)
                sum += r.RequiredTime;
            return sum;
        }

        // Builds a response to: asking, imperative and ping request; in the same way
        public static Response BuildResponse(Status status, Request req)
        {
            Dictionary<string, bool> data = GetAvailablesRscs(req, status.AvailableResources);
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
                // data[item.Name] = false; // Comentado para no estorbar con el Jefe recibiendo recursos que no tiene
            }
            return data;
        }

    }

}