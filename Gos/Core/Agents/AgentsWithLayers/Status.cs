using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers{
    public class Status{
        #region Server Properties
        public int MaxCapacity { get; private set; }
        private ILogger<Status> _logger;

        internal MicroService MicroService;
        internal string serverID;
        internal List<Resource> AvailableResources;
        #endregion

        #region  Server State
        private int processedAtOnce;
        internal bool HasCapacity => processedAtOnce < MaxCapacity;
        internal bool HasRequests => aceptedRequests.Count > 0;

        Queue<Request> aceptedRequests;

        internal void SaveEntry(Perception p)
        {
            if (p is Message msg)
                _messagingHistory.Add(msg);
        }

        Dictionary<int,Response> _notCompletdRespns { get; set; }
        List<(int, Perception)> _sendToEnv;
        Dictionary<string, object> _variables;

        List<Message> _messagingHistory;

        #endregion
   
        public Status(string iD, ILogger<Status> logger = null)
        {
            _sendToEnv = new();
            _variables = new();

            MaxCapacity = 5;
            AvailableResources = new();
            aceptedRequests = new();
            serverID = iD;

            _messagingHistory = new();
            _notCompletdRespns = new();

            _logger= logger;
        }

        internal void AddPartialRpnse(Response resp)
        {
            if (!_notCompletdRespns.ContainsKey(resp.ReqID)) // Si no esta lo agrego
                _notCompletdRespns.Add(resp.ReqID, resp);
            else{ // Si esta hago: actual U resp
                _logger?.LogDebug("Union de los responses a {reqID}, de {Sender1} con {Sender2}", resp.ReqID, _notCompletdRespns[resp.ReqID].Sender, resp.Sender);
                var actualRep = _notCompletdRespns[resp.ReqID];
                _notCompletdRespns[resp.ReqID] = Response.Union(actualRep, resp);
            }
            var possbComplete = _notCompletdRespns[resp.ReqID];
            if (!BehaviorsLib.Incomplete(this, possbComplete)) {
                _logger?.LogDebug("Se completo la respuesta a {reqID}", resp.ReqID);
                this.Subscribe(possbComplete);
                _notCompletdRespns.Remove(possbComplete.ReqID);
            }
        }

        internal bool IncProcessing()
        {
            if (HasCapacity)
            {
                processedAtOnce++;
                return true;
            }
            return false;
        }
        internal bool DecProcessing()
        {
            if (processedAtOnce > 0)
            {
                processedAtOnce--;
                return true;
            }
            return false;
        }



        //Suscribe Perceptions en un tiempo 'time'
        public void SubscribeAt(int time, Perception p)
        {
            _logger?.LogDebug("Susbcribiendo percepcion de tipo {type} para el tiempo {id}", p.GetType().Name, time);
            _sendToEnv.Add((time, p));
        }
        //Suscribe Perceptions dentro de un tiempo 'time'
        public void SubscribeIn(int time, Perception p) => SubscribeAt(Env.Time + time, p);
        //Suscribe Perceptions para ya
        public void Subscribe(Perception p) => SubscribeIn(0, p);

        public void AcceptReq(Request req)
        {
            if (req.Type is not ReqType.DoIt)
                throw new ArgumentException("Only DoIt requests are accepted for processing later", nameof(req));
            aceptedRequests.Enqueue(req);
        }
        public Request ExtractAcceptedReq() => aceptedRequests.Dequeue();
        public List<Message> GetMsgBySender(string sender)
            => _messagingHistory.Where(m => m.Sender == sender).ToList();
        public Message GetMsgById(int id) => _messagingHistory.First(m => m.ID == id);
        public void SetVariable(string name, object value) => _variables[name] = value;
        public object GetVariable(string name) => _variables.ContainsKey(name) ? _variables[name] : null;
        
        //Se llama cuando se recorrieron todas las capas, retorna un enumerable con todas las persepciones acumuladas de las capas y luego borra el historial de ellas.
        public IEnumerable<(int, Perception)> EnumerateAndClear() {
            RemoveDuplicatesBeforeSending();
            foreach(var x in _sendToEnv){
                _logger?.LogInformation("Enviando {type} hacia {receiver} desde {this}", x.Item2.GetType().Name, x.Item2, serverID);
                yield return x;
            }
            _sendToEnv.Clear();
        }

        private void RemoveDuplicatesBeforeSending()
        {   // Allow the first respond to the same request, it delete the rest 
            List<int> seens = new();
            // Allow only one Observer in a time t
            List<int> times = new();

            for (int i = 0; i < _sendToEnv.Count; i++) {
                var p = _sendToEnv[i];
                if (p.Item2 is Response resp){ // Response
                    if (seens.Contains(resp.ReqID)) {
                        _sendToEnv.RemoveAt(i);
                        i--;
                    }
                    else
                        seens.Add(resp.ReqID);
                }
                else if (p.Item2 is Observer obs){ // Observer
                    if (times.Contains(p.Item1)){
                        _sendToEnv.RemoveAt(i);
                        i--;
                    }
                    else
                        times.Add(p.Item1);
                }
            }
        }

        public void SetMicroservice(MicroService ms){
            _logger?.LogDebug("Asignado el server {this} al MicroServicio {ms}", serverID, ms.Name);
            MicroService = ms;
        }

        public Request GetRequest()
        {
            return aceptedRequests.Peek();
        }

        public Perception GetRequestSentToEnv()
        {
            return _sendToEnv[_sendToEnv.Count-1].Item2;
        }
    }
}


