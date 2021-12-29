using System;
using System.Collections.Generic;

namespace Agents{

    // Cualquiera que sea capaz de ejecutarse en un momento dado en la linea de tiempo del Environment.
    public interface IExecutable{
        void Execute();
    }
    // El medio de ejecucion de los agentes.
    public class Environment{
        
        const int RESPONSE_TIME = 2;
        const int REQUEST_TIME = 1;
        List<Agent> agents;
        public List<Response> solutionResponses; // poner privado y hacer como que un Enumerable :D
        public int currentTime {get; set;}

        public Environment(bool debug=false){
			this.debug = debug;
            currentTime = 0;
            agents = new();
            turn = new();
            solutionResponses = new();
        }
        private Utils.Heap<IExecutable> turn; //el proximo evento que le toca ejecutarse...
        public IEnumerable<Action> EnumerateActions(){
            while (turn.Count != 0){
                (int time, IExecutable exe ) = this.turn.RemoveMin();
                this.currentTime = time;
                yield return exe.Execute;
            }
        }
        public void Run(){
            foreach (var item in this.EnumerateActions())
                item();
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
		bool debug;
        public void SubsribeEvent(IExecutable e, int time){
            if(e is Request) 
                turn.Add(time + REQUEST_TIME,e);
            else if(e is Response) 
                turn.Add(time + RESPONSE_TIME,e);
            else 
                turn.Add(time,e);

        }
        public void PrintAgent(Agent a,string toPrint){ // debug...
			if( !this.debug ) return;
            System.Console.WriteLine($"( Time:{this.currentTime},  Agent:{a.ID} ) - {toPrint}"); 
        }

        public void AddRequest(string sender, string toAgent, string url, int time)//con URL
            => turn.Add(time, new Request(sender, toAgent, this,url));
        public void AddRequest(string sender,string toAgent, int time)
            => turn.Add(time,new Request(sender,toAgent,this));
        public void AddRequest(Request r, int time) => turn.Add(time, r);
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
            private Agent RegisterAgent(Agent a){
                env.agents.Add(a);
                return a;
            }
            public AgentCreator(Environment env){
                this.env = env;
            }
            public Worker Worker(string ID=""){
                string id =  ID == "" ? (nextInt++).ToString() : ID;
                var agent = new Worker(env, id);
                return RegisterAgent(agent) as Worker;
            } 
            public Distributor Distributor(List<string> workers,string ID=""){
                string id =  ID == "" ? (nextInt++).ToString() : ID;
                var agent = new Distributor(env,id,workers);
                return RegisterAgent(agent) as Distributor;
            }

            public InteractiveWorker InteractiveWorker(string ID="")
            {
                string id =  ID == "" ? (nextInt++).ToString() : ID;
                var agent = new InteractiveWorker(env, id);
                return RegisterAgent(agent) as InteractiveWorker;
            }
        }
    }
} 
