using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Status{
        List<(int, Perseption)> _sendToEnv;
        Dictionary<string, object> _variables;
        public Status(){
            _sendToEnv = new();
            _variables = new();
        }
        //Suscribe Perseptions en un tiempo 'time' en '_sendToEnv'.
        public void Subscribe(int time, Perseption p)
        {
            _sendToEnv.Add((time, p));
        }
        //Se llama cuando se recorrieron todas las capas, retorna un enumerable con todas las persepciones acumuladas de las capas y luego borra el historial de ellas.
        public IEnumerable<(int, Perseption)> EnumerateAndClear() {
            foreach(var x in _sendToEnv){
                yield return x;
            }
            _sendToEnv.Clear();
        }
    }
}


