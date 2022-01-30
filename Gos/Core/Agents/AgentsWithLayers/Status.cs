using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Status{
        public int MaxCapacity { get; private set; }

        internal List<Resource> availableResources;
        internal Queue<Request> aceptedRequests;
        internal bool HasCapacity => aceptedRequests.Count < MaxCapacity;
        internal string serverID;
        internal MicroService MicroService;
        List<(int, Perception)> _sendToEnv;
        Dictionary<string, object> _variables;
   
        public Status(string iD)
        {
            _sendToEnv = new();
            _variables = new();

            MaxCapacity = 5;
            availableResources = new();
            aceptedRequests = new();
            serverID = iD;
        }
        //Suscribe Perceptions en un tiempo 'time' en '_sendToEnv'.
        public void Subscribe(int time, Perception p)
        {
            _sendToEnv.Add((time, p));
        }
        public void Subscribe(Perception p) => Subscribe(Env.Time, p);

        //Se llama cuando se recorrieron todas las capas, retorna un enumerable con todas las persepciones acumuladas de las capas y luego borra el historial de ellas.
        public IEnumerable<(int, Perception)> EnumerateAndClear() {
            foreach(var x in _sendToEnv){
                yield return x;
            }
            _sendToEnv.Clear();
        }
        
        public void SetMicroservice(string microserviceID){
            this.MicroService = MicroService.Get(microserviceID);
        }
    }
}


