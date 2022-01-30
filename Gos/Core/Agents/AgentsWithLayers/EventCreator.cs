using System;
using System.Collections.Generic;
using System.Linq;

namespace ServersWithLayers{
    public abstract class EventCreator{

        List<Type> _eventTypes;
        public List<Event> GetEvents(int quantity, int timeInit=1, int timeFinal=100){
            List<Event> events = new();
            var r = new Random();
            while(quantity --> 0){
                var t = r.Next(timeInit, timeFinal);
                var eType = SelectType();
                var newEvent = Build(eType, t);
                events.Add(newEvent);
            }
            return events;
        }

        private Event Build(Type eType, int t)
        {
            // eType switch {
            //     typeof(Request) => ...
            // }
            throw new NotImplementedException();
        }

        private Type SelectType()
        {
            return _eventTypes.First();
        }
    }

}

/*
cuando empiezo el worker:

veo que tareas complete'
agendo un response para cada una
libero el recurso ocupado

mientras tenga capacidad y hayan tareas:
    cojo una tarea de las aceptadas pendientes
    me pongo a hacerla
*/