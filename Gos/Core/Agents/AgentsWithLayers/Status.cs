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
        public string serverID;
        public List<Resource> AvailableResources;
        #endregion

        #region  Server State
        private int processedAtOnce;
        public IEnumerable<Request> AceptedRequests => aceptedRequests;
        public bool HasCapacity => processedAtOnce < MaxCapacity;
        internal bool HasRequests => aceptedRequests.Count > 0;

        Queue<Request> aceptedRequests;

        public void SaveEntry(Perception p)
        {
            if (p is Message msg)
                _messagingHistory.Add(msg);
        }

        Dictionary<int,Response> _notCompletdRespns { get; set; }
        internal List<(int, Perception)> _sendToEnv;
        Dictionary<string, object> _variables;

        //internal Dictionary<int, Request> _requestsAceptedHistory;

        List<Message> _messagingHistory;

        #endregion
   
        public Status(string iD, ILogger<Status> logger)
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

        public void AddPartialRpnse(Response resp)
        {
            if (!_notCompletdRespns.ContainsKey(resp.ReqID)) // Si no esta lo agrego
                _notCompletdRespns.Add(resp.ReqID, resp);
            else{ // Si esta hago: actual U resp
                _logger.LogDebug("Union de los responses obtenidos hasta el momento");
                var actualRep = _notCompletdRespns[resp.ReqID];
                _notCompletdRespns[resp.ReqID] = Response.Union(actualRep, resp);
            }
            var possbComplete = _notCompletdRespns[resp.ReqID];
            if (!BehaviorsLib.Incomplete(this, possbComplete)) {
                _logger.LogDebug("La respuesta está completada, subscribimos el evento");
                this.Subscribe(possbComplete);
                _notCompletdRespns.Remove(possbComplete.ReqID);
            }
        }

        public bool IncProcessing()
        {
            if (HasCapacity)
            {
                processedAtOnce++;
                return true;
            }
            return false;
        }
        public bool DecProcessing()
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
            _logger.LogDebug("Susbcribiendo el evento que se ejecutará en el tiempo {id}",time);
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
            _logger.LogInformation("Devuelve todos los eventos que se han subscrito en es Status del server {id} para subcribirlos en el Enviroment, se envia en evento y el tiempo en que tiene que subscribir y luego se eliminan del status", serverID);
            foreach(var x in _sendToEnv){
                yield return x;
            }
            _sendToEnv.Clear();
        }

        private void RemoveDuplicatesBeforeSending()
        {
            throw new NotImplementedException();
        }

        public void SetMicroservice(MicroService ms){
            _logger.LogDebug("setea el MicroServicio");
            MicroService = ms;
        }

        /// <summary>
        /// Se asegura que <paramref name="req"/> no este' en la cola.
        /// </summary>
        /// <param name="req"></param>
        public void EnsureExtractedFromAccepted(Request req) {
            var l = aceptedRequests.ToList();
            
            if (l.RemoveAll(r => r.ID == req.ID) > 1) {
                throw new Exception("Two requests with the same ID.");
            }
            aceptedRequests.Clear();
            l.ForEach(r => aceptedRequests.Enqueue(r));
        }
    }
}


