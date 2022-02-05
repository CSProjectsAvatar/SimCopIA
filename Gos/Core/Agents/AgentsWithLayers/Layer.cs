using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public class Layer
    {
        internal Server server;
        internal List<Behavior> behaviors;

        public Layer()
        {
            behaviors = new List<Behavior> { };
        }

        //Aqui la capa hace todo lo referente a modificar el estado de 'server'  o suscribir al environment Perceptions.
        //Esto basado en un una Perception 'p' y el estado interno de 'server'
        public void Process(Perception p)
        {
            Behavior conduct = behaviors[0]; // @todo implementar una politica de seleccion de behaviors
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
            return new Layer() { behaviors = behaviors.Select(x => x.Clone() as Behavior).ToList() };
        }
    
    }

}