using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Resource {
        private static Dictionary<string, Resource> resources = new();
        public string Name {get => _name; 
            set {
                if (resources.ContainsKey(Name))
                    throw new Exception("Resource already exists");
                resources.Add(Name, this);
                _name = value;
            }
        }
        public List<(int, string)> Requirements;
        public int TimeCost;
        private string _name;

        public Resource(){
            this.Requirements = new();
        }

        public void AddReq(string resource, int quantity=1){
            if(!resources.ContainsKey(resource))
                throw new Exception("Resource doesn't exists");
                
            this.Requirements.Add((quantity,resource));
        }
    }

}