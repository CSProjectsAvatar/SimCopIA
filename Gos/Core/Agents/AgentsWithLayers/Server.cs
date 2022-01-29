using System.Collections.Generic;

namespace ServersWithLayers
{
    public class Server{
        public string ID {get;}
        public Status Stats {get;}
        private List<Layer> _layers; 
        public Server(Env env, string ID){
            
            this.ID = ID;

            this.Stats = new();
            
            this._layers = new();
        }

        public void HandlePerception(Perception p){

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
        public void AddLayer(Layer layer){
            var clonedLayer = layer.CloneInServer(this);
            _layers.Add(clonedLayer);
        }
        
    }
}