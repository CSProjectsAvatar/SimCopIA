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
            int len = count % 2 == 1 ? count / 2 + 1 : count / 2;
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
        

    public class Individual{
        public int numero;
    }
}
