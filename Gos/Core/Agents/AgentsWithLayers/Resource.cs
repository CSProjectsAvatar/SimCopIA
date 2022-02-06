using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
namespace ServersWithLayers{
    public class Resource {
        internal static Dictionary<string, Resource> Resources = new();
        private static int _amount;
        private static List<Resource> finishers = new();
        internal static List<Resource> GetFinishedRscs { 
            get 
            { 
                if (_amount == Resources.Count)
                    return finishers;
                
                _amount = Resources.Count;
                finishers = Resources.Values.Where(r => !r.IsRequired).ToList();
                return finishers;
            }}
        public string Name {get => _name; 
            set {
                if (Resources.ContainsKey(value))
                    throw new Exception("Resource already exists");
                Resources.Add(value, this);
                _name = value;
            }
        }

        internal static List<Resource> GetRndFinishedRscs()
        {
            var finR = GetFinishedRscs;
            var rndCount = UtilsT.Rand.Next(1, finR.Count + 1);
            // returns rndCount items from finR randomly
            return finR.OrderBy(x => UtilsT.Rand.Next()).Take(rndCount).ToList();
        }
        public int RequiredTime { get; internal set; }
        public bool IsRequired { get; private set; }

        List<string> Requirements;
        private string _name;

        public Resource(string name){
            
            this.Name = name;
            this.Requirements = new();
        }

        private void AddReq(string resource){
            if(!Resources.ContainsKey(resource))
                throw new Exception("Resource doesn't exists");
                
            this.Requirements.Add(resource);
            Resources[resource].IsRequired = true;
        }
        public void AddReq(Resource resource){
            AddReq(resource.Name);
        }
        public void AddReqList(List<Resource> resources){
            foreach(var r in resources)
                AddReq(r);
        }

        public static void Dispose() {
            Resources.Clear();
        }

        public static Resource Rsrc(string name) {
            return Resources[name];
        }
    }

}