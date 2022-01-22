using System;
using System.Collections.Generic;

namespace Agents
{
    public class Distributor:Agent{
        public List<string> workers {get;}
        public Func<List<string>, int> selectionProtocol{get;}
        public Distributor( Environment env, string ID, List<string> workers):base(env,ID){
            
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
            
            status.environment.PrintAgent(status.agent,$"LLega request desde {r.sender}.");
            var this_distributor = (status.agent as Distributor);
            
            var workerIndex = this_distributor.selectionProtocol(this_distributor.workers);
            Request req = new Request(status.agent.ID,workers[workerIndex],status.environment,true);  // request de verificacion de disponibilidad
            
            status.AddEvent(status.environment.currentTime, req);
        }
        private void AvailibityCheckerResponse(IResponsable status, Response r){
            if(!r.ToKnowAvailibility)
                return;
                
            if(r.IsAvailable){
                //mandar el request original del cliente
                Request req = status.DeleteRequestAt(0);
                req.ChangeReciever(r.sender);
                status.AddEvent(status.environment.currentTime, req);
            }else{
                //seleccionar otro worker que este disponible
                var this_distributor = (status.agent as Distributor);
                var workerIndex = this_distributor.selectionProtocol(this_distributor.workers);
                Request req = new Request(this_distributor.ID,workers[workerIndex],status.environment,true);  // request de verificacion de disponibilidad
            
                status.AddEvent(status.environment.currentTime, req);
            }

        }

    }
}