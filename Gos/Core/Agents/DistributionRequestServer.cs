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
        public Dictionary<Request, List<Request>> originalRequest;
        public Dictionary<int, string> responseRequest;

        public DistributionRequestServer(Environment env, string ID) : base(env, ID)
        {
            originalRequest = new Dictionary<Request, List<Request>> { };
            responseRequest = new Dictionary<int, string> { };
           
        }

       
        private void GettingRequestServer( Request r)
        {
            originalRequest.Add(r, new List<Request> { });
            responseRequest.Add(r.ID, "");
            if (requestNeeds.ContainsKey(r.URL))
            {
                List<Agent> servers = requestNeeds[r.URL];
                foreach (var item in servers)
                {
                    Request new_request = new Request(this.ID, item.ID, environment);
                    environment.AddRequest(new_request,10);//aqui que time le pongo
                    originalRequest[r].Add(new_request);
                }
            }

        }

        private void GiveResponse(Response r)
        {
            bool mark = false;
            foreach (var item in originalRequest.Keys)
            {
                foreach (var req in originalRequest[item])
                {
                    if (req.ID == r.requestID)
                    {
                        mark = true;
                        responseRequest[item.ID] += r.body;
                        originalRequest[item].Remove(req);
                        if (originalRequest[item].Count == 0)
                        {
                            var response = new Response(item.ID, this, item.sender, environment, responseRequest[item.ID]);
                            environment.SubscribeResponse(response);
                            responseRequest.Remove(item.ID);
                            originalRequest.Remove(item);
                            //para que es AddSolutionResponse
                        }
                        break;
                    }
                }
                if (mark)
                    break;
            }
        }
    }
  
}
