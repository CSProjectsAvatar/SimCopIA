using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    public class Request:Message {
        public static Request RndClientReq => GetRndClientReq();
        private static Request GetRndClientReq() //@todo Omar: review this
        {
            var clientId = "0"; // id de los clientes
            var destServer = Env.CurrentEnv.GetRndEntryPoint();
            var askRscs = Resource.GetRndFinishedRscs();

            var req = new Request(clientId, destServer, ReqType.DoIt);
            req.AskingRscs = askRscs;
            return req;
        }
        
        private List<Resource> _askingRscs;
        public List<Resource> AskingRscs {
            get =>_askingRscs;
            set {
                _askingRscs = value;
                if (Type is ReqType.DoIt && _askingRscs.Count == 0)
                    throw new Exception("Si el Request es de tipo DoIt la lista de pedidos no puede estar vacia");
            }
        }
        public Request(string sender, string receiver, ReqType type) : base(sender,receiver, type){
            this.MatureTime = 1;
            AskingRscs = new();
        }
        // Crea un reponse a partir del request, en sentido contrario, con el campo data
        public Response MakeResponse(Dictionary<string, bool> data = null){
            return new Response(
                this.ID,
                this.Receiver,
                this.Sender,
                this.Type,
                data
            );
        }
    }
    public enum ReqType{
        Asking,
        DoIt,
        Ping
    }
}