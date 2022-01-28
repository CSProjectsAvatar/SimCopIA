using System;
using System.Collections.Generic;

namespace ServersWithLayers
{
    public class BehaviourLibrary
    {

        Directory directory;
        
        public BehaviourLibrary()
        {
            
        }

        private void ArrivesRequest(Status status, Request r)
        {
            Dictionary<string, object> data;
            if (r.Type == RequestType.AskSomething)
            {
                List<string> IDservers = directory.YellowPages[r.URL];
                
                foreach (var item in IDservers)
                {
                    if (item == r.Receiber)
                    {
                        bool acepted = true; //Aceptation(item); //por ahora pongamoslo en true
                        data = new Dictionary<string, object>() { { r.URL, acepted } };
                        r.env.SubsribeEvent(0, r.MakeResponse(data)); // que time pongo
                        return;
                    }
                }
                data = new Dictionary<string, object>() { { r.URL, false } };
                r.env.SubsribeEvent(0, r.MakeResponse(data)); // que time pongo
            }
            else if(r.Type == RequestType.DoSomething)
            {
                // aqui lo mando a ejecutar 
                data = new Dictionary<string, object>() { { r.URL, true } };
                r.env.SubsribeEvent(0, r.MakeResponse(data)); // que time pongo
                return;
            }
            else
            {

            }

        }

        private bool Aceptation(string item)
        {
            throw new NotImplementedException();
        }
    }
}
