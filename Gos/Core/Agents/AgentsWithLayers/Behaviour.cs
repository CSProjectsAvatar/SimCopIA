using System;
using System.Collections.Generic;

namespace ServersWithLayers{

    public class Behavior  {
        Dictionary<string, object> variables;
        Action<Status, Perception, Dictionary<string,object>> action;
        public Behavior() {
            variables = new Dictionary<string, object>();
        }

        public void SetVar(string name, object value) {
            variables[name] = value;
        }
      
    }
} 