using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        private static string inProcessStr = "inProcess";
       

       private static void WorkerBehavInit(Dictionary<string, object> vars){
           vars[inProcessStr] = new Utils.Heap<Request>();
       }
       private void WorkerBehav(Status status, Perception perce, Dictionary<string, object> vars){
            var heap = vars[inProcessStr] as Utils.Heap<Request>;
            (int, Request) closest = heap.First; 
            
            if(closest.Item1 <= Environment.CurrentEnvironment.time){ // ended < actual_time
                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildPositiveResponse(req);
                status.Subscribe(response);

            }
       }
    }
} 
