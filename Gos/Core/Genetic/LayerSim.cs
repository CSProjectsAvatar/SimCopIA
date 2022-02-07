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
                behavior.Add(_random.Next(behavior.Count));
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
            List<int> bevavior = Rellenar(5);// count de omarB
            var countBehavior = _random.Next(1,3/*lo de omar countB*/);
            for (int i = 0; i < countBehavior; i++)
            {
                var va = _random.Next(bevavior.Count-1);// lo de omar countB
                l.behavior.Add(bevavior[va]);
                bevavior.RemoveAt(va);
            }
            return l;
        }

        internal static List<int> Rellenar(int count)
        {
            List<int> b = new List<int> { };
            for (int i = 0; i < count-1; i++)
            {
                b.Add(i);
            }
            return b;
        }
    }

}

