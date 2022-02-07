using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    // Cualquier evento que le toque ejecutarse en algun punto de la simulacion.
    public abstract class Perception : Event{
        public string Receiver;
        //internal Env env;

        public Perception(string receiver) : base(){
            this.Receiver = receiver;
        }
        // Se ejecuta al salir de la cola de prioridad en el Environment en su tiempo correspondiente.
        // Le hace conocer al servidor que tiene que manejar su llegada.
        public override void ExecuteInTime(){
            var rec = Env.CurrentEnv.GetServerByID(this.Receiver);
            if(rec != null)
                rec.HandlePerception(this);
            else{
                //no hacer nada si no se encontro un server con ese ID
            }
        }
    }
   
}