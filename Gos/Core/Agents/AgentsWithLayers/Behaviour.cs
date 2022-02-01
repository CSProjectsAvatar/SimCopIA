using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public class Behavior
    {
        Dictionary<string, object> variables;
        internal Action<Status, Dictionary<string, object>> _init;
        Action<Status, Perception, Dictionary<string, object>> action;
        
        bool _firstTime = true;
        public Behavior():this(null) { }
        public Behavior(Action<Status, Perception, Dictionary<string, object>> action, Action<Status, Dictionary<string, object>> init = null)
        {
            this.action = action;
            this.variables = new Dictionary<string, object>();
            _init = init;
        }

        public void SetVar(string name, object value)
        {
            variables[name] = value;
        }

        public void Run(Status status, Perception perception) {
            if(_firstTime){
                _firstTime = false;
                if(_init is not null) _init(status, variables);
            }
            action(status, perception, variables);
         }

        public Behavior Clone()
        {
            var copy = new Behavior();  //@todo review dict var
            copy.action += this.action;
            return copy;
        }
    }
} 
/*
Donde IA:
- Eleccion cantidad de reputacion que se les otorga a los agentes luego de completar un task
- Ponderacion de los parametros que importan a los Jefes para elegir un contratista(reputacion entre ellos)
- Distribucion de la arquitectura(esto requiere una biyeccion a una gramatica o algo asi)

Algo relacionado con la cache
*/