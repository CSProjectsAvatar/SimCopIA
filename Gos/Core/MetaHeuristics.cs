using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core
{
    [TestClass]
    public class MetaHeuristics
    {
        Stopwatch crono;
        private float _mutationProb;

        public MetaHeuristics(float mutationProb)
        {
            MetaH(new List<Individual>() { },
             (Individual i) => { return i.Dispatchers; },
             (Individual i) => { return 0 < i.Doers; },
             1000);


            crono = new Stopwatch();
            _mutationProb = mutationProb;
        }

        private void MetaH(List<Individual> individuals, 
        Func<Individual, int> mini, 
        Func<Individual, bool> validate,
        long timeInMs)
        {
            var initCount = individuals.Count;
            crono.Reset();

            while(crono.ElapsedMilliseconds < timeInMs){
                var parents = getParents(individuals, mini); // Selecciono mejores padres
                var childs = getChilds(parents); // Realizo entrecruzamiento (Con Mutacion)

                individuals.Clear();
                individuals.AddRange(parents);
                individuals.AddRange(childs);

                // Se mueren los menos adaptados
                while (individuals.Count > initCount)
                {
                    for (int i = 0; i < individuals.Count; i++) {
                        // if(!Survives(individuals[i])){
                        //     individuals.RemoveAt(i);
                        // }
                    }
                    // @audit Si Toda la poblacion es el caso maximo, Loop Infinito
                }
                
            } // Repito
        }

        private void generateMutation(Individual ch, object mutationProb)
        {
            throw new NotImplementedException();
        }      

        private List<Individual> getParents(List<Individual> individuals, Func<Individual, int> mini)
        {
            int count = individuals.Count;
            int len = (int)Math.Ceiling(count * 0.6);
            List<int> pre_mini = (from item in individuals
                                  select mini(item)).ToList();

            var bests = individuals.Zip(pre_mini, (o, i) => new { o, i })
                                    .OrderBy(x => x.i)
                                    .Select(x => x.o)
                                    .Take(len)
                                    .ToList();
            // List<Individual> best = result.ToList();

            // List<Individual> best_parents = new List<Individual> { };
            // for (int i = 0; i < len; i++)
            // {
            //     best_parents.Add(bests[i]);
            // }

            return bests;
        }

        private List<Individual> getChilds(List<Individual> parents)
        {
            List<Individual> childrens = new List<Individual> { };
            Random random1 = new Random();

            while (childrens.Count != parents.Count)
            {
                var parent1 = parents[random1.Next(0, parents.Count - 1)];
                var parent2 = parents[random1.Next(0, parents.Count - 1)];
                
                childrens.Add(generateChild(parent1, parent2));
            }
            return childrens;
        }

        private Individual generateChild(Individual parent1, Individual parent2)
        {
            Random random = new Random();
            double va = random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
            Individual child;
            if (va < 0.5) {
                child = new Individual(parent1.Dispatchers, parent1.Doers);
            }
            else {
                child = new Individual(parent2.Dispatchers, parent2.Doers);
            }

            return child;
        }

        [TestMethod]
        public void getParentsTest()
        {
            var lista = new List<Individual>(){
                new Individual(2, 5),
                new Individual(1, 4),
                new Individual(3, 3),
                new Individual(5, 8),
            };
            var parents = getParents(lista, i => i.Dispatchers);
            Assert.AreEqual(3, parents.Count);

            Assert.AreEqual(1, parents[0].Dispatchers);
            Assert.AreEqual(2, parents[1].Dispatchers);
        }

        [TestMethod]
        public void getChildTest()
        {
            var lista = new List<Individual>(){
                new Individual(2, 5),
                new Individual(1, 4),
                new Individual(3, 3),
                new Individual(5, 8),
            };
            var parents = getParents(lista, i => i.Dispatchers);
            var childs = getChilds(parents);

            Assert.AreEqual(parents.Count, childs.Count);

            // Assert.AreEqual(parents[0].Dispatchers,);
            // Assert.AreEqual(2, parents[1].Dispatchers);
        }
    }
    public class Individual{

        public int Dispatchers { get; set; }
        public int Doers { get; set; }
        public int MonthlyMaintennanceCost { get; }


        public Individual(){ }
        public Individual(int dispatchers, int doers)
        {
            Dispatchers = dispatchers;
            Doers = doers;
            MonthlyMaintennanceCost = dispatchers*10 + doers*5;
        }
    }
}
