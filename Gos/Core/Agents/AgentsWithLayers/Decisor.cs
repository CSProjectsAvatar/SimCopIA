using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public class Decisor
    {
        Random Rnd = new Random(Environment.TickCount);
        int cycleTime;
        private Layer layer;
        int lastTimeMarked = 0;
        int currentIndex = 0;
        private double lastReputation = 0;
        private double curReputation => MicroService.GetReputation(layer.server.ID);

        private int Count => bGainedReputation.Count;

        private double totalGained;

        private List<double> bGainedReputation;
        private List<double> bWeight;

        public Decisor(Layer layer, int cycle = 500)
        {
            if (layer.behaviors.Count == 0) {
                throw new Exception("Layer must have at least one behavior");
            }
            cycleTime = cycle;
            this.layer = layer;
            lastReputation = curReputation;
            // set 1/count in every position
            // bWeight = Enumerable.Repeat(1.0 / layer.behaviors.Count, layer.behaviors.Count).ToList();
            bGainedReputation = Enumerable.Repeat(0.0, layer.behaviors.Count).ToList();
        }

        public int BehaviorDecisor(){
            
            if (TimeToChangeBehav()) {
                var gained = curReputation - lastReputation;
                lastReputation = curReputation;
                totalGained += gained;
                
                bGainedReputation[currentIndex] += gained;

                if (Exploring()){
                    currentIndex = Rnd.Next(Count); // Unbiased Random
                } else { // Exploting
                    var rnd = Rnd.NextDouble();
                    currentIndex = CalculateIndex(rnd); // Biased Random
                }
                lastTimeMarked = Env.Time;
            }
            return currentIndex;
        }

        private bool TimeToChangeBehav()
        {
            if (Env.Time - lastTimeMarked >= cycleTime)
                return true;
            return false;
        }

        private bool Exploring()
        {
            if(Env.Time >= cycleTime * Count * 5)
                return false;
            return true;
        }

        private int CalculateIndex(double rnd)
        {
            double acc = 0;
            for (int i = 0; i < Count; i++) {
                var rep = bGainedReputation[i]/totalGained;
                acc += rep;
                if (rnd <= acc) return i;
            }
            return 0;
        }
    }

}