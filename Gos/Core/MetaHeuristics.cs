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

        private List<Individual> getChilds(List<Individual> parents)
        {
            generateMutation(ch, _mutationProb);

            throw new NotImplementedException();
        }

        private List<int> getParents(List<Individual> individuals, Func<Individual, int> mini)
        {
            int count = individuals.Count;
            int len = (int)Math.Ceiling(count * 0.6);
            List<int> pre_mini = new List<int> { };

            foreach (var item in individuals)
            {
                pre_mini.Add(mini(item));
            }

            pre_mini.Sort();

            List<int> best_mini = new List<int> { };
            for (int i = 0; i < len; i++)
            {
                best_mini.Add(pre_mini[i]);
            }

            return best_mini;
        }

        private List<Individual> getChildren(List<int> parents)
        {
            List<Individual> children = new List<Individual> { };
            int childrenCount = 0;
            Random random1 = new Random();
            Random random2 = new Random();

            while (childrenCount != parents.Count)
            {
                var parent1 = parents[random1.Next(0, parents.Count - 1)];
                var parent2 = parents[random2.Next(0, parents.Count - 1)];
                children.Add(generateChildren(parent1, parent2));
                childrenCount++;
            }
            return children;
        }

        private Individual generateChildren(int parent1, int parent2)
        {
            throw new NotImplementedException();
        }

    public class Individual{
        public int Dispatchers { get; set; }
        public int Doers { get; set; }
        public int MonthlyMaintennanceCost { get; set; }

    }
}
