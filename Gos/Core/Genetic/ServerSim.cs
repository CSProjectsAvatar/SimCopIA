using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    public class ServerSim
    {
        private const double probMutateRscs = 0.7;
        internal List<LayerSim> layers = new();
        internal List<int> resources = new();
        private static Random _random = new Random();
        private double probMutateLayer = 0.4;
        private double probAddLayer;
        private double probRemoveLayer;

        public ServerSim Clone()
        {
            ServerSim serverSim = new ServerSim();
            serverSim.layers = (from item in layers
                                select item.Clone()).ToList();

            serverSim.resources = (from item in resources
                                   select item).ToList();

            return serverSim;
        }

        internal void Mutate()
        {
            // Layers
            // prob de agregar una capa
            if (_random.NextDouble() < probAddLayer) {
                layers.Add(LayerSim.RndLayer());
            }

            for (int i = 0; i < layers.Count; i++) {
                var layer = layers[i];

                // prob de eliminar una capa
                if (_random.NextDouble() < probRemoveLayer) {
                    layers.RemoveAt(i);
                    i--;
                }
                // probabilidad de mutacion de una capa
                else if (_random.NextDouble() < probMutateLayer) 
                    layer.Mutate();
            }

            // Resources
            for (int i = 0; i < resources.Count; i++) {
                if (_random.NextDouble() < probMutateRscs) { // prob de mutar un recurso
                    var max = resources.Max(); // @audit ver lo del max ese
                    var reso = _random.Next(max);
                    resources[i] = reso;
                }
            }
        }

        internal static ServerSim RndServer()
        {
            throw new NotImplementedException();
        }
    }

}

