using System;
using System.Collections.Generic;

// Cualquiera que sea capaz de ejecutarse en un momento dado
public interface IExecutable{
    void Execute();
}

// Se Crea cada vez que se va a mandar a un agente un paquete
public class PackageSender:IExecutable{
    public Agent agent {get;}
    public Environment environment {get;}
    public PackageSender(Agent a, Environment e){
        agent = a;
        environment = e;
    }
    public void Execute(){
        environment.PrintAgent(agent,"LLega paquete.");
        agent.SetPerception(Perception.IsDataAvailable);
        agent.Execute();
        agent.SetPerception(Perception.NotData);
    }
}

// Un agente con una funcion de execute que llama las funciones de los estados, Next y Action 
// La percepcion del agente se cambia usando SetPerception (usando por ejemplo un PackageSender)
public class Agent : IExecutable{
    public int ID{get;}
    private Perception currentPerception;
    protected Status status {get; set;}
    public Agent connectedTo {get; private set;}
    public Environment environment {get;}
    public Agent(Environment e, int ID){
        this.environment = e;    
        this.ID = ID;
    }

    public void SetPerception(Perception p){
        currentPerception = p;
    }

    public void Execute(){
        this.status=status.Next(currentPerception);
        this.status.Action();
    }
    public void Connect(Agent a){
        this.connectedTo = a;
    }

}

#region Ejemplo de agente

//un agente simple, con los estados Available y las variaciones de Unavailable.
public class SimpleServer : Agent{ 
    public SimpleServer (Environment env, int ID): base(env, ID){
        this.status = new Available(this);
    }
    
}
#endregion

// un estado de un agente, tiene una funcion de cambio de estado (Next) y tiene una funcion de interaccion con el ambiente (Action)
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

        if (this.agent.connectedTo != null)
            this.environment.AddPackageSender(this.agent.connectedTo,this.EndTime);

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
            environment.PrintAgent(agent,$"Sale paquete, quedan {PackagesOnQueue-1} en cola.");
            return new Unavailable(this.agent,PackagesOnQueue-1);
        }else if (EndTime== this.environment.currentTime) {
            environment.PrintAgent(agent,$"Sale paquete.");
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
    public void PrintAgent(Agent a,string toPrint){ // debug...
        System.Console.WriteLine($"( Time:{this.currentTime},  Agent:{a.ID} ) - {toPrint}"); 
    } 
    public void AddPackageSender(Agent toAgent, int time)
        => turn.Add(time,new PackageSender(toAgent,this));
        
    public AgentCreator Build => new AgentCreator(this);
    public class AgentCreator{
        Environment env; 
        private static int nextInt; 
        public AgentCreator(Environment env){
            this.env = env;
        }
        public SimpleServer SimpleServer(){
            var agent = new SimpleServer(env, nextInt++);
            env.agents.Add(agent);
            return agent;

        } 
    }
} 