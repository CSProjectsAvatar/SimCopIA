using System.Collections.Generic;

namespace ServersWithLayers
{
    public class Server{
        public string ID {get;}
        public Status Stats {get;}
        private List<Layer> _layers; 
        public Server(Environment env, string ID){
            
            this.ID = ID;

            this.Stats = new();
            
            this._layers = new();
        }

        public void HandlePerception(Perception p){

            foreach(var l in _layers) 
                l.Execute(p);
                        
            foreach(var e in this.Stats.EnumerateAndClear())
                Environment.CurrentEnv.SubsribeEvent(e.Item1,e.Item2);
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
            layer.SetServer(this);
            _layers.Add(layer);
        }
        
    }
}