using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Core
{
    public class MetaHeuristics
    {
        Stopwatch crono;
        private float _mutationProb;

        public MetaHeuristics(float mutationProb)
        {
            MetaH(new List<Individual>() { },
             (Individual i) => { return i.numero; },
             (Individual i) => { return 0 < i.numero; });


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
                        if(!Survives(individuals[i])){
                            individuals.RemoveAt(i);
                        }
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
            List<int> pre_mini = new List<int> { };

            foreach (var item in individuals)
            {
                pre_mini.Add(mini(item));
            }

            var result = individuals.Zip(pre_mini, (o, i) => new { o, i }).OrderBy(x => x.i).Select(x => x.o);
            List<Individual> best = result.ToList();

            List<Individual> best_parents = new List<Individual> { };
            for (int i = 0; i < len; i++)
            {
                best_parents.Add(best[i]);
            }

            return best_parents;
        }

        private List<Individual> getChilds(List<Individual> parents)
        {
            List<Individual> children = new List<Individual> { };
            int childrenCount = 0;
            Random random1 = new Random();
            Random random2 = new Random();

            while (childrenCount != parents.Count)
            {
                var parent1 = parents[random1.Next(0, parents.Count - 1)];
                var parent2 = parents[random2.Next(0, parents.Count - 1)];
                children.Add(generateChild(parent1, parent2));
                childrenCount++;
            }
            return children;
        }

        private Individual generateChild(Individual parent1, Individual parent2)
        {
            Random random = new Random();
            double va = random.NextDouble(); //variable aleatoria con probabilidad uniforme en [0,1]
            Individual child = new Individual();
            if (va < 0.5)
            {
                child.Dispatchers = parent1.Dispatchers;
                child.Doers = parent1.Doers;
                child.MonthlyMaintennanceCost = parent2.MonthlyMaintennanceCost;
            }
            else
            {
                child.Dispatchers = parent2.Dispatchers;
                child.Doers = parent2.Doers;
                child.MonthlyMaintennanceCost = parent1.MonthlyMaintennanceCost;
            }

            return child;
        }

    public class Individual{
        public int Dispatchers { get; set; }
        public int Doers { get; set; }
        public int MonthlyMaintennanceCost { get; set; }

    }
}
