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

        internal LayerSim Clone()
        {
            LayerSim layer = new LayerSim();
            layer.behavior = (from item in behavior
                              select item).ToList();
            return layer;
        }

        internal void Mutate()
        {
            for (int i = 0; i < behavior.Count; i++)
            {
                if (_random.NextDouble() < probMutateBehav)
                {
                    var max = behavior.Max(); // @audit ver lo del max ese
                    var beha = _random.Next(max);
                    behavior[i] = beha;
                }
            }
            
        }
    }

}

