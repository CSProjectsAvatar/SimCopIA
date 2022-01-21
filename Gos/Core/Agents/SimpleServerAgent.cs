using System;
using System.Collections.Generic;

namespace Agents
{
    public class Worker : Agent{ 
        public int TotalRequests {get;}
        public Worker (Environment env, string ID, int totalRequests = 1): base(env, ID){
            this.TotalRequests = totalRequests;

            this.functionsToHandleRequests.Add(PredefinedFunctions.Workers.WorkerGettingRequest);
            this.functionsToHandleRequests.Add(PredefinedFunctions.Workers.WorkerProcessRequest);
            
            this.functionsToHandleStatus.Add(PredefinedFunctions.Workers.WorkerSendResponse);
            this.functionsToHandleStatus.Add(PredefinedFunctions.Workers.WorkerSetAvailableAfterSendResponse);
        }

       // public Worker (Environment env, string ID, List<Action<...> functions, int totalRequests = 1): base(env, ID, functions){

       
        public static int TimeOffset() => new Random().Next(5,10);
    } 
}
