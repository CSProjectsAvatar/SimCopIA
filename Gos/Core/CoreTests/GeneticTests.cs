using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServersWithLayers;
using System.Linq;
using Utils;
using ServersWithLayers;

namespace Core {
    [TestClass]
    public class GeneticTests : BaseTest {
        #region  vars

        private Behavior boss;
        private Behavior workerB;
        private Behavior falenLeader;
        private Behavior contractor;

        #region Resources
        private Resource r1;
        private Resource r2;
        private Resource r3;
        private Resource r4;
        private Resource r5;
        private Resource r6;
        private Resource r7;
        private Resource r8;
        #endregion

        #region Listas de Resources
        List<int> resource1 ;
        List<int> resource2 ;
        List<int> resource3 ;
        List<int> resource4 ;
        List<int> resource5 ;
        #endregion

        #region Listas de Behavior

        List<int> behavior1 ;
        List<int> behavior2 ;
        List<int> behavior3 ;
        List<int> behavior4 ;
        List<int> behavior5 ;
        List<int> behavior6 ;
        List<int> behavior7 ;
        #endregion

        List<ServerSim> servers1 = new List<ServerSim> { };
        List<ServerSim> servers2 = new List<ServerSim> { };
        List<ServerSim> servers3 = new List<ServerSim> { };

        ServerSim serverSim1;
        ServerSim serverSim2;
        ServerSim serverSim3;
        ServerSim serverSim4;
        ServerSim serverSim5;
        ServerSim serverSim6;
        ServerSim serverSim7;

        MicroServiceSim M1;

        IndividualSim individual;
        List<MicroServiceSim> microServices;
        private Env env;
 



        #endregion

        [TestInitialize]
        public void Init() {

            boss = BehaviorsLib.Boss;
            workerB = BehaviorsLib.Worker;
            contractor = BehaviorsLib.Contractor;
            falenLeader = BehaviorsLib.FallenLeader;

            #region Resources
            r1 = new Resource("img1");
            r2 = new Resource("img2");
            r3 = new Resource("index");
            r4 = new Resource("home");
            r5 = new Resource("ing3");
            r6 = new Resource("index1");
            r7 = new Resource("index2");
            r8 = new Resource("index3");
            #endregion

            #region listas de resources

            resource1 = new List<int> { 0, 1, 2 };
            resource2 = new List<int> { 1, 3 };
            resource3 = new List<int> { 4, 5};
            resource4 = new List<int> { 4, 6, 7 };
            resource5 = new List<int> { 2, 5, 6 };
            #endregion

            #region Listas de behavior
            behavior1 = new List<int> { 0 };
            behavior2 = new List<int> { 0, 1 };
            behavior3 = new List<int> { 2 };
            behavior4 = new List<int> { 0, 1, 2 };
            behavior5 = new List<int> { 1, 2 };
            behavior6 = new List<int> { 2, 0 };
            behavior7 = new List<int> { 2, 1, 0 };
            #endregion

            #region MyRegion

            LayerSim layer1 = new LayerSim();
            layer1.behavior = behavior1;
            LayerSim layer2 = new LayerSim();
            layer2.behavior = behavior2;
            LayerSim layer3 = new LayerSim();
            layer3.behavior = behavior3;
            LayerSim layer4 = new LayerSim();
            layer4.behavior = behavior4;
            LayerSim layer5 = new LayerSim();
            layer5.behavior = behavior5;
            LayerSim layer6 = new LayerSim();
            layer6.behavior = behavior6;
            LayerSim layer7 = new LayerSim();
            layer7.behavior = behavior7;
            #endregion


            List<LayerSim> layerSims1 = new List<LayerSim> { layer1,layer2};
            List<LayerSim> layerSims2 = new List<LayerSim> {layer2,layer3 };
            List<LayerSim> layerSims3 = new List<LayerSim> {layer4, layer5, layer1 };
            List<LayerSim> layerSims4 = new List<LayerSim> {layer6};
            List<LayerSim> layerSims5 = new List<LayerSim> { layer7,layer5};

            #region MyRegion

            serverSim1 = new ServerSim();
            serverSim1.resources = resource1;
            serverSim1.layers = layerSims1;

            serverSim2 = new ServerSim();
            serverSim2.resources = resource2;
            serverSim2.layers = layerSims2;

            serverSim3 = new ServerSim();
            serverSim3.resources = resource3;
            serverSim3.layers = layerSims3;

            serverSim4 = new ServerSim();
            serverSim4.resources = resource1;
            serverSim4.layers = layerSims1;

            serverSim5 = new ServerSim();
            serverSim5.resources = resource4;
            serverSim5.layers = layerSims4;

            serverSim6 = new ServerSim();
            serverSim6.resources = resource5;
            serverSim6.layers = layerSims5;

            serverSim7 = new ServerSim();
            serverSim7.resources = resource2;
            serverSim7.layers = layerSims2;
            #endregion

            List<ServerSim> S1 = new List<ServerSim> { serverSim1,serverSim2};
            List<ServerSim> S2 = new List<ServerSim> { serverSim3,serverSim4};
            List<ServerSim> S3 = new List<ServerSim> { serverSim5,serverSim6,serverSim7};

            M1 = new MicroServiceSim();
            M1.Servers = S1;
            MicroServiceSim M2 = new MicroServiceSim();
            M2.Servers = S2;
            MicroServiceSim M3 = new MicroServiceSim();
            M3.Servers = S3;

            microServices = new List<MicroServiceSim> { M1,M2,M3};

            individual = new IndividualSim();

            individual.MicroServices = microServices;

            var env = new Env();
        }

