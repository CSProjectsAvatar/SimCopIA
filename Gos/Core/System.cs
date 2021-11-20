using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class System
    {
        public Tuple<Dictionary<uint,double>,Dictionary<uint,double>> GetDictionaries (Individual individuals)
        {
           var simulation = new OneLeaderFollowersSimulator(individuals.Doers, 0.3, loggerFactory.CreateLogger<OneLeaderFollowersSimulator>());
           var dictionary = Tuple.Create(simulation.Arrivals, simulation.GetDepartures());
           return dictionary;
        }

        public double Min(Tuple<Dictionary<uint, double>, Dictionary<uint, double>> dictionaries)
        {
            var arrivals = dictionaries.Item1;
            var departures = dictionaries.Item2;
            double[] sub = new double[arrivals.Count];
            int i = 0;
            var arrivalsValues = arrivals.Values.ToList();

            foreach (var item in arrivals.Keys)
            {
                sub[i] = departures[item] - arrivalsValues[i];
                i++;
            }

            return  sub.Sum() / sub.Length;

        }
    }
}
