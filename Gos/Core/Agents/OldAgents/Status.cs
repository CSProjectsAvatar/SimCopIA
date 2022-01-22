using System;
using System.Collections.Generic;
using System.Linq;

namespace Agents
{
    public interface IStatus {
        void SetAvailibility(bool b);
        bool IsAvailable {get;}
        void AddEvent(int time,IExecutable e);
        Request DeleteRequest(Request r);
        Request DeleteRequestAt(int index);
        //Environment env {get;}
        Agent agent {get;}
        Environment environment {get;}
    }
    public interface IRequestable: IStatus{
        void SaveRequest(Request r);
        void AddRequestToProcessed(Request r); 
        List<Request> GetListRequest();
    }
    public interface IResponsable : IStatus{
        void SaveResponse(Response r);
    }
    public interface IObservable : IStatus{
        
    }
    public class Status : IStatus, IRequestable, IResponsable, IObservable{
        List<(int,IExecutable)> toExecute;
        public bool IsAvailable {get; private set;}
        public List<Request> requests  {get; private set;}
        public List<Response> responses  {get; private set;}
        public List<Request> requestToProcessed { get; private set; }

        public Environment environment => this.agent.environment;
        public Agent agent {get;}
        public Status(Agent agent){
            IsAvailable = true;
            toExecute = new();
            requests = new();
            requestToProcessed = new();
            //this.env = env;
            this.agent = agent;
        }

        public List<Request> GetListRequest()
        {
            return this.requestToProcessed;
        }

        public Request DeleteRequest(Request r)
        {
           // requestToProcessed.Remove(r);//aqui hay que redefinir el compare?

            Request sol  = default; //ver si esto no da problemas
            for(int i=0; i<this.requestToProcessed.Count;i++){
                if(r.ID == this.requestToProcessed[i].ID){
                    sol = this.requestToProcessed[i];
                    this.requestToProcessed.RemoveAt(i);
                    break;    
                }
            }
            return sol;
        }
        public Request DeleteRequestAt(int index){
            var sol = this.requestToProcessed[index];
            this.requestToProcessed.RemoveAt(index);
            return sol;
        }

        public void AddEvent(int time, IExecutable e)
        {
            toExecute.Add((time, e));
        }
        public void AddRequestToProcessed(Request r)
        {
            requestToProcessed.Add(r);
        }
        public IEnumerable<(int,IExecutable)> EnumerateAndClear() {
            foreach(var x in toExecute){
                yield return x;
            }
            toExecute.Clear();
        }
        public void SetAvailibility(bool b){
            this.IsAvailable = b;
        }
        public void SaveRequest(Request r){
            this.requests.Add(r);
        }
        public void SaveResponse(Response r){
            this.responses.Add(r);
        }
    } 
}