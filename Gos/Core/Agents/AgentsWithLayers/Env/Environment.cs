using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
namespace ServersWithLayers
{
    public class Env {

        public static Env CurrentEnv {get; private set;}
        public static int Time => CurrentEnv.currentTime;
        Dictionary<string,Server> servers; //todos los servidores registrados en este enviroment.
        public List<Response> solutionResponses; //poner privado y hacer como que un Enumerable :D
        public int currentTime {get; private set;} // El tiempo actual en la simulacion
        private Utils.Heap<Event> turn; // Cola de prioridad, con los eventos ordenados por tiempo.

        private static string main = "Main";
        public Env(){
            Env.CurrentEnv = this;
            currentTime = 0;
            this.servers = new();
            turn = new();
            solutionResponses = new();
             
            new MicroService(main).SetAsEntryPoint(); // crea el microservicio principal
        }

        internal List<Server> GetServers(Func<Server, bool> filter = null)
        {
            return servers.Select(s => s.Value).Where(filter ?? (s => true)).ToList();
        }
        internal void FailureInServer(string serverName)
        {
            var server = servers[serverName];
            server.Failure();
        }

        public void AddServerList(List<Server> servers){
            foreach(var server in servers){
                server.SetMServiceIfNull(main);
                AddServer(server);
            }
            
        }
        private void AddServer(Server s){
            servers.Add(s.ID, s);
        }
        public IEnumerable<Action> EnumerateActions(){
            while (turn.Count != 0){
                (int time, Event exe ) = this.turn.RemoveMin();
                this.currentTime = time;
                yield return exe.ExecuteInTime;
            }
        }
        public Event FirstEvent(){
            return turn.First.Item2;
        }
        //Ejecuta la simulacion.
        public void Run(){
            foreach (var item in this.EnumerateActions())
                item();
        }

        //Suscribe un evento en el environment.
        public void SubsribeEvent(int time, Event e){
            turn.Add(time + e.MatureTime, e);
        }
        //Retorna un servidor con el identificador 'ID' si esta en el environment.
        public Server GetServerByID(string ID){
            if (this.servers.ContainsKey(ID))
                return this.servers[ID];
            return null;
        }

        internal string GetRndEntryPoint()
        {
            var entryPoints = MicroService.Services
                .Where(pair => pair.Value.Type is ServiceType.EntryPoint)
                .Select(pair => pair.Value.LeaderId);

            return entryPoints.ElementAt(UtilsT.Rand.Next(entryPoints.Count()));
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


Expo: ///////////////////
- Hablar de la versatilidad de Eventos y percepciones (si lo hacemos)

*/