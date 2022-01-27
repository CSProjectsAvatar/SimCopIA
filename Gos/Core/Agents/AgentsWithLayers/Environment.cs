using System;
using System.Collections.Generic;

namespace ServersWithLayers
{
    public class Environment{

        public static Environment CurrentEnv {get; private set;}
        const int RESPONSE_TIME = 2; //tiempo de demora de llegada de un response
        const int REQUEST_TIME = 1;  //tiempo de demora de llegada de un request 

        Dictionary<string,Server> servers; //todos los servidores registrados en este enviroment.
        public List<Response> solutionResponses; //poner privado y hacer como que un Enumerable :D
        public int currentTime {get; private set;} // El tiempo actual en la simulacion

        public bool debug{get;set;}
        public Environment(bool debug=false){
            Environment.CurrentEnv = this;
            this.debug = debug;
            currentTime = 0;
            this.servers = new();
            turn = new();
            solutionResponses = new();
        }
        private Utils.Heap<Event> turn; // Cola de prioridad, con los eventos ordenados por tiempo.
        public IEnumerable<Action> EnumerateActions(){
            while (turn.Count != 0){
                (int time, Event exe ) = this.turn.RemoveMin();
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
        public void SubsribeEvent(int time, Event e){
            // Considerar luego otras clases de eventos 
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
/*
Environment:
- Guardar los objetos del ambiente
- correr los eventos

EventCreator:
- Crear eventos

Event:
- Realizar una accion sobre uno o mas objetos del environment.

Resource:
- Unidad principal pedida y transferida entre agentes. (i.e una pagina web, un archivo, una imagen, etc)

IA
Como repartir los recursos

*/