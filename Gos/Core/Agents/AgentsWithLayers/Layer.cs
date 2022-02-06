using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public class Layer
    {
        internal Server server;
        internal List<Behavior> behaviors;
        private Func<IEnumerable<Behavior>,int> behaviorSelector;

        public IEnumerable<Behavior> Behavs => behaviors;

        public Layer()
        {
            behaviors = new List<Behavior> { };
            behaviorSelector = index => 0;
           
        }

        //Aqui la capa hace todo lo referente a modificar el estado de 'server'  o suscribir al environment Perceptions.
        //Esto basado en un una Perception 'p' y el estado interno de 'server'
        public void Process(Perception p)
        {
            if(behaviors.Count == 0) return;
            
            int index = behaviorSelector(behaviors);
            Behavior conduct = behaviors[index]; // @todo implementar una politica de seleccion de behaviors
            conduct.Run(server.Stats, p);
        }
        public Layer CloneInServer(Server server)
        {
            var copy = this.Clone() as Layer;
            copy.server = server;
            return copy;
        }
        object Clone()
        {
            return new Layer() { behaviors = behaviors.Select(x => x.Clone() as Behavior).ToList(), behaviorSelector=this.behaviorSelector};
        }

        public void SetBehaviourSelector(Func<IEnumerable<Behavior>, int>selector)
        {
            behaviorSelector = selector;
        }

        public void SetBehaviors(IEnumerable<Behavior> enumerable) {
            behaviors.AddRange(enumerable);
        }
    }
}