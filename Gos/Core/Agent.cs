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
        System.Console.WriteLine("Llega paquete al servidor "+ this.agent);//debug
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
        this.status = new Available(this);
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
            System.Console.WriteLine("Se ocupa el servidor "+this.agent);//debug 
            return new Unavailable(this.agent);
        }    
        else return this;
    }
    public override void Action()
    {
    }
}
public class Unavailable:Status{
    public double EndTime {get;}
    public int PackagesOnQueue {get;}
    public Unavailable(Agent a): base(a){
        this.EndTime = this.environment.currentTime + _genTimeOffset() ;
        this.environment.SubsribeEvent(this.agent, this.EndTime);
    }
    public Unavailable(Agent a,int packagesOnQueue):this(a){
        this.PackagesOnQueue = packagesOnQueue;
    }

    public override Status Next(Perception p ){
        if (p == Perception.IsDataAvailable && EndTime < this.environment.currentTime){
            return new Unavailable(this.agent,PackagesOnQueue+1) ;
        }else if (p == Perception.NotData && PackagesOnQueue>1 && EndTime == this.environment.currentTime){
            return new Unavailable(this.agent,PackagesOnQueue-1);
        }else if (EndTime== this.environment.currentTime) {
            System.Console.WriteLine("Sale paquete de "+this.agent);//debug
            return new Available(this.agent);
        }else{
            return this;
        }
    }
    public override void Action(){
    }
    private double _genTimeOffset() {
        var lambda = 1.5;
        var _rand = new Random();
        return -1 / lambda * Math.Log(_rand.NextDouble()); // distribución exponencial
    } 
}

#endregion 

public enum Perception{
    IsDataAvailable,
    NotData
}

public class Environment{
    List<Agent> agents;
    public double currentTime {get; set;}
    public Environment(){
        currentTime = 0;
        agents = new();
        turn = new();
    }
    private Utils.Heap<IExecutable> turn; //el proximo evento que le toca ejecutarse...
    public IEnumerable<Action> Enumerable(){
        while (turn.Count != 0){
            (double time, IExecutable exe ) = this.turn.RemoveMin();
            this.currentTime = time;
            yield return exe.Execute;
        }
    }
    public void AddAgent(Agent a){
        agents.Add(a);
    }
    public void SubsribeEvent(IExecutable e, double time){
        turn.Add(time,e);
    }
    
    public void AddPackageSender(Agent toAgent, double time)
        => turn.Add(time,new PackageSender(toAgent,this));
} 