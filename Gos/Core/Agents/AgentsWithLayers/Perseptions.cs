using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    // Cualquier evento que le toque ejecutarse en algun punto de la simulacion.
    public abstract class Perception : Event{
        string receiver;
        internal Env env;
        public Perception(string receiver) : base(){
            this.receiver = receiver;
        }
        // Se ejecuta al salir de la cola de prioridad en el Environment en su tiempo correspondiente.
        // Le hace conocer al servidor que tiene que manejar su llegada.
        public override void ExecuteInTime(){
            var rec = Env.CurrentEnv.GetServerByID(this.receiver);
            if(rec != null)
                rec.HandlePerception(this);
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
        public string Receiver {get;}
        public RequestType Type {get;}
        public Message(string sender, string receiver, RequestType type): base(receiver){
            this.Sender = sender;
            this.Receiver = receiver;
        }
    }

    //Un Request con informacion como:
    //  -ID
    //  -URL asociada.
    public class Request:Message{
        // public Request RndClientReq => GetRndClientReq();

        // private static Request GetRndClientReq()
        // {
        //     var clientId = "0"; // id de los clientes
        //     var destServer = Env.CurrentEnv.GetRndEntryPoint();
        //     var askRscs = Resource.GetRndFinishedRscs();

        //     var req = new Request(clientId, destServer, RequestType.DoSomething);
        //     req.AskingRscs = askRscs;
        // }

        static int lastRequestID = 0; 
        public int ID {get;}
        public List<Resource> AskingRscs { get; set; }
        public Request(string sender, string receiver, RequestType type) : base(sender,receiver, type){
            this.ID = ++lastRequestID; 
            this.MatureTime = 1;
            AskingRscs = new();
        }

        // Crea un reponse a partir del request, en sentido contrario, con el campo data
        public Response MakeResponse(Dictionary<string, bool> data){
            return new Response(
                this.ID,
                this.Receiver,
                this.Sender,
                this.Type,
                data
            );
        }
    
    }
    //Un Response con informacion como:
    //  - ID del request asociado.
    //  - Los datos que contiene el response asociados a lo solicitado por el request. ( por ahora es un diccionario de strings )
    public class Response : Message{
        public int ReqID {get; private set;}
        public Dictionary<string,bool> AnswerRscs {get;}
        public Response(int requestID, string sender, string receiver, 
                RequestType type, Dictionary<string, bool> data ) : base(sender, receiver, type){
            this.ReqID = requestID;
            this.AnswerRscs = data;
            this.MatureTime = 1;
        }

        public void Reassign(int reqID){
            this.ReqID = reqID;
        }
        // Crea un response union, quedandose con los valores en True de los values alguna Data.
        public static Response Union(Response r1, Response r2){
            var list = r1.AnswerRscs.Concat(r2.AnswerRscs);

            var re = from kv in list
                        group kv by kv.Key into gr
                        select new
                        {
                            Key = gr.Key,
                            Value = gr.Where(x => x.Value is true).Count() > 0 ? true : false
                        };
            Dictionary<string, bool> unionData = re.ToDictionary(x => x.Key, y => y.Value);

            return new Response(
                r1.ReqID,
                r1.Sender,
                r1.Receiver,
                r1.Type,
                unionData
            );
        }
    
    }

    // Un Observer esta encargado de informarle a un servidor que debe manejar un su estado interno, 
    // es util cuando se usan cronomtros etc,
    // contiene el objeto Objective, que es un objeto que identifica lo que va a suceder dentro del server que suscribio el Observer. 
    public class Observer:Perception{
        // public object Objetive {get;}
        public Observer(string sender) : base(sender){ }
    }

    
    public enum RequestType{
        AskSomething,
        DoSomething,
        Ping
    }
    
    [TestClass]
    public class PerceptionTests {

        [TestMethod]
        public void ResponesUnionTest(){
            var r1 = new Response(1, "sender", "receiber", RequestType.AskSomething, new Dictionary<string, bool>{
                {"r1", true},
                {"r2", false},
                {"r3", true}
            });
            var r2 = new Response(1, "sender", "receiber", RequestType.AskSomething, new Dictionary<string, bool>{
                {"r1", false},
                {"r2", true},
                {"r3", true},
                {"r4", true}
            });
            var r3 = Response.Union(r1, r2);
            Assert.AreEqual(4, r3.AnswerRscs.Count);
            Assert.AreEqual(r3.AnswerRscs["r1"], true);
            Assert.AreEqual(r3.AnswerRscs["r2"], true);
            Assert.AreEqual(r3.AnswerRscs["r3"], true);
            Assert.AreEqual(r3.AnswerRscs["r4"], true);
        }
        

    }
}