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
        public bool EntryPoint;
        private static Random _random = new Random();
        private double probChangeLeader = 0.3;
        private double probChangeServer = 0.3;
        private double probAddServer = 0.3;
        private double probEntryPointChange = 0.3;


        internal List<ServerSim> Servers = new();

        public double Budget => Servers.Sum(s => s.Cost);

        internal static MicroServiceSim RndMicro()
        {
            MicroServiceSim m = new MicroServiceSim();

            var countServer = _random.Next(1,FactorySim.MaxServers);
            for (int i = 0; i < countServer; i++)
            {
                m.Servers.Add(ServerSim.RndServer());
            }
            m.EntryPoint = _random.NextDouble() < 0.5;

            return m;
        }

        public MicroServiceSim Clone()
        {
            MicroServiceSim microServerSim = new MicroServiceSim();
            microServerSim.EntryPoint = EntryPoint;
            
            microServerSim.Servers = (from item in Servers
                                      select item.Clone()).ToList();
            return microServerSim;
        }

        public void Mutate()
        {
            // cambiar su condicion de EntryPoint
            if (_random.NextDouble() < probEntryPointChange){
                EntryPoint = !EntryPoint;
            }

            if(_random.NextDouble() < probAddServer) { // prob de agregar un server
                Servers.Add(ServerSim.RndServer());
            }
            // prob de cambiar el lider
            if(Servers.Count > 1 && _random.NextDouble() < probChangeLeader) { 
                int r = _random.Next(1, Servers.Count);
                (Servers[0], Servers[r]) = (Servers[r], Servers[0]); // @audit ver si esto pincha

                // var aux = Servers[0];
                // Servers[0] = Servers[r];
                // Servers[r] = aux;
            }
            for (int i = 0; i < Servers.Count; i++) {
                var server = Servers[i];
                
                if(_random.NextDouble() < probChangeServer) { // prob de eliminar un server
                    Servers.RemoveAt(i);
                    i--;
                }
                else if(_random.NextDouble() < probChangeServer) // prob de mutar un server
                    server.Mutate();
            }
        }

        public override string ToString()
        {
            string toString = "Servers :" +"\n";
            int i = 0;
            foreach (var item in Servers)
            {
                toString += "S"+i+ "\n" + item.ToString()+"\n";
                i++;
            }
            return toString;
        }
    }

}

