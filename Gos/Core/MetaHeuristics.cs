using System;
using System.Collections.Generic;

namespace Core
{
    public class MetaHeuristics
    {
        public MetaHeuristics(){
            MetaH(new List<Individual>(){},
             (Individual i) => {return i.numero;}, 
             (Individual i) => {return 0 < i.numero;});
        }

        private void MetaH(List<Individual> individuals, Func<Individual, int> mini, Func<Individual, bool> validate)
        {
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
        public int numero;
    }
}
