using System;
using System.Collections.Generic;

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

        public MicroService()
        {
            
        }
        internal void SetAsEntryPoint(){
            this.Type = ServiceType.EntryPoint;
        }
        internal static MicroService Get(string microserviceID)
        {
            if (!Services.ContainsKey(microserviceID))
                throw new Exception("MicroService doesn't exists");
            return Services[microserviceID];
        }
        internal List<string> GetProviders(string resName)
        {
            return Dir.YellowPages[resName];
        }
        internal ServerBio GetBio(string serverID)
        {
            return Dir.WhitePages[serverID];
        }

        internal void ChangeLeader(string leaderID)
        {
           LeaderId =  leaderID;
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