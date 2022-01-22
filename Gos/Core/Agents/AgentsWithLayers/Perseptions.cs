using System.Collections.Generic;

namespace ServersWithLayers
{
    // Cualquier evento que le toque ejecutarse en algun punto de la simulacion.
    public abstract class Perception{
        string receiber;
        protected Environment env;

        public Perception( Environment env){
            this.env = env;
        }

        // Se ejecuta al salir de la cola de prioridad en el Environment en su tiempo correspondiente.
        // Le hace conocer al servidor que tiene que manejar su llegada.
        public void ExecuteInTime(){
            var receiber=env.GetServerByID(this.receiber);
            if(receiber != null)
                receiber.HandlePerception(this);
            else{
                //no hacer nada si no se encontro un server con ese ID
            }
        }
    }

    /// Un Message es o un Request o un Response y tiene:
    /// -el tipo de request del que se hizo o el tipo del request asociado a un response, 
    /// -quien lo manda (sender),
    /// -quien lo debe recibir (recieber).
    public abstract class Message : Perception{
        public string Sender {get;} 
        public string Receiber {get;}
        public RequestType Type {get;}
        public Message(string sender, string receiber, RequestType type, Environment env): base(env){
            this.Sender = sender;
            this.Receiber = receiber;
        }
    }

    //Un Request con informacion como:
    //  -ID
    //  -URL asociada.
    public class Request:Message{
        
        static int lastRequestID = 0; 
        public int ID {get;}

        string URL;
        public Request(string sender, string receiber, RequestType type, Environment env, string URL="/") : base(sender,receiber, type, env){
            this.ID = ++lastRequestID; 
            this.URL = URL;
        }

        // Crea un reponse a partir del request,
        // esta dirigido a el que envio el request y lo manda el que recibio el request,
        // se le agregan los datos que supuestamente pide el request en cuestion.
        public Response MakeResponse(Dictionary<string, string> data){
            return new Response(
                this.ID,
                this.Receiber,
                this.Sender,
                this.Type,
                data,
                this.env
            );
        }
 
    }
    //Un Response con informacion como:
    //  - ID del request asociado.
    //  - Los datos que contiene el response asociados a lo solicitado por el request. ( por ahora es un diccionario de strings )
    public class Response : Message{
        public int RequestID {get;}
        public Dictionary<string,string> Data {get;}
        public Response(int requestID, string sender, string receiber, RequestType type, Dictionary<string, string> data, Environment env) : base(sender, receiber, type, env){
            this.RequestID = requestID;
            this.Data = data;
        }

    }

    // Un Observer esta encargado de informarle a un servidor que debe manejar un su estado interno, 
    // es util cuando se usan cronomtros etc,
    // contiene el objeto Objective, que es un objeto que identifica lo que va a suceder dentro del server que suscribio el Observer. 
    public class Observer:Perception{
        public string Sender;
        public object Objetive {get;}
        public Observer( string sender, object obj, Environment env) : base(env){
            this.Sender = sender;
            this.Objetive=  obj; 
        }
    }

    
    public enum RequestType{
        AskSomething,
        DoSomething,
        Ping

    }
    
}