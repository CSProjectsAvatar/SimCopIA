using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers{
    public class MicroService{
        internal static Dictionary<string, MicroService> MicroServices = new();
        private string _name;

        public string Name {get => _name; 
            set {
                if (MicroServices.ContainsKey(value))
                    throw new Exception("MicroService already exists");
                MicroServices.Add(value, this);
                _name = value;
            }
        }   
        public string LeaderId { get; set; }
        private Directory Dir { get; set; }

        public MicroService(string name){
            this.Name = name;
            this.Dir = new Directory();
        }

        internal static MicroService Get(string microserviceID)
        {
            if (!MicroServices.ContainsKey(microserviceID))
                throw new Exception("MicroService doesn't exists");
            return MicroServices[microserviceID];
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
    }


}