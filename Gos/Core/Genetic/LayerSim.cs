using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    internal class LayerSim
    {
        private const double probMutateBehav = 0.7;
        internal List<int> behavior = new();
        private static Random _random = new Random();
        private double probAddBehav;
        private double probRemoveBehav;

        internal LayerSim Clone()
        {
            LayerSim layer = new LayerSim();
            layer.behavior = (from item in behavior
                              select item).ToList();
            return layer;
        }

        internal void Mutate()
        {
            // prob de agregar un behavior
            if (_random.NextDouble() < probAddBehav) { // prob de agregar un behavior
                behavior.Add(_random.Next(FactorySim.MaxBehavior));
            }
            for (int i = 0; i < behavior.Count; i++) {
                // prob de eliminar un behavior
                if (_random.NextDouble() < probRemoveBehav) { // prob de eliminar un behavior
                    behavior.RemoveAt(i);
                    i--;
                }
            }
        }

        internal static LayerSim RndLayer()
        {
            LayerSim l = new LayerSim();
            List<int> bevavior = new List<int>(Enumerable.Range(0, FactorySim.MaxBehavior));
            var countBehavior = _random.Next(1, FactorySim.MaxBehavior);
            for (int i = 0; i < countBehavior; i++)
            {
                var va = _random.Next(bevavior.Count);
                l.behavior.Add(bevavior[va]);
                bevavior.RemoveAt(va);
            }
            return l;
        }

        
    }

}

