using System;
using System.Collections.Generic;

namespace ServersWithLayers{
    public class Directory{

        //  Resources -> ServersId
        Dictionary<string, List<string>> YellowPages;

        //  serverId -> ServerBIo
        Dictionary<string, ServerBio> WhitePages;

    }

    public class ServerBio{
        public string ID {get;}
        public int Reputation {get; private set;}
        public string Leader {get; private set;}

        public ServerBio(string ID){
            this.ID = ID;
            this.Reputation = 0;
            this.Leader = null;
        }
    }
}