using System;
using System.Collections.Generic;

namespace AgentesDiscretos{

    // Cualquiera que sea capaz de ejecutarse en un momento dado
    public interface IExecutable{
        void Execute();
    }

    // Se Crea cada vez que se va a mandar a un agente un paquete
    public class Request:IExecutable{
       
        public string sender {get;} 
        public int time {get;}
        public Agent agent {get;}
        public Environment environment {get;}
        public Request(string sender,string ID, Environment e){
            this.agent = e.GetAgent(ID);
            environment = e;
            this.sender = sender;
        }
        public void Execute(){

            environment.PrintAgent(agent,"LLega paquete.");
            agent.HandleRequest(this);
        }
        public void MakeResponse(string body){
            environment.SubscribeResponse(new Response(agent,sender,environment,body));
        }
    }
    public class Response:IExecutable{
        public Agent sender{get;} 
        public string receiver{get;}
        public int responseTime{get;private set;}
        public string body{get;}
        public Environment env{get;}
        public Response(Agent sender, string receiver, Environment env, string body){
            this.sender = sender;
            this.receiver = receiver;
            this.env = env;
            this.responseTime = -1;
            this.body = body;
        }
        public void Execute(){
            if(receiver == "0"){
                this.env.AddSolutionResponse(this);
                return;
            }
            var agent=env.GetAgent(receiver);
            agent.HandleResponse(this);
        }
        public void SetTime(int time) => this.responseTime = time;
    }

    // Un agente con una funcion de execute que llama las funciones de los estados, Next y Action 
    // La percepcion del agente se cambia usando SetPerception (usando por ejemplo un Request)
    public abstract class Agent : IExecutable{
        public string ID{get;}
        public Environment environment {get;}
        public Agent(Environment e, string ID){
            this.environment = e;    
            this.ID = ID;
        }


        public abstract void HandleRequest(Request r);
        public abstract void HandleResponse(Response r);
        public void Execute(){
        }

    }

    #region Ejemplo de agente

    //un agente simple, con los estados Available y las variaciones de Unavailable.
    public class SimpleServer : Agent{ 
        public SimpleServer (Environment env, string ID): base(env, ID){
        }

        public override void HandleRequest(Request r)
        {
            if (r.sender == "0"){
               r.MakeResponse("Cosa de servidor simple!!");
               return;
            }
            throw new NotImplementedException();
        }
        public override void HandleResponse(Response r)
        {
            
        }

    }
    #endregion

    
    
    public class Environment{
 
        const int RESPONSE_TIME = 2;
        List<Agent> agents;
        public List<Response> solutionResponses; // poner privado :D
        public int currentTime {get; set;}

        public Environment(){
            currentTime = 0;
            agents = new();
            turn = new();
            solutionResponses = new();
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
        public Agent GetAgent(string ID){
            foreach(var a in agents)
                if (a.ID == ID)
                    return a;
            return null;
        }
        public void SubsribeEvent(IExecutable e, int time){
            turn.Add(time,e);
        }
        public void PrintAgent(Agent a,string toPrint){ // debug...
            System.Console.WriteLine($"( Time:{this.currentTime},  Agent:{a.ID} ) - {toPrint}"); 
        } 
        public void AddRequest(string sender,string toAgent, int time)
            => turn.Add(time,new Request(sender,toAgent,this));
        public void SubscribeResponse(Response r){
            int time = this.currentTime + RESPONSE_TIME;
            r.SetTime(time);
            turn.Add(time, r);
        }
        public void AddSolutionResponse(Response r)
            => this.solutionResponses.Add(r);

        public AgentCreator Build => new AgentCreator(this);
        public class AgentCreator{
            Environment env; 
            private static int nextInt = 1; 
            public AgentCreator(Environment env){
                this.env = env;
            }
            public SimpleServer SimpleServer(){
                var agent = new SimpleServer(env, (nextInt++).ToString());
                env.agents.Add(agent);
                return agent;

            } 
        }
    }
} 