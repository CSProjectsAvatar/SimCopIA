using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    public class Request:Message {
        public Request RndClientReq => GetRndClientReq();
        private static Request GetRndClientReq()
        {
            var clientId = "0"; // id de los clientes
            var destServer = Env.CurrentEnv.GetRndEntryPoint();
            var askRscs = Resource.GetRndFinishedRscs();

            var req = new Request(clientId, destServer, RequestType.DoSomething);
            req.AskingRscs = askRscs;
            return req;
        }
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
    public enum RequestType{
        AskSomething,
        DoSomething,
        Ping
    }
}