using System;
using System.Collections.Generic;

namespace Agents
{
    public class SimpleServer : Agent{ 
        public SimpleServer (Environment env, string ID): base(env, ID){
            this.functionsToHandleRequests.Add(this.AddRequest);
            this.functionsToHandleRequests.Add(this.GettingRequest);

            this.functionsToHandleStatus.Add(this.SendResponse);
        }
        private void AddRequest(IRequestable status, Request r){
            status.SaveRequest(r);
        }

        private void GettingRequest(IRequestable status, Request r){
            if(!status.IsAvailable){ 
                this.environment.PrintAgent(this,"No esta disponible "+this.ID);
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
            string sender = (o.Object as Request).sender;
            Response response = new(this,sender,environment,"Cosas de servidor simple.");
            response.SetTime(environment.currentTime);
            status.AddEvent(environment.currentTime,response);

        }
        private int TimeOffset() => new Random().Next(5,10);
    } 
}