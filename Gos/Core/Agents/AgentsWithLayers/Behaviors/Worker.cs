using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{

    public static class WorkerBehav
    {
        private static string inProcessRequests_Worker = "inProcessRequests";
        internal static void BehavInit(Status state, Dictionary<string, object> vars)
        {
            vars[inProcessRequests_Worker] = new Utils.Heap<Request>();
        }
        internal static void Behavior(Status st, Perception perce, Dictionary<string, object> vars)
        {
            // Checking Tasks Done
            var heap = vars[inProcessRequests_Worker] as Utils.Heap<Request>;

            while (heap.Count != 0 && heap.First.Item1 <= Env.Time)
            { // first elem is done
                st.DecProcessing();
                
                var req = heap.RemoveMin().Item2; // Request completed
                var response = BehaviorsLib.BuildResponse(st, req);

                if (BehaviorsLib.Incomplete(st, response))
                { // if incomplete I save it for fill it later
                    st.AddPartialRpnse(response); // Save response
                } else {
                    st.Subscribe(response); // Subscribe response
                }
            }

            // Checking Tasks to do
            while (st.HasCapacity && st.HasRequests)
            {
                st.IncProcessing(); 

                var req = st.ExtractAcceptedReq(); // elijo request
                var rtime = BehaviorsLib.GetRequiredTimeToProcess(req);

                heap.Add(Env.Time + rtime, req); // comienzo a procesar la tarea
                st.SubscribeIn(rtime, new Observer(st.serverID)); // 
            }
        }
    
    }

}