using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers{
    public class MicroService{
        internal static Dictionary<string, MicroService> Services = new();
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
        public MicroService(string name, ILogger<MicroService> logger)
        {
            this.Name = name;
            this.Dir = new Directory();
            _logger = logger;
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
            _logger.LogInformation("Los empleados del microservicio cambian de lider");
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

        private void ResetReputation(ServerBio biography)
        {
            biography.Reputation = ServerBio.constant;
        }

        internal void ForAllBiography()
        {
            Dictionary<string, ServerBio> whitePages = Services[this.Name].Dir.WhitePages;
            foreach (var item in whitePages)
            {
                ResetReputation(item.Value);
            }
        }

        internal double Reputation (string serverID)
        {
            ServerBio sb = this.Dir.WhitePages[serverID];
            return sb.Reputation;
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