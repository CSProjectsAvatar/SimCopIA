using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers
{
    public class Env {

        public static Env CurrentEnv {get; private set;}
        public static int Time => CurrentEnv.currentTime;
        Dictionary<string,Server> servers; //todos los servidores registrados en este enviroment.
        public List<Response> solutionResponses 
        => (from tR in GetClientResponses() select tR.Item2).ToList();
        public int currentTime {get; private set;} // El tiempo actual en la simulacion
        private Utils.Heap<Event> turn; // Cola de prioridad, con los eventos ordenados por tiempo.

        private Server _clientServer;

        private static string main = "Main";
        public Env(){
            Env.CurrentEnv = this;
            currentTime = 0;
            this.servers = new();
            turn = new();
             
            new MicroService(main); // crea el microservicio principal

            InitializeClientServer();

        }
        private void InitializeClientServer(){
            this._clientServer = new("0");
            new MicroService("CLIENTE");
            this._clientServer.SetMService("CLIENTE");
            Layer l = new Layer();
            l.behaviors = new List<Behavior>{LoggerBehav.LoggerBehavior};
            this._clientServer.AddLayer(l);
            this.AddServer(this._clientServer);
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

            return entryPoints.ElementAt(new Random().Next(entryPoints.Count()));
        }

        public IEnumerable<(int,Response)> GetClientResponses() {
            var l=LoggerBehav.GetResponseList(this._clientServer,0);
            if(l == null) 
                return new List<(int,Response)>();
            return l;
        }
        public IEnumerable<(int,string)> GetClientReciveLog() {
            var l = LoggerBehav.GetLogList(this._clientServer,0);
            if(l == null) 
                return new List<(int,string)>();
            return l;
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


*/