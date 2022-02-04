using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers{
    public class MicroService{
        internal static Dictionary<string, MicroService> Services = new();
        private string _name;
        internal ServiceType Type {get; private set;}
        public string Name {get => _name; 
            set {
                if (Services.ContainsKey(value))
                    throw new Exception("MicroService already exists");
                Services.Add(value, this);
                _name = value;
            }
        }   
        internal string LeaderId { get; set; }
        private Directory Dir { get; set; }
        public MicroService(string name){
            this.Name = name;
            this.Dir = new Directory();
        }

        internal void SetAsEntryPoint(){
            this.Type = ServiceType.EntryPoint;
        }
        internal static MicroService GetMS(string microserviceID)
        {
            if (!Services.ContainsKey(microserviceID))
                throw new Exception("MicroService doesn't exists");
            return Services[microserviceID];
        }

        internal static void AddServer(Server server, string microS)
        {
            if (!Services.ContainsKey(microS))
                throw new Exception("MicroService doesn't exists");

            var ms = Services[microS];
            if (ms.LeaderId is null) // if it's the first server to be added to the microservice
                ms.ChangeLeader(server.ID); // set the leader to the server 
                
            server.Stats.SetMicroservice(ms);
            Services[microS].Dir.AddServer(server);
        }

        ///<summary>
        /// Asigna una recompensa en reputacion a todos los servidores que dieron respuestas
        /// </summary>
        public void SetReward(List<Response> responses)
        {
            var servers = responses.Select(r => r.Sender).Distinct();
            var it = DecresingPercents();
            foreach (var server in servers) {
                it.MoveNext();
                AddRep(server, it.Current);
            }
        }
        private IEnumerator<double> DecresingPercents(double init = 0.2, double next = 0.8){
            while(true){
                yield return init;
                init *= next;
            }
        }
        private void AddRep(string server, double percent)
        {
            if(percent > 1) throw new ArgumentException("Percent has to be less than 1", nameof(percent));
            var bio = Dir.WhitePages[server];
            bio.Reputation *= 1 + percent;
        }

        internal List<string> GetProviders(string resourceName) 
        {
            List<string> res = new();
            // Getting the leaders of others MicroServices with the asked resource
            var othersGoodServices = Services
                .Values
                .Where(m => m.Name != this.Name)
                .Where(m => m.GetAllResourcesAvailable()
                    .Contains(Resource.Resources[resourceName]))
                .Select(m => m.LeaderId);
            // my providers
            if(Dir.YellowPages.TryGetValue(resourceName, out List<string> providers))
                res.AddRange(providers);

            res.AddRange(othersGoodServices);
            return res;
        }
        internal ServerBio GetBio(string serverID)
        {
            return Dir.WhitePages[serverID];
        }
        internal List<Resource> GetAllResourcesAvailable()
        {
            return Dir.YellowPages.Keys.Select(resName => Resource.Resources[resName]).ToList();  
        }
        internal void ChangeLeader(string leaderID)
        {
           LeaderId =  leaderID;
        }
        internal List<string> GetServers()
        {
            var servers = Dir.WhitePages.Select(pair => pair.Key).ToList();
            return servers;
        }
    }

    public enum ServiceType{
        Private,
        EntryPoint
    }
    // public enum ServerStatus{
    //     Active,
    //     Inactive,
    //     Dead
    // }
}