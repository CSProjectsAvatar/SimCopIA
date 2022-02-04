using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Directory{

        //  Resources -> ServersId
        internal Dictionary<string, List<string>> YellowPages;

        //  serverId -> ServerBIo
        internal Dictionary<string, ServerBio> WhitePages;

        public Directory(){
            YellowPages = new();
            WhitePages = new();
        }
        internal void AddServer(Server server)
        {
            if(WhitePages.ContainsKey(server.ID))
                throw new Exception($"Server {server.ID} already added to microservice");

            WhitePages.Add(server.ID, new ServerBio(server));
               
            // Add to Yellow pages as keys the resources that the server provides and add his id to the list of servers that provide that resource
            foreach (var resource in server.Stats.AvailableResources)
            {
                var resName = resource.Name;
                
                if (!YellowPages.ContainsKey(resName))
                    YellowPages.Add(resName, new List<string>());
                // lo puedo añadir directamente, no hace falta comprobar que no esté ya
                YellowPages[resName].Add(server.ID); 
            }
           
        }
    }

    public class ServerBio{
        public string ID {get;}
        public int Reputation {get; private set;}
        public int ParallelProcessors { get; }
        public string Leader {get; private set;}

        public ServerBio(Server server){
            this.ID = server.ID;
            this.Reputation = 0;
            this.Leader = null;
            this.ParallelProcessors = server.Stats.MaxCapacity;
        }


    }
}