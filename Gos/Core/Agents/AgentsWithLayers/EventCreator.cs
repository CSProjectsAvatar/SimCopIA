using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Utils;
namespace ServersWithLayers{
    public abstract class EventCreator{

        List<Type> _eventTypes;
        private List<double> _accProbabilities;

        public EventCreator(List<Type> eventTypes, List<double> probabilities){
            SetTypes(eventTypes);
            SetProbabilities(probabilities);
        }
        public EventCreator(List<Type> eventTypes) : this(eventTypes,
            // set 1/eventTypes.count in each element of the prob list
            Enumerable.Repeat(1.0/eventTypes.Count, eventTypes.Count).ToList()
        ){}

        internal void SetTypes(List<Type> eventTypes){
            _eventTypes = eventTypes;
        }
        internal void SetProbabilities(List<double> probability){
            if (probability.Count != _eventTypes.Count)
                throw new GoSException("Probabylities count must be equal to event types count");
            var sum = probability.Sum();
            if (sum != 1)
                throw new GoSException("Probabylities must sum up to 1");

            _accProbabilities = probability.Select(
                (x, i) => x + (i == 0 ? 0 : _accProbabilities[i - 1])).ToList();
        }
        
        /// <summary>
        /// Returns random pair of (event, arrivalTime)
        /// </summary>
        public List<(Event, int)> GetEvents(int quantity)
        {// gets 'quantity' number of events from 'EventItertor' with LINQ
            var events = EventItertor().Take(quantity).ToList();
            return events;
        }
        public IEnumerable<(Event, int)> EventItertor()
        {
            while(true){
                var time = (int)UtilsT.GenTimeOffset();
                var type = GetType(UtilsT.Rand.NextDouble());

                yield return (BuildEvent(type), time);
            }
        }

        private Type GetType(double v)
        {// returns type of event by probability using '_accProbabilities'
            if(v < 0 || v > 1)
                throw new Exception("Probability must be between 0 and 1");

            var index = _accProbabilities.FindIndex(x => x > v);
            return _eventTypes[index];
        }

        private Event BuildEvent(Type eType)
        {
            return eType.Name switch {
                "Request" => Request.RndClientReq,

                _ => throw new Exception("Event type not found")
            };
        }
        
        
    }

}
