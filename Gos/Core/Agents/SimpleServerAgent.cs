using System;
using System.Collections.Generic;

namespace Agents
{
    public class SimpleServer : Agent{ 
        const int TOTAL_REQUEST = 1;
        public SimpleServer (Environment env, string ID): base(env, ID){
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
                this.environment.PrintAgent(this,"No esta disponible para nuevos request.");
                return;
            }
            
            environment.PrintAgent(this,"LLega paquete.");
                        
            Observer o = new Observer(this,environment, r);

            int time = environment.currentTime + TimeOffset();
            status.AddEvent(time,o);
        }
        private void ProcessRequest(IRequestable status, Request r)
        {
            if (status.IsAvailable)
            {
                status.AddRequestToProcessed(r);
                if (status.GetListRequest().Count >= TOTAL_REQUEST)
                    status.SetAvailibility(false);
            }
        }

        private void SendResponse(IObservable status,Observer o){
            Request originalRequest = (o.Object as Request);
            Response response = new(originalRequest.ID,this,originalRequest.sender,environment,"Cosas de servidor simple.");
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