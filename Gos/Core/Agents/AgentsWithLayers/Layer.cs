using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Layer{

        internal Server server;
        public Layer(){

        } 

        public void SetServer(Server server){
            this.server = server;
        }
        
        //Aqui la capa hace todo lo referente a modificar el estado de 'server'  o suscribir al environment Perceptions.
        //Esto basado en un una Perception 'p' y el estado interno de 'server'
        public void Process(Perception p){
            throw new NotImplementedException("Layer Class not Implemented!");
        }
        public void SetServer(Server serv){
            this.server = serv ;
        } 
    }

}