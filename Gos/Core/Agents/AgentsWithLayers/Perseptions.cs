using System.Collections.Generic;

namespace ServersWithLayers
{
    public abstract class Perseption{
        string receiber;
        protected Environment env;

        public Perseption(string receiber, Environment env){
            this.receiber = receiber;
            this.env = env;
        }

        // Se ejecuta al salir de la cola de prioridad en el Environment en su tiempo correspondiente.
        // Le hace conocer al servidor que tiene que manejar su llegada.
        public void ExecuteInTime(){
            var receiber=env.GetServerByID(this.receiber);
            if(receiber != null)
                receiber.HandlePerseption(this);
            else{
                //no hacer nada si no se encontro un server con ese ID
            }
        }
    }
    //Un Request con informacion como:
    //  -ID
    //  -quien lo manda (sender),
    //  -quien lo debe recibir (recieber) y
    //  -URL asociada.
    public class Request:Perseption{
        
        static int lastRequestID = 0; 
        public int ID {get;}
        string sender; 
        string receiber;
        string URL;
        public Request(string sender, string receiber, Environment env, string URL) : base(receiber, env){
            this.ID = ++lastRequestID; 
            this.sender = sender;
            this.URL = URL;
        }
 
    }
    //Un Response con informacion como:
    //  -ID del request asociado,
    //  -quien lo manda (sender) y
    //  -quien lo debe recibir (recieber).
    public class Response:Perseption{
        string requestID;
        string sender; 
        public Response(string requestID, string sender, string receiber, Environment env) : base(receiber, env){
            this.requestID = requestID;
            this.sender = sender;
        }

    }

    //Un Observer esta encargado de informarle a un servidor que debe manejar un su estado interno, 
    //es util cuando se usan cronomtros etc,
    //contiene el objeto Objective, que es un objeto que identifica lo que va a suceder dentro del server que suscribio el Observer. 
    public class Observer:Perseption{
        public object Objective {get;}
        public Observer( string receiber, object obj, Environment env, string URL) : base(receiber, env){
            this.Objective=  obj; 
        }
    }
}