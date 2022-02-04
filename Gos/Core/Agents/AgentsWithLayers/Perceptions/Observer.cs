using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServersWithLayers
{
    // Un Observer esta encargado de informarle a un servidor que debe manejar un su estado interno, 
    // es util cuando se usan cronomtros etc,
    // contiene el objeto Objective, que es un objeto que identifica lo que va a suceder dentro del server que suscribio el Observer. 
    public class Observer:Perception{
        public Observer(string sender) : base(sender){ }
    }
}