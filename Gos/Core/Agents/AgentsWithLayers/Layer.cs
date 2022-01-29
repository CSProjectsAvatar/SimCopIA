using System;
using System.Collections.Generic;

namespace ServersWithLayers
{
    public class Layer
    {

        internal Server server;
        internal List<Behavior> behaviors;

        public Layer()
        {

        }

        //Aqui la capa hace todo lo referente a modificar el estado de 'server'  o suscribir al environment Perceptions.
        //Esto basado en un una Perception 'p' y el estado interno de 'server'
        public void Process(Perception p)
        {

            Behavior conduct = behaviors[0];
            conduct.Run(server.Stats, p);

        }

        public Layer Clone()
        {
            var copy = new Layer();

            if (behaviors != null)
            {
                var tempBehavior = new List<Behavior> { };
                for (var i = 0; i < behaviors.Count; i++)
                {
                    var value = behaviors[i];
                    value = value.Clone();
                    tempBehavior.Add(value);
                }
                copy.behaviors = tempBehavior;
            }
            return copy;
        }

        public Layer CloneInServer(Server server)
        {
            var copyLayer = this;
            var copy = copyLayer.Clone();
            copy.server = server;
            return copy;
        }
    }

}