using System.Collections.Generic;
using Core;

namespace ServersWithLayers
{
    public class Server{
        public string ID {get;}
        public Status Stats {get;}
        public bool ServerDown { get; private set; }

        internal List<Layer> _layers; 
        public Server(string ID){
            if (ID.Equals("0"))
                throw new GoSException("Server ID can't be 0, thats reserver for clients");
                
            this.ID = ID;
            this.Stats = new(ID);
            
            this._layers = new();
        }

        public void HandlePerception(Perception p){
            if (ServerDown) return;
            
            Stats.SaveEntry(p);

            foreach(var l in _layers) 
                l.Process(p);
                        
            foreach(var e in this.Stats.EnumerateAndClear())
                Env.CurrentEnv.SubsribeEvent(e.Item1,e.Item2);
        }

        public void SetLayers(IEnumerable<Layer> layers){
            _layers = new List<Layer>();
            AddLayers(layers);
        }
        public void AddLayers(IEnumerable<Layer> layers){
            foreach(var l in layers)
                AddLayer(l);
        }

        internal void SetMServiceIfNull(string main)
        {
            if(this.Stats.MicroService is null)
                SetMService(main);
        }
        internal void SetMService(string mService) //Note: the server can't have been in a microS before
        {
            MicroService.AddServer(this, mService);
        }

        internal void Failure()
        {
            ServerDown = true;
        }

        public void AddLayer(Layer layer){
            var clonedLayer = layer.CloneInServer(this);
            _layers.Add(clonedLayer);
        }

        internal void SetResources(IEnumerable<Resource> resources)
        {
            Stats.AvailableResources = new();
            Stats.AvailableResources.AddRange(resources);
        }
    }
}