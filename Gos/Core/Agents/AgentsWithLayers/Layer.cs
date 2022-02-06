using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public class Layer
    {
        internal Server server;
        internal List<Behavior> behaviors;
        private Func<IEnumerable<Behavior>,int> behaviorSelector;
        private IDecisor decisor;

        public Layer()  
        {
            behaviors = new List<Behavior> { };
            behaviorSelector = index => 0;
        }
        public void AssociateDecisor(IDecisor decisor){
            this.decisor = decisor;
            decisor.SetLayer(this);
            behaviorSelector = decisor.Selector;
        }
        

        /// <summary>
        /// Process a perception 'p' and update the internal state of the server. Returns the index of the selected behavior.
        /// </summary>
        public int Process(Perception p)
        {
            if(behaviors.Count == 0) return -1;
            
            int index = behaviorSelector(behaviors);
            Behavior conduct = behaviors[index];
            conduct.Run(server.Stats, p);
            return index;
        }
        public Layer CloneInServer(Server server)
        {
            var copy = this.Clone() as Layer;
            copy.server = server;
            return copy;
        }
        object Clone()
        {
            var newL = new Layer() { 
                behaviors = behaviors.Select(x => x.Clone() as Behavior).ToList(), 
                behaviorSelector = behaviorSelector
            };
            if(decisor is not null)
                newL.AssociateDecisor(decisor.Clone() as IDecisor);
            return newL;
        }

        public void SetBehaviourSelector(Func<IEnumerable<Behavior>, int>selector)
        {
            behaviorSelector = selector;
        }

        public void AddBehavior(Behavior beha)
        {
            behaviors.Add(beha);
        }
    }

}

/*

history = [ ]
req = [ r1, r2 ]
ask req all

history.add(env.time, req)


status.reward(...)
*/