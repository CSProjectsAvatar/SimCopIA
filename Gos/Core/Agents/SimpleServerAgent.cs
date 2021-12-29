using System;
using System.Collections.Generic;

namespace Agents
{
    public class SimpleServer : Agent{ 
        public int TotalRequests {get;}
        public SimpleServer (Environment env, string ID, int totalRequests = 1): base(env, ID){
            this.TotalRequests = totalRequests;

            this.functionsToHandleRequests.Add(this.GettingRequest);
            this.functionsToHandleRequests.Add(this.ProcessRequest);
             
            this.functionsToHandleStatus.Add(this.SendResponse);
            this.functionsToHandleStatus.Add(this.SetAvailableAfterSendResponse);
        }
        

        private void GettingRequest(IRequestable status, Request r){
            if(!status.IsAvailable){ 
                var res = new Response(r.ID,this,r.sender,this.environment,"Servidor no disponible");
                res.SetTime(this.environment.currentTime);
                status.AddEvent(this.environment.currentTime, res );
                this.environment.PrintAgent(this,"Llega requst pero no esta disponible.");
                return;
            }
            
            environment.PrintAgent(this,$"LLega request desde {r.sender}.");

                        
            Observer o = new Observer(this,environment, r);

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
            Response response = new(originalRequest.ID,this,originalRequest.sender,environment,$"Cosas de servidor simple {this.ID}.");
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
