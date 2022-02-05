using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers{
    public class MicroService{
        public static Dictionary<string, MicroService> Services = new();
        private ILogger<MicroService> _logger;

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
        internal Func<ServerBio,double> credibilityFunction;
        public MicroService(string name, ILogger<MicroService> logger = null) 
            : this(name, defaultCredibility, logger) { }
        public MicroService(string name, Func<ServerBio,double> credibilityFunction
                            , ILogger<MicroService> logger) {
            this.Name = name;
            this.Dir = new Directory();
            _logger = logger;
            this.credibilityFunction = credibilityFunction; 
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
        internal static double GetReputation(string server)
        {
            var sBio = Services
                        .Select(kv => kv.Value)
                        .Where(m => m.ContainsServer(server))
                        .FirstOrDefault()
                        .GetBio(server);
            return sBio.Reputation;
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

        

        internal bool ContainsServer(string server)
        {
            return Dir.WhitePages.ContainsKey(server);
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
            if (!Dir.WhitePages.TryGetValue(serverID, out ServerBio bio))
                throw new ArgumentException($"Microservice {_name} does not contain server {serverID}");
            return bio;
        }
        internal List<Resource> GetAllResourcesAvailable()
        {
            return Dir.YellowPages.Keys.Select(resName => Resource.Resources[resName]).ToList();  
        }
        internal void ChangeLeader(string leaderID)
        {
            _logger?.LogInformation("Microservicio cambia de lider a {leaderID}", leaderID);
           LeaderId =  leaderID;
        }
        internal List<string> GetServers()
        {
            var servers = Dir.WhitePages.Select(pair => pair.Key).ToList();
            return servers;
        }

        private void ResetReputation(ServerBio biography)
        {
            biography.Reputation = (int)ServerBio.initRep;
        }

        internal void ForAllBiography()
        {
            Dictionary<string, ServerBio> whitePages = Services[this.Name].Dir.WhitePages;
            foreach (var item in whitePages)
            {
                ResetReputation(item.Value);
            }
        }

        static double defaultCredibility(ServerBio bio){
            var reputation = bio.Reputation;
            var pProcessors = bio.ParallelProcessors;
            return  reputation * pProcessors;
        }

        public IEnumerable<string> SortedByCredibility(string resName){

            var listServers=GetProviders(resName);

            listServers.OrderByDescending(
                (string s)=>credibilityFunction(this.GetBio(s))
            );

            return listServers;
        } 
    }
//borrar :(
    class CredibilityComparer : IComparer<string>{
        MicroService microService;
        bool greaterFirst;
        Func<ServerBio, double> creditibilityFunction => microService.credibilityFunction;
        public CredibilityComparer(MicroService microService, bool greaterFirst =true){
            this.microService = microService;
            this.greaterFirst = greaterFirst;
        }
        public int Compare(string s1, string s2){
            ServerBio sb1 = microService.GetBio(s1);
            ServerBio sb2 = microService.GetBio(s2);

            int direction = greaterFirst ? -1 : 1;            

            if(this.creditibilityFunction(sb1) > this.creditibilityFunction(sb2))
                return 1 * direction;
            if(this.creditibilityFunction(sb1) < this.creditibilityFunction(sb2))
                return -1 * direction;
            return 0;
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