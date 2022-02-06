using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    public class FactorySim
    {
        List<Behavior> _behaviors;
        List<Resource> _resources;
        public FactorySim(List<Behavior> behaviors, List<Resource> resources)
        {
            _behaviors = behaviors;
            _resources = resources;
        }

        public void RunSimulation(IndividualSim individual)
        {
            List<MicroService> microServices = new List<MicroService> { };
            List<Server> servers = new List<Server> { };

            for (int i = 0; i < individual.MicroServices.Count; i++)
            {
                string name = "M" + i;
                MicroService ms = new MicroService(name);
                for (int j = 0; j < individual.MicroServices[i].Servers.Count; j++)
                {
                    servers.Add(CreateServer(j, name, individual.MicroServices[i].Servers[j]));
                }
                microServices.Add(ms);
            }

        }

        private Server CreateServer(int j, string name, ServerSim serverSim)
        {
            Server server = new Server("S" + j);
            server.AddLayers(CreateLayers(serverSim.layers));
            server.SetResources(CreateResources(serverSim.resources));
            server.SetMService(name);
            return server;
        }

        private IEnumerable<Resource> CreateResources(List<int> resources)
        {
            List<Resource> res = new List<Resource> { };
            foreach (var i in resources)
                res.Add(_resources[resources[i]]);

            return res;
        }


        private IEnumerable<Layer> CreateLayers(List<LayerSim> layers)
        {
            List<Layer> _layers = new List<Layer> { };
            foreach (var beha in layers)
                _layers.Add(CreateLayer(beha));
                
            return _layers;
        }

        private Layer CreateLayer(LayerSim beha)
        {
            Layer l = new Layer();

            foreach (var i in beha.behavior)
                l.AddBehavior(CreateBehavior(i));// o poner _behaviors[i]; 

            return l;
        }

        private Behavior CreateBehavior(int i)
        {
            return _behaviors[i];
        }
    }

}

