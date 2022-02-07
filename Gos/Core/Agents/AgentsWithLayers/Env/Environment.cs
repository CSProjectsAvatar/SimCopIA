using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Microsoft.Extensions.Logging;

namespace ServersWithLayers
{
    public class Env {

        public static Env CurrentEnv {get; private set;}
        public static int Time => CurrentEnv.currentTime;
        internal int GetEventCount => turn.Count;
        Dictionary<string,Server> servers; //todos los servidores registrados en este enviroment.
        public List<Response> solutionResponses 
        => (from tR in GetClientResponses() select tR.Item2).ToList();

        private List<(int,Request)> clientRequests;
        public int currentTime {get; private set;} // El tiempo actual en la simulacion
        private Utils.Heap<Event> turn; // Cola de prioridad, con los eventos ordenados por tiempo.
        private ILogger<Env> _loggerEnv;

        

        private Server _clientServer;
        public Output Output {get;}

        private static string main = "Main";
        public Env(ILogger<Env> loggerEnv = null, ILogger<MicroService> loggerMS = null)
        {
            Env.CurrentEnv = this;
            currentTime = 0;
            this.servers = new();
            turn = new();
             
            _loggerEnv = loggerEnv;
            new MicroService(main,loggerMS).SetAsEntryPoint(); // crea el microservicio principal
            InitializeClientServer();
            
            this.clientRequests = new();
            this.Output = new Output(this);

        }

        /// <summary>
        /// Used for testing purpuses only
        /// </summary>
        internal void AdvanceTime(int toAdd){
            currentTime += toAdd;

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
                
                _loggerEnv?.LogInformation("Ejecutando evento {exe} en el tiempo {time}", exe.GetType().Name, time);
                yield return exe.ExecuteInTime;
            }
        }
        public Event FirstEvent(){
            return turn.First.Item2;
        }
        //Ejecuta la simulacion.
        public void Run(int maxSimulationTime=10000){
            foreach (var item in this.EnumerateActions()){
                if(this.currentTime > maxSimulationTime)
                    break;
                item();
            }
            Output.ProcessData();
        }

        //Suscribe un evento en el environment.
        public void SubsribeEvent(int time, Event e){
            turn.Add(time + e.MatureTime, e);

            Request r0 = e as Request;
            if( r0 != null && r0.Sender == "0" ){
                clientRequests.Add((time,r0));
            }

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

        // todas los responses enviados al cliente
        public IEnumerable<(int,Response)> GetClientResponses() {
            var l=LoggerBehav.GetResponseList(this._clientServer,0);
            if(l == null) 
                return new List<(int,Response)>();
            return l;
        }
        public IEnumerable<(int,Request)> GetClientRequests(){
            return clientRequests as IEnumerable<(int,Request)>;
        }
        // el string log del cliente
        public IEnumerable<(int,string)> GetClientReciveLog() {
            var l = LoggerBehav.GetLogList(this._clientServer,0);
            if(l == null) 
                return new List<(int,string)>();
            return l;
        } 
        // el string log de todos los servidores
        public IEnumerable<(int,string)> GetAllServersLogs(){
            List<(int,string)> logList = new();
            logList.AddRange(GetClientReciveLog());
            foreach (var item in servers)
                logList.AddRange(LoggerBehav.GetLogList(item.Value,0));

            logList.Sort();
            return logList;
        }

        internal static void ClearServersLayers()
        {
            foreach (var server in Env.CurrentEnv.servers.Values)
                server.ClearLayers();
        }

        public void GenerateEventsInTimeRange(List<Type> eventTypes, List<double> probabilities,int totalTime,int initialTime=0){
            var eventCreator = new EventCreator(eventTypes,probabilities);

            int lastEventTime = initialTime; 
            foreach(var e in eventCreator.EventItertor())
            {
                lastEventTime += e.Item2;
                if(lastEventTime > initialTime + totalTime)
                    break;
                this.SubsribeEvent(lastEventTime, e.Item1);
            }
        }
        //tiene algun error
        public void GenerateNEvents(List<Type> eventTypes, List<double> probabilities,int N,int initialTime=0){
            var eventCreator = new EventCreator(eventTypes,probabilities);

            int lastEvent = initialTime;
            foreach(var e in eventCreator.GetEvents(N)){
                lastEvent +=  e.Item2;
                this.SubsribeEvent(lastEvent, e.Item1);
            }
        }
        // Le agrega eventos a ejecutrase al environmen actual y ejecuta la simulacion,
        // de no existir crea uno nuevo.
        public static void RunDefaultCurrentEnv(){
            if(Env.CurrentEnv == null)
                new Env();
            Env.CurrentEnv.RunDefault();
        }
        public void RunDefault(){
            
            List<Type> eventsTypes = new(){
                typeof(Request),
                typeof(CritFailure)
            };
            List<double> probabilities = new(){
                0.98,
                0.02
            };

            this.GenerateEventsInTimeRange(eventsTypes,probabilities,10000);

            this.Run();
        }
        public void Dispose(){
            MicroService.Services.Clear();
            Resource.Resources.Clear();
            Env.ClearServersLayers();
        }

        public double GetMothlyCost(){
            return servers.Values.Select((Server s)=> s.MonthlyCost).Sum();
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