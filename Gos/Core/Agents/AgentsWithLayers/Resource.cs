using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Resource {
        internal static Dictionary<string, Resource> Resources = new();
        public string Name {get => _name; 
            set {
                if (Resources.ContainsKey(Name))
                    throw new Exception("Resource already exists");
                Resources.Add(Name, this);
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
            if(!Resources.ContainsKey(resource))
                throw new Exception("Resource doesn't exists");
                
            this.Requirements.Add((quantity,resource));
        }
    }

}