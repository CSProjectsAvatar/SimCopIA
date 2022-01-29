using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public static class BehaviorsLib
    {
        public Behavior Worker;

        
        private static string inProcess_Worker = "inProcess";
       

       private static void WorkerBehavInit(Dictionary<string, object> vars){
           vars[inProcess_Worker] = new Utils.Heap<Request>();
       }

       private void WorkerBehav(Status status, Perception perce, Dictionary<string, object> vars){
            var heap = vars[inProcess_Worker] as Utils.Heap<Request>;
            while(heap.First.item1 <= )
            (int, Request) closest = heap.First;
            
            if(closest.Item1 <= Environment.CurrentEnvironment.time){ // ended < actual_time
                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildPositiveResponse(req);
                status.Subscribe(response);

            }
       }
    }
} 
