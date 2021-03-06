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
        public string ServerId => serverID;
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
        List<(int, Perception)> _sendToEnv;
        Dictionary<string, object> _variables;

        List<Message> _messagingHistory;

        #endregion
   
        public Status(string iD, int parallels, ILogger<Status> logger = null)
        {
            _sendToEnv = new();
            _variables = new();

            MaxCapacity = parallels;
            AvailableResources = new();
            aceptedRequests = new();
            serverID = iD;

            _messagingHistory = new();
            _notCompletdRespns = new();

            _logger= logger;
        }

        public Status(string id, ILogger<Status> logger = null) : this(id, 5, logger) { }

        public void AddPartialRpnse(Response resp)
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

        public string LeaderId() {
            return MicroService.LeaderId;
        }

        public void ChangeLeader(string newLeaderId) {
            MicroService.ChangeLeader(newLeaderId);
        }

        /// <summary>
        /// Devuelve el ID de todos los servidores del microservicio, inclui'2 el servidor asocia2 a la instancia de esta clase.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetNeighbors() {
            return MicroService.GetServers();
        }

        public Request GetRequest()
        {
            return aceptedRequests.Peek();
        }

        public Perception GetRequestSentToEnv()
        {
            return _sendToEnv[_sendToEnv.Count-1].Item2;
        }

        internal int CountMessagingHistory()
        {
            return _messagingHistory.Count();
        }

        public void Reward(Response response, int delay) {
            MicroService.SetReward(response, delay);
        }

        public IEnumerable<string> Providers(Resource resource) {
            return MicroService.GetProviders(resource.Name);
        }

        public bool TryGetBio(string servId, out ServerBio bio) {
            try {
                bio = MicroService.GetBio(servId);
                return true;

            } catch (ArgumentException) {
                bio = null;
                return false;
            }
        }

        public void Penalize(ServerBio bio) {
            MicroService.ReduceReputation(bio);
        }

        public void PenalizeAll() {
            MicroService.LostRepInMicroS();
        }
    }
}


