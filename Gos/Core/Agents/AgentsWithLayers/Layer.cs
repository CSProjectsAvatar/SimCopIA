using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Layer{

        Server server;
        public Layer(Server server){
            this.server = server;
        } 
        
        //Aqui la capa hace todo lo referente a modificar el estado de 'server'  o suscribir al environment Perseptions.
        //Esto basado en un una Perseption 'p' y el estado interno de 'server'
        public void Execute(Perseption p){
            throw new NotImplementedException("Layer Class not Implemented!");
        }
    }

}