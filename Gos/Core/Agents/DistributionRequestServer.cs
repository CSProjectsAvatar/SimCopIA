using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    class DistributionRequestServer : Agent
    {
        public Dictionary<string, List<Agent>> requestNeeds;

        public DistributionRequestServer(Environment env, string ID) : base(env, ID)
        {
        }

        private void GettingRequestServer( Request r)
        {
            if (requestNeeds.ContainsKey(r.URL))
            {
                List<Agent> servers = requestNeeds[r.URL];
                foreach (var item in servers)
                {
                    item.HandleRequest(new Request(this.ID, item.ID, environment));
                }
            }

        }
    }
  
}
