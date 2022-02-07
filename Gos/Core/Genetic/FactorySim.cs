using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;
using Utils;
namespace Core
{
    public class FactorySim
    {
        static List<Behavior> _behaviors;
        static List<Resource> _resources;
        static List<Type> _events;
        static private List<double> _probs;

        public static double Budget { get; private set;}
        public static int MaxProcesorsInServer { get; private set;}

        public static int MaxBehavior ;
        public static int MaxResource;
        public static int MaxServers => (int)(Budget / UtilsT.CostByMicro);


        public FactorySim(List<Behavior> behaviors, List<Resource> resources, double budget = 50, int maxProcesors = 10)
            : this(behaviors, resources, 
                new List<Type>() { 
                    typeof(ReqType) 
                }, 
                new List<double>() { 
                    1.0 
                },
                budget,
                maxProcesors
            ) { }


        public FactorySim(List<Behavior> behaviors, 
        List<Resource> resources, 
        List<Type> events, 
        List<double> probabilities, 
        double budget,
        int maxProcesors
        )
        {
            _behaviors = behaviors;
            _resources = resources;
            _events = events;
            _probs = probabilities;

            Budget = budget;
            MaxProcesorsInServer = maxProcesors;
            
            MaxBehavior = _behaviors.Count();
            MaxResource = _resources.Count();
        }


        public static void RunSimulation(IndividualSim individual)
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

        private  static Server CreateServer(int j, ServerSim serverSim, string mSName = null)
        {
            Server server = new Server("S" + j);

            server.AddLayers(CreateLayers(serverSim.layers));
            server.SetResources(CreateResources(serverSim.resources));

            if(mSName is not null) server.SetMService(mSName);
            return server;
        }

        private static IEnumerable<Resource> CreateResources(List<int> resources)
        {
            List<Resource> res = new List<Resource> { };
            foreach (var i in resources)
                res.Add(_resources[resources[i]]);
           

            return res;
        }


        private static IEnumerable<Layer> CreateLayers(List<LayerSim> layers)
        {
            List<Layer> _layers = new List<Layer> { };
            foreach (var beha in layers)
                _layers.Add(CreateLayer(beha));
                
            return _layers;
        }

        private static Layer CreateLayer(LayerSim layer)
        {
            Layer l = new Layer();

            foreach (var i in layer.behavior)
                l.AddBehavior(CreateBehavior(i));// o poner _behaviors[i]; 

            return l;
        }

        private static Behavior CreateBehavior(int i)
        {
            return _behaviors[i];
        }

        public static void SaveSimulation(IndividualSim individual)
        {
           // var run = RunSimulation(individual);
        }
    }

}

