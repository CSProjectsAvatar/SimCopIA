using System.Collections.Generic;
using Core;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers
{
    public class Server{
        public string ID {get;}
        public Status Stats {get;}
        public bool ServerDown { get; private set; }

        internal Layer FirstLayer => _layers[0];
        private List<Layer> _layers;
        private ILogger<Server> _logger;
        public Server(string ID, ILogger<Server> logger=null, ILogger<Status> loggerS=null)
        {
            this.ID = ID;
            this.Stats = new(ID,loggerS);
            
            this._layers = new();
            _logger = logger;
        }

        public void HandlePerception(Perception p){
            if (ServerDown) return;
            
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

        internal void Failure()
        {
            ServerDown = true;
        }

        public void AddLayer(Layer layer){
            var clonedLayer = layer.CloneInServer(this);
            _layers.Add(clonedLayer);
        }

        public void SetResources(IEnumerable<Resource> resources)
        {
            Stats.AvailableResources = new();
            Stats.AvailableResources.AddRange(resources);
        }

        internal void ClearLayers()
        {
            _layers.Clear();
        }
        ///retorna el object asociado a una variable `varName` de el primer comportamiento asociado a la capa en la posicion `layerIndex`
        internal object GetLayerBehaVars(int layerIndex,string varName){
            if(_layers.Count > layerIndex)
                return _layers[layerIndex].behaviors[0].GetVariables(varName);
            throw new System.Exception("Indice capa mayor que la cantidad de capas.");
        }
    }
}