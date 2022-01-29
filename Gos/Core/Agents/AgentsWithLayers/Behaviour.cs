using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public class Behavior
    {
        Dictionary<string, object> variables;
        Action<Status, Perception, Dictionary<string, object>> action;
        public Behavior()
        {
            variables = new Dictionary<string, object>();
        }

        public void SetVar(string name, object value)
        {
            variables[name] = value;
        }

        public void Run(Status status, Perception perception) { }

        public Behavior Clone()
        {
            var copy = new Behavior();  //@todo review dict var
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