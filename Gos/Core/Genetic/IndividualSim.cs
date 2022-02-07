using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers;

namespace Core
{
    public class IndividualSim
    {
        internal List<MicroServiceSim> MicroServices = new();
        
        private static Random _random = new Random(Environment.TickCount);
        private const double probPassMicro = 0.75;
        private const double probPassServer = 0.7;
        private double probRemoveMicro = 0.2;
        private double probMutateMicro = 0.2;

        public IndividualSim Clone()
        {
            IndividualSim individual = new IndividualSim();
            individual.MicroServices = (from item in MicroServices
                                       select item.Clone()).ToList();

            return individual;
        }

        public void Mutate()
        {
            // probabilidad de agregar un microservicio
            if (_random.NextDouble() < probRemoveMicro)
                MicroServices.Add(MicroServiceSim.RndMicro());

            for (int i = 0; i < MicroServices.Count; i++)
            {
                if (_random.NextDouble() < probRemoveMicro) {// probabilidad de remover micro
                    MicroServices.RemoveAt(i);
                    i--;
                }

                else if (_random.NextDouble() < probMutateMicro) // probabilidad de mutacion micro
                    MicroServices[i].Mutate();
            }
        }

        /// <summary>
        /// Generates a child from two parents
        /// </summary>
        public static IndividualSim generateChild(IndividualSim parent1, IndividualSim parent2)
        {
            IndividualSim child = new IndividualSim();
            foreach (var micro in parent1.MicroServices.Concat(parent2.MicroServices))
            {
                double va = _random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
                if (va < probPassMicro)
                    child.MicroServices.Add(RndSubMicro(micro));
            }
            return child;
        }
       
        /// <summary>
        /// Generates a random microservice subset of a given microservice
        /// </summary>
        private static MicroServiceSim RndSubMicro(MicroServiceSim micro)
        {
            List<ServerSim> subServers = new List<ServerSim> { };
            foreach (var serv in micro.Servers)
            {
                double va = _random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
                if (va < probPassServer)
                    subServers.Add(serv.Clone());
            }
            MicroServiceSim microServer = new MicroServiceSim();
            microServer.Servers = subServers;

            return microServer;
        }

        internal static IndividualSim RndIndividual()
        {
            IndividualSim i = new IndividualSim();

            var countMicro = _random.Next(1, FactorySim.MaxServers/4);// cambier 
            for (int j = 0; j < countMicro; j++)
            {
                i.MicroServices.Add(MicroServiceSim.RndMicro());
            }

            return i;
        }

        internal static List<IndividualSim> GeneratePoblation(int n)
        {
            List<IndividualSim> poblation = new List<IndividualSim> { };
            for (int i = 0; i <= n; i++)
            {
                poblation.Add(RndIndividual());
            }
            return poblation;
        }
    }

}

