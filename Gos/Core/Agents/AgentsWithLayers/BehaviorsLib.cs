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
       private static void WorkerBehav(Status status, Perception perce, Dictionary<string, object> vars){
            var heap = vars[inProcessStr] as Utils.Heap<Request>;
            (int, Request) closest = heap.First; 
            
            if(closest.Item1 <= Environment.CurrentEnvironment.time){ // ended < actual_time
                var req = heap.RemoveMin().Item2; // Request completed
                var response = BuildPositiveResponse(req);
                status.Subscribe(response);

            }
       }

        private static void ArrivesRequest(Status status, Perception r)
        {
            Dictionary<Resource, object> data = new Dictionary<Resource, object> { };
            if (r is not Request)
                return;
            Request req = r as Request;
            if (req.Type == RequestType.AskSomething)
            {
                bool acepted = true; //Aceptation(); // esto es si el worker lo puede o lo quiere acaptar por ahora pongamoslo en true
                if (acepted)
                {
                    foreach (var item in req.Asking)
                    {
                        if (status.availableResources.Contains(item))
                        {
                            data[item] = true;
                            continue;
                        }
                        data[item] = false;
                    }
                    var response = req.MakeResponse(data);//aqui da error pq es dic es de resorce,object y no de string
                    status.Subscribe(response);
                }
            }
            else if (req.Type == RequestType.DoSomething)
            {
                status.acepted_requests.Add(req);
            }
            else
            {
                var response = req.MakeResponse(data);//aqui da error pq es dic es de resorce,object y no de string
                status.Subscribe(response);
            }

        }

        private  static bool Aceptation(string item)
        {
            throw new NotImplementedException();
        }
    }
} 
