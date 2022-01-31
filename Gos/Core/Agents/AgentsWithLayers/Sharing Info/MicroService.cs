using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class MicroService{
        internal static Dictionary<string, MicroService> MicroServices = new();
        private string _name;

        public string Name {get => _name; 
            set {
                if (MicroServices.ContainsKey(Name))
                    throw new Exception("MicroService already exists");
                MicroServices.Add(Name, this);
                _name = value;
            }
        }   
        public static string LeaderId { get; set; }
        public Directory Dir { get; set; }

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
    }


}