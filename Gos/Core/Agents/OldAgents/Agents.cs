using System;
using System.Collections.Generic;

namespace Agents{

    public class Agent  {
        public string ID{get;}
        public Environment environment {get;}
        protected List<Action<IRequestable,Request>> functionsToHandleRequests; 
        protected List<Action<IResponsable, Response>> functionsToHandleResponses; 
        protected List<Action<IObservable, Observer>> functionsToHandleStatus;
        public Status status;

        public Agent(Environment e, string ID){
            this.environment = e;    
            this.ID = ID;
            this.functionsToHandleRequests = new();
            this.functionsToHandleResponses = new();
            this.functionsToHandleStatus = new();
            this.status=new Status(this);
            this.environment.PrintAgent(this,"Agente creado!!");
        }
        public void HandleRequest(Request r){
            var status = this.status;
            status.SaveRequest(r);

            if(r.ToKnowAvailibility){ //para saber si es un request para conocer si un server esta disponible
                Response res = new Response(r.ID, this, r.sender, this.environment,status.IsAvailable);
                environment.SubsribeEvent(res,this.environment.currentTime);
                environment.PrintAgent(this,$"LLega request de disponibilidad desde {r.sender}. Respondiendo con {res.IsAvailable}."); // debug
                return;
            }

            foreach(var f in functionsToHandleRequests){
                f(status,r);
            }
            foreach(var e in status.EnumerateAndClear()){
                environment.SubsribeEvent(e.Item2,e.Item1);
            }
            
        }
        public void HandleResponse(Response r){
            var status = this.status; 
           
            foreach(var f in functionsToHandleResponses){
                f(status,r);
            }
            foreach(var e in status.EnumerateAndClear()){
                environment.SubsribeEvent(e.Item2,e.Item1);
            }
        }
        public virtual void HandleStatus(Observer o){
            var status = this.status; 
            
            foreach(var f in functionsToHandleStatus){
                f(status, o);
            }
            foreach(var e in status.EnumerateAndClear()){
                environment.SubsribeEvent(e.Item2,e.Item1);
            }
        }


        //Para agregar Handlers...
        public void AddRequestHandlerFunction(Action<IRequestable,Request> fun){
            this.functionsToHandleRequests.Add(fun);
        }
        public void AddResponseHandlerFunction(Action<IResponsable,Response> fun){
            this.functionsToHandleResponses.Add(fun);
        }
        public void AddRequestHandlerFunction(Action<IObservable,Observer> fun){
            this.functionsToHandleStatus.Add(fun);
        }
    }
} 