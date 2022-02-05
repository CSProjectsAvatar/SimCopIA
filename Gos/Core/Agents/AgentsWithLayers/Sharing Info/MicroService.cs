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

        internal Func<ServerBio,double> credibilityFunction;
        public MicroService(string name) : this(name,defaultCredibility) {
            this.Dir = new Directory();
        }
        public MicroService(string name,Func<ServerBio,double> credibilityFunction) {
            this.Name = name;
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

        internal List<string> GetProviders(string resName)
        {
            return Dir.YellowPages[resName];
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

        internal List<string> GetServers(Status status)
        {
            List<string> servers = new List<string> { };
            Dictionary<string, ServerBio> whitePages = Services[status.MicroService.Name].Dir.WhitePages;
            foreach (var item in whitePages)
            {
                servers.Add(item.Key);
            }
            return servers;
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
    public enum ServerStatus{
        Active,
        Inactive,
        Dead
    }
}