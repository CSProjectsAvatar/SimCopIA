using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class DistributionRequestServer : Agent
    {

        public Dictionary<string, List<Agent>> requestNeeds;
        public Dictionary<Request, List<Request>> originalRequest;
        public Dictionary<int, string> responseRequest;

        public DistributionRequestServer(Environment env, string ID) : base(env, ID)
        {
            originalRequest = new Dictionary<Request, List<Request>> { };
            responseRequest = new Dictionary<int, string> { };
            requestNeeds = new Dictionary<string, List<Agent>> { };
            AñadirAlDiccionario();//esto es para probar
            this.functionsToHandleRequests.Add(this.GettingRequestServer);
            this.functionsToHandleResponses.Add(this.GiveResponse);
        }


        private void GettingRequestServer(IRequestable status, Request r)
        {

            if (requestNeeds.ContainsKey(r.URL))
            {
				this.environment.PrintAgent(this,"llega request de "+r.sender);

                originalRequest.Add(r, new List<Request> { });
                responseRequest.Add(r.ID, "");
                List<Agent> servers = requestNeeds[r.URL];
                foreach (var item in servers)
                {
					this.environment.PrintAgent(this,"mandando request a "+item.ID);

                    Request new_request = new Request(this.ID, item.ID, environment);
                    status.AddEvent(environment.currentTime, new_request);
                    originalRequest[r].Add(new_request);
                }
            }

        }

        private void GiveResponse(IResponsable status, Response r)
        {
			this.environment.PrintAgent(this,"llega response de "+r.sender.ID);
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
                            var new_response = new Response(item.ID, this, item.sender, environment, responseRequest[item.ID]);
							new_response.SetTime(this.environment.currentTime);
                            status.AddEvent(environment.currentTime, new_response);
                            responseRequest.Remove(item.ID);
                            originalRequest.Remove(item);
                        }
                        break;
                    }
                }
                if (mark)
                    break;
            }
        }

        private void AñadirAlDiccionario ()
        {
            requestNeeds["youtube.com"] = new List<Agent> { environment.GetAgent("2"), environment.GetAgent("3"), environment.GetAgent("4") };
           // requestNeeds["google.com"] = new List<Agent> { environment.GetAgent("1") };
            requestNeeds["amazon.com"] = new List<Agent> {environment.GetAgent("5"), environment.GetAgent("6") };
            requestNeeds["facebook.com"] = new List<Agent> { environment.GetAgent("7") };




        }
    }

}
