using System;
using System.Collections.Generic;

namespace Agents
{
    //to do
    public class DistributionServer:Agent{
        public List<string> workers {get;}
        public Func<List<string>, int> selectionProtocol{get;}
        public DistributionServer( Environment env, string ID, List<string> workers):base(env,ID){
            
            this.workers = workers;
            selectionProtocol = delegate (List<string> workers){
                Random r = new();
                return r.Next(workers.Count); 
            };
            this.functionsToHandleRequests.Add(AskToWorkers);

            this.functionsToHandleResponses.Add(AvailibityCheckerResponse);

        }
         
        private void AskToWorkers(IRequestable status, Request r){
            status.AddRequestToProcessed(r);
            
            environment.PrintAgent(this,$"LLega request desde {r.sender}.");

            var workerIndex = this.selectionProtocol(this.workers);
            Request req = new Request(this.ID,workers[workerIndex],environment,true);  // request de verificacion de disponibilidad
            
            status.AddEvent(this.environment.currentTime, req);
        }
        private void AvailibityCheckerResponse(IResponsable status, Response r){
            if(!r.ToKnowAvailibility)
                return;
                
            if(r.IsAvailable){
                //mandar el request original del cliente
                Request req = status.DeleteRequestAt(0);
                req.ChangeReciever(r.sender);
                status.AddEvent(this.environment.currentTime, req);
            }else{
                //seleccionar otro worker que este disponible
                var workerIndex = this.selectionProtocol(this.workers);
                Request req = new Request(this.ID,workers[workerIndex],environment,true);  // request de verificacion de disponibilidad
            
                status.AddEvent(this.environment.currentTime, req);
            }

        }

    }
}