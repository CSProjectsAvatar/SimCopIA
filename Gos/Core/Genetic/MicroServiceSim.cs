using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    public class MicroServiceSim
    {
        private static Random _random = new Random();
        private double probChangeLeader = 0.3;
        private double probChangeServer = 0.3;


        internal List<ServerSim> Servers = new();

        internal static MicroServiceSim RndMicro()
        {
            throw new NotImplementedException();
        }

        public MicroServiceSim Clone()
        {
            MicroServiceSim microServerSim = new MicroServiceSim();
            microServerSim.Servers = (from item in Servers
                                      select item.Clone()).ToList();
            return microServerSim;
        }

        public void Mutate()
        {
            if(Servers.Count > 1 && _random.NextDouble() < probChangeLeader) {
                int r = _random.Next(1, Servers.Count);
                (Servers[0], Servers[r]) = (Servers[r], Servers[0]); // @audit ver si esto pincha

                // var aux = Servers[0];
                // Servers[0] = Servers[r];
                // Servers[r] = aux;
            }
            foreach (var server in Servers) {
                if(_random.NextDouble() < probChangeServer)
                    server.Mutate();
            }
        }
    }

}

