using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;
using Utils;
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

        public int ParallelsProcesors { get; private set; }
        public double Cost => ParallelsProcesors * UtilsT.CostByMicro;

        public ServerSim Clone()
        {
            ServerSim serverSim = new ServerSim();
            serverSim.ParallelsProcesors = ParallelsProcesors;
            
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
                    var reso = _random.Next(FactorySim.MaxResource);
                    resources[i] = reso;
                }
            }
        }

        internal static ServerSim RndServer()
        {
            ServerSim s = new ServerSim();
            s.ParallelsProcesors = _random.Next(1, FactorySim.MaxProcesorsInServer);
            if(s.ParallelsProcesors == 0)
                throw new Exception("ParallelsProcesors == 0");

            var countLayer = _random.Next(1, FactorySim.MaxBehavior);
            for (int i = 0; i < countLayer; i++)
            {
                s.layers.Add(LayerSim.RndLayer());
            }

            var countResource = _random.Next(1, FactorySim.MaxResource);
            List<int> resor = new List<int>(Enumerable.Range(0, FactorySim.MaxResource));
            for (int i = 0; i < countResource; i++)
            {
                var va = _random.Next(resor.Count);
                s.resources.Add(resor[va]);
                resor.RemoveAt(va);

            }

            return s;
        }

        public override string ToString()
        {

            string toString = "Parallels Procesors :" + ParallelsProcesors+"\n";
            foreach (var item in layers)
            {
                toString += "Layer { "+item.ToString() +" }"+ "\n";
            }
            toString += "Resources" + "{ ";

            foreach (var item in resources)
            {
                toString += FactorySim._resources[item].Name + " ";
            }
            toString += " }" + "\n";

            return toString;
        }
    }

}

