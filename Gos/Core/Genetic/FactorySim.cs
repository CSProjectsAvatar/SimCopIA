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
        List<Type> _events;
        private List<double> _probs;
        public static int MaxBehavior;
        public static int MaxResorce;
        public static int MaxServers;


        public FactorySim(List<Behavior> behaviors, List<Resource> resources)
            : this(behaviors, resources, 
                new List<Type>() { 
                    typeof(ReqType) 
                }, 
                new List<double>() { 
                    1.0 
                }
            ) {
            MaxBehavior = _behaviors.Count();
            MaxResorce = _resources.Count();

        }


        public FactorySim(List<Behavior> behaviors, List<Resource> resources, List<Type> events, List<double> probabilities)
        {
            _behaviors = behaviors;
            _resources = resources;
            _events = events;
            _probs = probabilities;
        }

        public void RunSimulation(IndividualSim individual)
        {
            List<Server> servers = new List<Server> { };

            // Creando los servers
            for (int i = 0; i < individual.MicroServices.Count; i++)
            {
                string mSName = null;
                if (i != 0){// Para coger el primer MicroS como el Main

                    mSName = "M" + i;
                    MicroService ms = new MicroService(mSName);
                }
                for (int j = 0; j < individual.MicroServices[i].Servers.Count; j++) {
                    // Creo los servidores
                    servers.Add(CreateServer(j, individual.MicroServices[i].Servers[j], mSName));
                }
            }

            // Asignando los servers a un nuevo env
            Env env = new Env();
            // env.CrearEventosConLoDeMauricio @audit 
            env.AddServerList(servers);




            // Limpio MicroServicios (No hace falta limpiar los Recursos, ni los Servers)
            MicroService.Services.Clear();


        }

        private Server CreateServer(int j, ServerSim serverSim, string mSName = null)
        {
            Server server = new Server("S" + j);

            server.AddLayers(CreateLayers(serverSim.layers));
            server.SetResources(CreateResources(serverSim.resources));

            if(mSName is not null) server.SetMService(mSName);
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

        private Layer CreateLayer(LayerSim layer)
        {
            Layer l = new Layer();

            foreach (var i in layer.behavior)
                l.AddBehavior(CreateBehavior(i));// o poner _behaviors[i]; 

            return l;
        }

        private Behavior CreateBehavior(int i)
        {
            return _behaviors[i];
        }
    }

}

