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
            foreach (var layer in layers) {
                if (_random.NextDouble() < probMutateLayer) // probabilidad de mutacion de una capa
                    layer.Mutate();
            }
            for (int i = 0; i < resources.Count; i++) {
                if (_random.NextDouble() < probMutateRscs)
                {
                    var max = resources.Max();
                    var reso = _random.Next(max);
                    resources[i] = reso;
                }
            }
        }
    }

}

