using System;
using System.Collections.Generic;
using System.Linq;

namespace Agents
{
    public interface IStatus {
        void SetAvailibility(bool b);
        bool IsAvailable {get;}
        void AddEvent(int time,IExecutable e);
        void DeleteRequest(Request r);
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
        public Status(){
            IsAvailable = true;
            toExecute = new();
            requests = new();
            requestToProcessed = new();
        }

        public List<Request> GetListRequest()
        {
            return this.requestToProcessed;
        }

        public void DeleteRequest(Request r)
        {
           // requestToProcessed.Remove(r);//aqui hay que redefinir el compare?

            this.requestToProcessed =  (from res in this.requestToProcessed 
                                        where r.ID != res.ID
                                        select res).ToList();
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