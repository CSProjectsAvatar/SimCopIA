using System.Collections.Generic;
using Core;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers
{
    public class Server{
        public string ID {get;}
        public Status Stats {get;}
        private List<Layer> _layers;
        private ILogger<Server> _logger;
        public Server(string ID, ILogger<Server> logger=null, ILogger<Status> loggerS=null)
        {
            if (ID.Equals("0"))
                throw new GoSException("Server ID can't be 0, thats reserver for clients");
                
            this.ID = ID;
            this.Stats = new(ID,loggerS);
            
            this._layers = new();
            _logger = logger;
        }

        public void HandlePerception(Perception p){
            Stats.SaveEntry(p);
            _logger?.LogDebug("Pasa por todas las capas del Server {id} y ejecuta un comportamiento de cada una elegido por un protocolo de selección", ID);
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

        public void AddLayer(Layer layer){
            var clonedLayer = layer.CloneInServer(this);
            _layers.Add(clonedLayer);
        }

        internal void SetResources(IEnumerable<Resource> resources)
        {
            Stats.AvailableResources = new();
            Stats.AvailableResources.AddRange(resources);
        }

        public List<Layer> Layers()
        {
            return _layers;
        }
    }
}