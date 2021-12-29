using System;
using System.Collections.Generic;

namespace Agents
{
    public class Worker : Agent{ 
        public int TotalRequests {get;}
        public Worker (Environment env, string ID, int totalRequests = 1): base(env, ID){
            this.TotalRequests = totalRequests;

            this.functionsToHandleRequests.Add(this.GettingRequest);
            this.functionsToHandleRequests.Add(this.ProcessRequest);
            
            this.functionsToHandleStatus.Add(this.SendResponse);
            this.functionsToHandleStatus.Add(this.SetAvailableAfterSendResponse);
        }

       // public Worker (Environment env, string ID, List<Action<...> functions, int totalRequests = 1): base(env, ID, functions){

        

        private void GettingRequest(IRequestable status, Request r){
            if(!status.IsAvailable){ 
                
                var res = new Response(r.ID,status.agent,r.sender,status.environment,"Servidor no disponible");
                res.SetTime(status.environment.currentTime);
                status.AddEvent(status.environment.currentTime, res );
                status.environment.PrintAgent(status.agent,"Llega requst pero no esta disponible.");
                return;
            }
            
            environment.PrintAgent(status.agent,$"LLega request desde {r.sender}.");

                        
            Observer o = new Observer(status.agent,status.environment, r);

            int time = environment.currentTime + TimeOffset();
            status.AddEvent(time,o);
        }
        private void ProcessRequest(IRequestable status, Request r)
        {
            if (status.IsAvailable)
            {
                status.AddRequestToProcessed(r);
                if (status.GetListRequest().Count >= TotalRequests)
                    status.SetAvailibility(false);
            }
        }

        private void SendResponse(IObservable status,Observer o){
            Request originalRequest = (o.Object as Request);
            Response response = new(originalRequest.ID,status.agent,originalRequest.sender,status.environment,$"Cosas de servidor simple {status.agent.ID}.");
            response.SetTime(environment.currentTime);
            status.AddEvent(environment.currentTime,response);
        }
        private void SetAvailableAfterSendResponse(IObservable status, Observer o){
            Request originalRequest = (o.Object as Request);
            status.DeleteRequest(originalRequest);            
            status.SetAvailibility(true);
        }
        private int TimeOffset() => new Random().Next(5,10);
    } 
}
