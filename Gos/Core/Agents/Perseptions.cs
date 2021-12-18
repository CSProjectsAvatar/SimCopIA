
using System;
using System.Collections.Generic;

namespace Agents
{
    public class Request:IExecutable{
       
        private static int lastRequestID = 0; 
        public int ID {get;}    // un identificador unico para los request
        public string sender {get;} 
        public int time {get;}
        public Agent agent {get;private set;}
        public Environment environment {get;}
        public Request(string sender,string ID, Environment e){
            this.ID = lastRequestID++;
            this.agent = e.GetAgent(ID);
            environment = e;
            this.sender = sender;
        }
        public void Execute(){

            agent.HandleRequest(this);
        }
        public void MakeResponse(string body){
            environment.SubscribeResponse(new Response(ID,agent,sender,environment,body));
        }
        public void ChangeReciever(Agent a){
            this.agent = a; 
        }
    }
    public class Response:IExecutable{
        public int requestID {get;}
        public Agent sender{get;} 
        public string receiver{get;}
        public int responseTime{get;private set;}
        public string body{get;}
        public Environment env{get;}
        public Response(int requestID,Agent sender, string receiver, Environment env, string body){
            this.requestID = requestID;
            this.sender = sender;
            this.receiver = receiver;
            this.env = env;
            this.responseTime = -1;  
            this.body = body;
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