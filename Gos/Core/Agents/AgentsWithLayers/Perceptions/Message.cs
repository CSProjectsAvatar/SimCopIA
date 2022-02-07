using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    /// Un Message es o un Request o un Response y tiene:
    /// -el tipo de request del que se hizo o el tipo del request asociado a un response, 
    /// -quien lo manda (sender),
    /// -quien lo debe recibir (recieber).
    public abstract class Message : Perception{
        static int lastMsgID = 0;

        public int ID {get;}
        public string Sender {get;} 
        // public string Receiver {get;}
        public ReqType Type {get;}
        public Message(string sender, string receiver, ReqType type): base(receiver){
            this.ID = ++lastMsgID; 

            this.Sender = sender;
            // this.Receiver = receiver;
            this.Type = type;
        }
    }

}