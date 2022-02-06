using System;
using System.Collections.Generic;
using Utils;

namespace ServersWithLayers{
    public abstract class Event{
        public int MatureTime {get; protected set;}
        public Event(){}
        public Event(int matureTime){
            this.MatureTime = matureTime;
        }
        public abstract void ExecuteInTime();
        
    }

    //evento de ejemplo, Imprime algo en la terminal 
    public class Writer : Event{
        public string ToWrite{get;set;}
        public override void ExecuteInTime()
        {
            Console.WriteLine($" ({Env.CurrentEnv.currentTime}) WriteEvent: {ToWrite}"); 
        }
    }
    public class CritFailure : Event{
        public override void ExecuteInTime()
        {
            // Select a rnd server and provokes a failure
            var servers = Env.CurrentEnv.GetServers();
            if(servers.Count == 0)
                return;
            var rndServer = servers[UtilsT.Rand.Next(servers.Count)];
            Env.CurrentEnv.FailureInServer(rndServer.ID);
        }
    }
}

