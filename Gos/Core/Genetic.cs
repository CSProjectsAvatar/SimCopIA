using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    [TestClass]
    public class Genetic
    {
        Random _random;
        Stopwatch crono;
        private double _mutationProb;

        public Genetic()
        {
            crono = new Stopwatch();
            _mutationProb = 0.2;
            _random = new Random(Environment.TickCount);

        }

        public void Run(List<IndividualSim> individuals,
        Func<IndividualSim, double> mini,
        Func<IndividualSim, bool> validate,
        long timeInMs)
        {
            var initCount = individuals.Count;
            crono.Reset();
            crono.Start();

            while (crono.ElapsedMilliseconds < timeInMs)
            {
                var parents = getParents(individuals, mini); // Selecciono mejores padres
                var childs = getChilds(parents); // Realizo entrecruzamiento (Con Mutacion)

                individuals.Clear();
                individuals.AddRange(parents);
                individuals.AddRange(childs);

                // Se mueren los menos adaptados
                while (individuals.Count > initCount)
                {
                    for (int i = 0; i < individuals.Count; i++)
                    {
                        if (!Survives(individuals[i]))
                        {
                            individuals.RemoveAt(i);
                        }
                    }
                }

            } // Proxima Generacion
        }

        private bool Survives(IndividualSim individual)
        {
            if (_random.NextDouble() < 0.8)
            { // @audit poner una funcion de supervivencia en funcion de la calidad del individuo
                return true;
            }
            return false;
        }

        private List<IndividualSim> getParents(List<IndividualSim> individuals, Func<IndividualSim, double> mini)
        {
            int count = individuals.Count;
            int len = (int)Math.Ceiling(count * 0.6);
            List<double> pre_mini = (from item in individuals
                                     select mini(item)).ToList();

            var bests = individuals.Zip(pre_mini, (o, i) => new { o, i })
                                    .OrderBy(x => x.i)
                                    .Select(x => x.o)
                                    .Take(len)
                                    .ToList();
            return bests;
        }

        private List<IndividualSim> getChilds(List<IndividualSim> parents)
        {
            List<IndividualSim> childrens = new List<IndividualSim> { };
            while (childrens.Count != parents.Count)
            {
                var parent1 = parents[_random.Next(0, parents.Count - 1)];
                var parent2 = parents[_random.Next(0, parents.Count - 1)];

                childrens.Add(IndividualSim.generateChild(parent1, parent2));
            }
            return childrens;
        }


    }
    public class IndividualSim
    {
        private static Random _random = new Random(Environment.TickCount);
        internal List<MicroServiceSim> MicroServers = new();

        public IndividualSim Clone()
        {
            IndividualSim individual = new IndividualSim();
            individual.MicroServers = (from item in MicroServers
                                       select item.Clone()).ToList();

            return individual;
        }

        public static IndividualSim generateChild(IndividualSim parent1, IndividualSim parent2)
        {
            IndividualSim child = new IndividualSim();
            foreach (var micro in parent1.MicroServers.Concat(parent2.MicroServers))
            {
                double va = _random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
                if (va < 0.75)
                    child.MicroServers.Add(RndMicro(micro));
            }
            return child;
        }
        private static MicroServiceSim RndMicro(MicroServiceSim micro)
        {
            List<ServerSim> subServers = new List<ServerSim> { };
            foreach (var serv in micro.Servers)
            {
                double va = _random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
                if (va < 0.7)
                    subServers.Add(serv.Clone());
            }
            MicroServiceSim microServer = new MicroServiceSim();
            microServer.Servers = subServers;

            return microServer;
        }


    }

    public class MicroServiceSim
    {
        internal List<ServerSim> Servers = new();

        public MicroServiceSim Clone()
        {
            MicroServiceSim microServerSim = new MicroServiceSim();
            microServerSim.Servers = (from item in Servers
                                      select item.Clone()).ToList();
            return microServerSim;
        }

    }

    public class ServerSim
    {
        internal List<LayerSim> layers = new();
        internal List<int> resources = new();

        public ServerSim Clone()
        {
            ServerSim serverSim = new ServerSim();
            serverSim.layers = (from item in layers
                                select item.Clone()).ToList();

            serverSim.resources = (from item in resources
                                   select item).ToList();

            return serverSim;
        }
    }

    internal class LayerSim
    {
        internal List<int> behavior = new();

        LayerSim auxiliar = new LayerSim();

        internal LayerSim Clone()
        {
            LayerSim layer = new LayerSim();
            layer.behavior = (from item in behavior
                              select item).ToList();
            return layer;
        }
    }
    public class Factory
    {
        List<Behavior> _behaviors;
        List<Resource> _resources;
        public Factory(List<Behavior> behaviors, List<Resource> resources)
        {
            _behaviors = behaviors;
            _resources = resources;
        }

        public void RunSimulation(IndividualSim individual)
        {
            List<MicroService> microServices = new List<MicroService> { };
            List<Server> servers = new List<Server> { };

            for (int i = 0; i < individual.MicroServers.Count; i++)
            {
                string name = "M" + i;
                MicroService ms = new MicroService(name);
                for (int j = 0; j < individual.MicroServers[i].Servers.Count; j++)
                {
                    servers.Add(CreateServer(j, name, individual.MicroServers[i].Servers[j]));
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

