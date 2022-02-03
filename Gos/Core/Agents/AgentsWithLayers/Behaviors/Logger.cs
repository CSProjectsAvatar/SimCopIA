using System.Collections.Generic;

namespace ServersWithLayers{
    public static class LoggerBehav{
        public static Behavior LoggerBehavior = new Behavior(Logger,LoggerInit);
        static string logListB = "logList";
        static string requestsB = "requests";
        static string responsesB = "responses";
        static string observersB = "observers";
        
        private static void LoggerInit(Status status,Dictionary<string, object> vars){
            vars[logListB] = new List<(int,string)>();
            vars[requestsB] = new List<(int,Request)>();
            vars[responsesB] = new List<(int,Response)>();
            vars[observersB] = new List<(int,Observer)>();
        } 
        private static void Logger(Status status, Perception p, Dictionary<string,object> vars)
        {
            var logList  = vars[logListB] as List<(int,string)>;
            var requests =vars[requestsB]  as List<(int,Request)>  ;
            var responses =vars[responsesB]  as List<(int,Response)>;
            var observers =vars[observersB]  as List<(int,Observer)>;

            var currentTime = Env.Time;

            //implementar Clone para las perseption
            switch (p){
                case Request r:
                    requests.Add((currentTime,r));
                    logList.Add((currentTime,"(s:" + status.serverID + "  t:"+ currentTime +") Llega request "+r.ID+" desde " + r.Sender));
                break;
                case Response r:
                    responses.Add((Env.Time,r));
                    logList.Add((currentTime,"(s:" + status.serverID + "  t:"+currentTime+") Llega response al request" + r.ReqID + " desde " + r.Sender));
                break;
                case Observer o:
                    observers.Add((Env.Time,o));
                    logList.Add((currentTime,"(s:" + status.serverID + "  t:"+currentTime+") Llega observer" ));
                break;
            }

        }
        public static List<(int, string)> GetLogList(Server s, int loggerLayerIndex){
            var logList = (s.GetLayerBehaVars(loggerLayerIndex,"logList") as List<(int, string)> );
            return logList;
        }

        public static List<(int, Request)> GetRequestList(Server s, int loggerLayerIndex){
            var requests = (s.GetLayerBehaVars(loggerLayerIndex,"requests") as List<(int, Request)> );
            return requests;
        }
        public static List<(int, Response)> GetResponseList(Server s, int loggerLayerIndex){
            var responses = (s.GetLayerBehaVars(loggerLayerIndex,"responses") as List<(int, Response)> );
            return responses;
        }

        // no probado
        public static void PrintRequest(object requests_as_object){
            var requests =requests_as_object as List<(int,Request)>  ;
            System.Console.WriteLine("Requests:");
            foreach(var r in requests) 
                System.Console.WriteLine(" t:"+r.Item1+" req:"+r.Item2.ID+"  ("+r.Item2.Sender+" --> "+r.Item2.Receiver+")");
        }
        // no probado
        public static void PrintResponse(object responses_as_object){
            var responses =responses_as_object  as List<(int,Response)>  ;
            System.Console.WriteLine("Responses:");
            foreach(var r in responses) 
                System.Console.WriteLine(" t:"+r.Item1+" req:"+r.Item2.ReqID+"  ("+r.Item2.Sender+" --> "+r.Item2.Receiver+")");
        }

    }
}