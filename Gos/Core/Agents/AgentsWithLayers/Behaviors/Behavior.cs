using System;
using System.Collections.Generic;

namespace ServersWithLayers
{

    public class Behavior : ICloneable
    {
        Dictionary<string, object> variables;
        private Action<Status, Dictionary<string, object>> _init;
        Action<Status, Perception, Dictionary<string, object>> action;
        
        bool _firstTime = true;

        public string Name { get; init; }

        public Behavior():this(null) { }
        public Behavior(Action<Status, Perception, Dictionary<string, object>> action, Action<Status, Dictionary<string, object>> init = null)
        {
            this.action = action;
            this.variables = new Dictionary<string, object>();
            _init = init;
        }

        public Behavior(string name, Action<Status, Perception, Dictionary<string, object>> action, Action<Status, Dictionary<string, object>> init = null) : this(action,init)
        {
            Name = name;
        }

        public void SetVar(string name, object value)
        {
            variables[name] = value;
        }

        public void Run(Status status, Perception perception) {
            if(_firstTime){
                _firstTime = false;
                _init?.Invoke(status, variables);
            }
            action(status, perception, variables);
         }

        public Object Clone()
        {
            var copy = new Behavior();
            copy.action += this.action;
            copy._init += this._init;
            return copy;
        }
        /// Retorna el object asociado a la variable `name`.
        public object GetVariables(string name){
            if(!variables.ContainsKey(name))
                return null;
            return variables[name];
        } 

    }
} 
/*
Donde IA:
- Eleccion cantidad de reputacion que se les otorga a los agentes luego de completar un task 
- Ponderacion de los parametros que importan a los Jefes para elegir un contratista(reputacion entre ellos)
- Distribucion de la arquitectura(esto requiere una biyeccion a una gramatica o algo asi)
- Como repartir los recursos
- Seleccionar que comportamiento se ejecuta en la capa
- Algo relacionado con la cache


Priorizar:
- Layer Decisor Function
- Usar el DSL

Cosas a hacer
- Implementar comunicacion entre microservicios

Task de IA

Task1 (C)
Delegado decisor del Behavior
Hacer un delegado en layer que decida que behavior usar, por defecto el 1ro

Task2 (O)
Sumar reputacion a los servers que den una respuesta aceptable al Boss(en el BossBehav)
- aceptable es que me respondiste en el tiempo que yo estuve esperando
- se le sumara a la reputacion una cantidad en funcion de el orden en que proveyo la respuesta
- hay una listica ahi de %s bajando de 20 -> 0, con -10% en cada elemento

Task3 (C)
Hacer funcion de reseteo ResetServersReputation
Hacer una funcion en MicroService que llame a Reset en la reputacion de todos los servers en el microservicio
Reset es una funcion de int a int(un lambda x ej) que puede ser:
x => 1
o
x => max (int)log x y 1

Task4 (M)
Hacer Funcion que prioriza entre Servers
Dado una lista de Servidores Devuelve una lista ordenada "Mejor" para pedirle una tarea, dado el valor que devuelva la funcion Credibility: reputacion*parallelProcesors

Credibility: reputacion * parallelProcessors

Task5 (M)
Dado una lista ordenada de Servers o servers name por prioridad y una lista de pares 
<recurso, list<servidores>) devolver los request a mandar, donde un recurso solo aparezca en un request, y a un server se le mande a lo sumo un request.


Cosas que se pueden Buscar con metaheuristica despues:
- Cuanta reputacion dar

Task6 (O)
Usar una metaheuristica semejante a la del Jefe, en Las capas  de los Servers:
Funcion que recibe una lista de Behaviors y devuelve un indice
una variable por cada behavior, entre todas suman 1
Se hace una variacion de la metaheuristica de exploracion y explotacion
rotando tiempo que le doy a un behavior y aumentandolo
la funcion de fitness puede ser la cantidad de reputacion ganada por request recibido

METER CSP PARA DECIDIR REPARTICION DE RECURSOS
*/
