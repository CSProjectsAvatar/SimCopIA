using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class SimulationSystem
    {
        public static double RunSimulation
        (Individual individual,  
        // Func<Dictionary<uint, double>, Dictionary<uint, double>, double> minimize = AttentionTime,
        double lambda = 0.3)
        {
           var simulation = new OneLeaderFollowersSimulator(individual.Doers, lambda);
        //    return minimize(simulation.Arrivals, simulation.GetDepartures());
           return AttentionTime(simulation.Arrivals, simulation.GetDepartures());
        }

        public static double AttentionTime(Dictionary<uint, double> arrivals, Dictionary<uint, double> departures)
        {
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
