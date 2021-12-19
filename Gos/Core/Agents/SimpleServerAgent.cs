using System;
using System.Collections.Generic;

namespace Agents
{
    public class SimpleServer : Agent{ 
        public SimpleServer (Environment env, string ID): base(env, ID){
            this.functionsToHandleRequests.Add(this.AddRequest);
            this.functionsToHandleRequests.Add(this.GettingRequest);
            this.functionsToHandleRequests.Add(this.ProcessRequest);
            this.functionsToHandleRequests.Add(this.SatisfiedRequest);
             
            this.functionsToHandleStatus.Add(this.SendResponse);
        }
        private void AddRequest(IRequestable status, Request r){
            status.SaveRequest(r);
        }
        private void ProcessRequest(IRequestable status, Request r)
        {
            if (status.IsAvailable)
            {
                status.AddRequestToProcessed(r);
                if (status.GetListRequest().Count > 15)//configurable
                    status.SetAvailibility(false);
            }
                
            //si no buscamos otro para enviarlo o se pierde
        }

        private void SatisfiedRequest(IRequestable status, Request r)
        {
            if (r.satisfied)
            {
                status.DeleteRequest(r);
                if (!status.IsAvailable)
                    status.SetAvailibility(true);
            } 
           
        }

        private void GettingRequest(IRequestable status, Request r){
            if(!status.IsAvailable){ 
                this.environment.PrintAgent(this,"No esta disponible para nuevos request.");
                return;
            }
            
            environment.PrintAgent(this,"LLega paquete.");
                        
            Observer o = new Observer(this,environment, r);
            status.SetAvailibility(false);
            int time = environment.currentTime + TimeOffset();
            status.AddEvent(time,o);
        }

        private void SendResponse(IObservable status,Observer o){
            status.SetAvailibility(true);
            Request originalRequest = (o.Object as Request);
            Response response = new(originalRequest.ID,this,originalRequest.sender,environment,"Cosas de servidor simple.");
            response.SetTime(environment.currentTime);
            status.AddEvent(environment.currentTime,response);
        }
        private int TimeOffset() => new Random().Next(5,10);
    } 
}