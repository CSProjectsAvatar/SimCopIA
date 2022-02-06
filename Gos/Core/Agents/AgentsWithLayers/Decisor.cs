using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public interface IDecisor : ICloneable {
        void SetLayer(Layer layer);
        int Selector(IEnumerable<Behavior> behavs);

    }
    public class Decisor : IDecisor
    {
        #region Internal vars
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
        // private List<double> bWeight;
        #endregion


        public Decisor(int cycle = 500)
        {
            cycleTime = cycle;
        }
        public void SetLayer(Layer layer)
        {
            this.layer = layer;
            bGainedReputation = new List<double>();
        }
        private bool firstTime = true;
        public int Selector(IEnumerable<Behavior> behavs){
            if (firstTime)
            {
                firstTime = false;

                lastReputation = curReputation;
                bGainedReputation = Enumerable.Repeat(0.0, layer.behaviors.Count).ToList();
            }
            return BehaviorDecisor();
        }

        public object Clone()
        {
            return new Decisor(cycleTime);
        }
        
        private int BehaviorDecisor(){
            
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