        [TestCleanup]
        public void Clean() {
            MicroService.Services.Clear();
            Resource.Resources.Clear();
            Env.ClearServersLayers();
        }

       
        [TestMethod]
        public void GeneticTest()
        {
            List<Behavior> behaviors = new List<Behavior> { boss, workerB, falenLeader, contractor };
            List<Resource> resources = new List<Resource> { r1, r2, r3, r4, r5, r6, r7, r8 };
            /// --------------------------------------------------
            List<(double, double)> results;
            var bests = GeneticMeta.GeneticAlgo(behaviors, resources, out results);
            /// --------------------------------------------------

            var theBestVals = "Puntuacion Mejor individuo -> Avg: "
                    + results.First().Item1 + " - Rspsive: " + results.First().Item2;
            var strBest = "Individuo: \n" + bests.First();
        }
    }

    public class GeneticMeta{

        public static List<IndividualSim> GeneticAlgo(List<Behavior> behaviors, List<Resource> resources, out List<(double, double)> results, int budget = 50, int timeToWaitMs = 10000, int initialSamplerSize = 50)
        {
            FactorySim factory = new FactorySim(behaviors, resources, budget);
            var meta = new Genetic();
            var poblation = IndividualSim.Sampler(initialSamplerSize);

            var bests = meta.Run(poblation,
                            (IndividualSim i) => Minimize(i),
                            (IndividualSim i) => i.ValidIndiv(),
                            timeToWaitMs);
            results = new();
            List<double> finalR = new();
            foreach (var item in bests.Take(10))
            {
                var output = CallRunSim(item);
                results.Add((output.Average, output.ResponsePercent));

                var res = AverageResponsive(output);
                finalR.Add(res);
            }
            return bests.Take(10).ToList();
        }

        public static double Minimize(IndividualSim i)
        {
            Output output = CallRunSim(i);
            var res = AverageResponsive(output);
            return res;
        }

        private static double AverageResponsive(Output output)
        {
            return output.Average * (1 / ((output.ResponsePercent) / 100.0));
        }

        private static Output CallRunSim(IndividualSim i)
        {
            return FactorySim.RunSimulation(i);
        }

    }
}
