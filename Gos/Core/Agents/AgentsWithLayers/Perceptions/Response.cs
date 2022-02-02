using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    //Un Response con informacion como:
    //  - ID del request asociado.
    //  - Los datos que contiene el response asociados a lo solicitado por el request. ( por ahora es un diccionario de strings )
    public class Response : Message{
        public int ReqID {get; private set;}
        public Dictionary<string,bool> AnswerRscs {get;}
        public Response(int requestID, string sender, string receiver, 
                ReqType type, Dictionary<string, bool> data ) : base(sender, receiver, type){
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

    [TestClass]
    public class PerceptionTests {

        [TestMethod]
        public void ResponesUnionTest(){
            var r1 = new Response(1, "sender", "receiber", ReqType.Asking, new Dictionary<string, bool>{
                {"r1", true},
                {"r2", false},
                {"r3", true}
            });
            var r2 = new Response(1, "sender", "receiber", ReqType.Asking, new Dictionary<string, bool>{
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