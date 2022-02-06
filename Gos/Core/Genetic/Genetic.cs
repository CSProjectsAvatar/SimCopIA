using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        public void Run(List<Individual> individuals, 
        Func<Individual, double> mini, 
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

        private List<Individual> getParents(List<Individual> individuals, Func<Individual, double> mini)
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

        [Obsolete]
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
  
}
