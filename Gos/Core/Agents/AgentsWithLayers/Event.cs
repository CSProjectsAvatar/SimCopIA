using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public abstract class Event{
        public abstract void ExecuteInTime();
    }

    //evento de ejemplo, Imprime algo en la terminal 
    public class Writer : Event{
        public string ToWrite{get;set;}
        public override void ExecuteInTime()
        {
            Console.WriteLine($" ({Environment.CurrentEnv.currentTime}) WriteEvent: {ToWrite}"); 
        }
    }
}

