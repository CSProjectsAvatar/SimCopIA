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
        Random _random;
        Stopwatch crono;
        private double _mutationProb;

        public MetaHeuristics()
        {
            crono = new Stopwatch();
            _mutationProb = 0.2;
            _random = new Random(Environment.TickCount);
           
        }

        public void Run(List<Individual> individuals, 
        Func<Individual, int> mini, 
        Func<Individual, bool> validate,
        long timeInMs)
        {
            var initCount = individuals.Count;
            crono.Reset();
            crono.Start();

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
                        if(!Survives(individuals[i])){
                            individuals.RemoveAt(i);
                        }
                    }
                }
                
            } // Proxima Generacion
        }

        private bool Survives(Individual individual)
        {
            if(_random.NextDouble() < 0.8){ // @audit poner una funcion de supervivencia en funcion de la calidad del individuo
                return true;
            }
            return false;
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
            while (childrens.Count != parents.Count)
            {
                var parent1 = parents[_random.Next(0, parents.Count - 1)];
                var parent2 = parents[_random.Next(0, parents.Count - 1)];
                
                childrens.Add(generateChild(parent1, parent2));
            }
            return childrens;
        }

        private Individual generateChild(Individual parent1, Individual parent2)
        {
            double va = _random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
            Individual child;
            if (va < 0.5) {
                child = new Individual(parent1.Doers);
            }
            else {
                child = new Individual(parent2.Doers);
            }

            if (_random.NextDouble() < _mutationProb)
                child.Mutate();
                
            return child;
        }

        [TestMethod]
        public void getParentsTest()
        {
            var lista = new List<Individual>(){
                new Individual(2),
                new Individual(1),
                new Individual(3),
                new Individual(5),
            };
            var parents = getParents(lista, i => i.Doers);
            Assert.AreEqual(3, parents.Count);

            Assert.AreEqual(1, parents[0].Doers);
            Assert.AreEqual(2, parents[1].Doers);
        }

        [TestMethod]
        public void getChildTest()
        {
            var lista = new List<Individual>(){
                new Individual(2),
                new Individual(1),
            };
            var parents = getParents(lista, i => i.Dispatchers);
            var childs = getChilds(parents);

            Assert.AreEqual(parents.Count, childs.Count);

            Assert.IsTrue(
                (parents[0].Dispatchers == childs[0].Dispatchers &&
                parents[1].Doers == childs[0].Doers)
                || 
                (parents[1].Dispatchers == childs[0].Dispatchers &&
                parents[0].Doers == childs[0].Doers) );
        }
    }
    public class Individual{
        static Random _random = new Random(Environment.TickCount);
        public int Dispatchers { get; set; } = 1;
        public int Doers { get; set; }
        public int MonthlyMaintennanceCost { get; }

        public Individual() : this(0,0) { }
        public Individual(int dispatchers, int doers)
        {
            Dispatchers = dispatchers;
            Doers = doers;
            MonthlyMaintennanceCost = dispatchers*10 + doers*5;
        }

        public Individual(int doers) : this(1, doers)
        {
        }

        public static List<Individual> Sampler(int number){
            var list = new List<Individual>();
            for (int i = 0; i < number; i++)
            {
                var doers = _random.Next(1, 100);
                list.Add(new Individual(doers));
            }
            return list;
        }
        internal void Mutate()
        {
            if(_random.NextDouble() < 0.5){
                Doers -= 1;
            }
            else {
                Doers += 1;
            }
        }
    }
}
