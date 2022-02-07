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

        public List<IndividualSim> Run(List<IndividualSim> individuals, 
        Func<IndividualSim, double> mini, 
        Func<IndividualSim, bool> validate,
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

                // Filtro de Validacion
                individuals = individuals.Where(validate).ToList();

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

            // var current = individuals.Count;
            // individuals = individuals.Where(validate).ToList();
            // if (individuals.Count < current)
            // {
            //     Console.WriteLine("Se murieron " + (current - individuals.Count) + " individuos");
            // }
            return individuals;

        }

        private bool Survives(IndividualSim individual)
        {
            if(_random.NextDouble() < 0.9){
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

                var child = IndividualSim.generateChild(parent1, parent2);
                if (_random.NextDouble() < _mutationProb) // @audit check this out
                    child.Mutate();
                childrens.Add(child);
            }
            return childrens;
        }
    }
  
}
