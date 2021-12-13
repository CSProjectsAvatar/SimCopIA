using System;
using System.Collections.Generic;
public interface IExecutable{
    void Execute();
}

public class PackageSender:IExecutable{
    public Agent agent {get;}
    public Environment environment {get;}
    public PackageSender(Agent a, Environment e){
        agent = a;
        environment = e;
    }
    public void Execute(){
        System.Console.WriteLine($"{this.environment.currentTime} Llega paquete al servidor {this.agent}");//debug
        agent.SetPerception(Perception.IsDataAvailable);
        agent.Execute();
        agent.SetPerception(Perception.NotData);
    }
}
public class Agent : IExecutable{
    private Perception currentPerception;
    protected Status status {get; set;}
    public Environment environment {get;}
    public Agent(Environment e){
        this.environment = e;    
    }

    public void SetPerception(Perception p){
        currentPerception = p;
    }

    public void Execute( ){
        this.status=status.Next(currentPerception);
        this.status.Action();
    }
}

#region Ejemplo de agente
public class SimpleServer : Agent{ 
    public SimpleServer (Environment env): base(env){
        this.status = new Available(this);
    }
    
}
#endregion
public abstract class Status{
    protected  Agent agent{get;}
    protected Environment environment => agent.environment;  
    public Status(Agent a){
        this.agent = a;
    }
    public abstract Status Next(Perception p);
    public abstract void Action();
   
}

#region ejemplos de estados... 
public class Available:Status{
    public Available(Agent a): base(a){
    }
    public override Status Next(Perception p){
        if (p == Perception.IsDataAvailable)
        {
            return new Unavailable(this.agent);
        }    
        else return this;
    }
    public override void Action()
    {
    }
}
public class Unavailable:Status{
    public int EndTime {get;}
    public int PackagesOnQueue {get;private set;}
    public Unavailable(Agent a): base(a){
        this.EndTime = this.environment.currentTime + _genTimeOffset() ;
        this.environment.SubsribeEvent(this.agent, this.EndTime);
    }
    public Unavailable(Agent a,int packagesOnQueue):this(a){
        this.PackagesOnQueue = packagesOnQueue;
    }

    void AddPackageToQueue()=> PackagesOnQueue++; 
    public override Status Next(Perception p ){
        if (p == Perception.IsDataAvailable && this.environment.currentTime < EndTime){
            AddPackageToQueue();
            return this;
        }else if (PackagesOnQueue>0 && EndTime == this.environment.currentTime){
            System.Console.WriteLine($"{this.environment.currentTime} Sale paquete de {this.agent} y se pone en cola el proximo, quedan en cola {this.PackagesOnQueue -1}");//debug
            return new Unavailable(this.agent,PackagesOnQueue-1);
        }else if (EndTime== this.environment.currentTime) {
            System.Console.WriteLine($"{this.environment.currentTime} Sale paquete de {this.agent}");//debug
            return new Available(this.agent);
        }else{
            return this;
        }
    }
    public override void Action(){
    }
    private int _genTimeOffset() {
        var _rand = new Random();
        return _rand.Next(5,20); 
    } 
}

#endregion 

public enum Perception{
    IsDataAvailable,
    NotData
}

public class Environment{
    List<Agent> agents;
    public int currentTime {get; set;}
    public Environment(){
        currentTime = 0;
        agents = new();
        turn = new();
    }
    private Utils.Heap<IExecutable> turn; //el proximo evento que le toca ejecutarse...
    public IEnumerable<Action> Enumerable(){
        while (turn.Count != 0){
            (int time, IExecutable exe ) = this.turn.RemoveMin();
            this.currentTime = time;
            yield return exe.Execute;
        }
    }
    public void AddAgent(Agent a){
        agents.Add(a);
    }
    public void SubsribeEvent(IExecutable e, int time){
        turn.Add(time,e);
    }
    
    public void AddPackageSender(Agent toAgent, int time)
        => turn.Add(time,new PackageSender(toAgent,this));
} 