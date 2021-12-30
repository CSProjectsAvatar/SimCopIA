
using System;
using System.Collections.Generic;

namespace Agents
{
    public class Request:IExecutable{
       
        private static int lastRequestID = 0; 
        public int ID {get;}    // un identificador unico para los request
        public string URL { get; }
        public string sender {get;} 
        public int time {get;}
        public bool satisfied { get;  set; }
        public Agent agent {get;private set;}
        public Environment environment {get;}
        public bool ToKnowAvailibility {get; private set;}
        public Request(string sender,string ID, Environment e){
            this.ID = lastRequestID++;
            this.agent = e.GetAgent(ID);
            environment = e;
            this.sender = sender;
        }
        public Request(string sender, string ID, Environment e, bool forKnowAvailibity) : this(sender,ID,e)
            => this.ToKnowAvailibility = forKnowAvailibity;
        public Request(string sender, string ID, Environment e,string URL)//con url
        {
            this.ID = lastRequestID++;
            this.agent = e.GetAgent(ID);
            environment = e;
            this.sender = sender;
            this.URL = URL;
        }
        public void Execute(){

            this.satisfied = true;
            agent.HandleRequest(this);
        }
        public void MakeResponse(string body){
            environment.SubscribeResponse(new Response(ID,agent,sender,environment,body));
        }
        public void ChangeReciever(Agent a){
            this.agent = a; 
        }
        public void SetKnowAvailibity(bool b) => this.ToKnowAvailibility = b;
    }
    public class Response:IExecutable{
        public int requestID {get;}
        public Agent sender{get;} 
        public string receiver{get;}
        public int responseTime{get;private set;}
        public string body{get;}
        public Environment env{get;}
        
        //para preguntar si esta disponible.          
        public bool ToKnowAvailibility {get; private set;}
        public bool IsAvailable{get; private set;}

        public Response(int requestID,Agent sender, string receiver, Environment env, string body){
            this.requestID = requestID;
            this.sender = sender;
            this.receiver = receiver;
            this.env = env;
            this.responseTime = -1;  
            this.body = body;
        }

        // response para conocer si un servidor esta disponible
        public Response(int requestID,Agent sender, string receiver, Environment env, bool isAvailable) : this(requestID,sender,receiver,env,"AvailibityChecker:"+isAvailable){
            this.ToKnowAvailibility = true;
            this.IsAvailable = isAvailable;
        }
        public void Execute(){
            if(receiver == "0"){ // si es "0" es que el request asociado venia del response.
                this.env.AddSolutionResponse(this);
                return;
            }
            var agent=env.GetAgent(receiver);
            agent.HandleResponse(this);
        }
        public void SetTime(int time) => this.responseTime = time;
        public void SetKnowAvailibity(bool b) => this.ToKnowAvailibility = true;
    }
    public class Observer:IExecutable{
        public Agent agent {get;}
        public object Object{get;}
        private Environment env;
        public Observer(Agent a, Environment env, object o){
            this.agent= a;
            this.env = env;
            this.Object = o;
        }
        public void Execute(){
            agent.HandleStatus(this);
        }

    }
}