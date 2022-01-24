using System;
using System.Collections.Generic;

namespace ServersWithLayers
{
    public class Environment{
        const int RESPONSE_TIME = 2; //tiempo de demora de llegada de un response
        const int REQUEST_TIME = 1;  //tiempo de demora de llegada de un request 

        Dictionary<string,Server> servers; //todos los servidores registrados en este enviroment.
        public List<Response> solutionResponses; //poner privado y hacer como que un Enumerable :D
        public int currentTime {get; set;} // El tiempo actual en la simulacion

        public bool debug{get;set;}
        public Environment(bool debug=false){
            this.debug = debug;
            currentTime = 0;
            this.servers = new();
            turn = new();
            solutionResponses = new();
        }
        private Utils.Heap<Perception> turn; // Cola de prioridad, con los eventos ordenados por tiempo.
        public IEnumerable<Action> EnumerateActions(){
            while (turn.Count != 0){
                (int time, Perception exe ) = this.turn.RemoveMin();
                this.currentTime = time;
                yield return exe.ExecuteInTime;
            }
        }

        //Ejecuta la simulacion.
        public void Run(){
            foreach (var item in this.EnumerateActions())
                item();
        }

        //Suscribe un evento en el environment.
        public void SubsribeEvent(int time, Perception e){
            if(e is Request) 
                turn.Add(time + REQUEST_TIME,e);
            else if(e is Response) 
                turn.Add(time + RESPONSE_TIME,e);
            else 
                turn.Add(time,e);

        }
        //Retorna un servidor con el identificador 'ID' si esta en el environment.
        public Server GetServerByID(string ID){
            if (this.servers.ContainsKey(ID))
                return this.servers[ID];
            return null;
        }
    }
}