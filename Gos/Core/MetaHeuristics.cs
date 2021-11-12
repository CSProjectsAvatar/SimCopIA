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
    }

    public class Individual{
        public int numero;
    }
}
