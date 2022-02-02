using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Status{
        #region Server Properties
        public int MaxCapacity { get; private set; }
        internal MicroService MicroService;
        public string serverID;
        public List<Resource> AvailableResources;
        #endregion

        #region  Server State
        public IEnumerable<Request> AceptedRequests => aceptedRequests;
        public bool HasCapacity => aceptedRequests.Count < MaxCapacity;
        public bool HasRequests => aceptedRequests.Count > 0;

        Queue<Request> aceptedRequests;
        Dictionary<int,Response> _notCompletdRespns { get; set; }
        List<(int, Perception)> _sendToEnv;
        Dictionary<string, object> _variables;
        Dictionary<int, Request> _requestsAceptedHistory;
        #endregion
   
        public Status(string iD)
        {
            _sendToEnv = new();
            _variables = new();

            MaxCapacity = 5;
            AvailableResources = new();
            aceptedRequests = new();
            serverID = iD;
            _requestsAceptedHistory = new();
            _notCompletdRespns = new();
        }

        public void AddPartialRpnse(Response resp)
        {
            if (!_notCompletdRespns.ContainsKey(resp.ReqID)) // Si no esta lo agrego
                _notCompletdRespns.Add(resp.ReqID, resp);
            else{ // Si esta hago: actual U resp
                var actualRep = _notCompletdRespns[resp.ReqID];
                _notCompletdRespns[resp.ReqID] = Response.Union(actualRep, resp);
            }
            if (!BehaviorsLib.Incomplete(this, resp)) {
                this.Subscribe(resp);
                _notCompletdRespns.Remove(resp.ReqID);
            }
        }

        //Suscribe Perceptions en un tiempo 'time'
        public void SubscribeAt(int time, Perception p)
        {
            _sendToEnv.Add((time, p));
        }
        //Suscribe Perceptions dentro de un tiempo 'time'
        public void SubscribeIn(int time, Perception p)
        {
            _sendToEnv.Add((Env.Time + time, p));
        }
        //Suscribe Perceptions para ya
        public void Subscribe(Perception p) => SubscribeIn(0, p);

        public void AcceptReq(Request req)
        {
            aceptedRequests.Enqueue(req);
            _requestsAceptedHistory.Add(req.ID, req);
        }
        public Request ExtractAcceptedReq() => aceptedRequests.Dequeue();
        public Request GetRequestById(int id) => _requestsAceptedHistory[id];

        public void SetVariable(string name, object value) => _variables[name] = value;
        public object GetVariable(string name) => _variables.ContainsKey(name) ? _variables[name] : null;
        
        //Se llama cuando se recorrieron todas las capas, retorna un enumerable con todas las persepciones acumuladas de las capas y luego borra el historial de ellas.
        public IEnumerable<(int, Perception)> EnumerateAndClear() {
            foreach(var x in _sendToEnv){
                yield return x;
            }
            _sendToEnv.Clear();
        }
        
        public void SetMicroservice(MicroService ms){
            MicroService = ms;
        }
    }
}


