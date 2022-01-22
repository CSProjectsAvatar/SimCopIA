using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class InteractiveWorker : Agent
    {

        public Dictionary<string, List<string>> requestNeeds;
        public Dictionary<Request, List<Request>> originalRequest;
        public Dictionary<int, string> responseRequest;

        public InteractiveWorker(Environment env, string ID) : base(env, ID)
        {
            originalRequest = new Dictionary<Request, List<Request>> { };
            responseRequest = new Dictionary<int, string> { };
            requestNeeds = new Dictionary<string, List<string>> { };

            this.functionsToHandleRequests.Add(this.GettingRequestServer);
            this.functionsToHandleResponses.Add(this.GiveResponse);
        }


        private void GettingRequestServer(IRequestable status, Request r)
        {
            var this_interactive_server = status.agent as InteractiveWorker; 
            if (this_interactive_server.requestNeeds.ContainsKey(r.URL))
            {
				status.environment.PrintAgent(status.agent,"llega request de "+r.sender);

                this_interactive_server.originalRequest.Add(r, new List<Request> { });
                this_interactive_server.responseRequest.Add(r.ID, "");
                List<string> servers = this_interactive_server.requestNeeds[r.URL];
                foreach (var item in servers)
                {
					status.environment.PrintAgent(status.agent,"mandando request a "+item);

                    Request new_request = new Request(status.agent.ID, item, status.environment);
                    status.AddEvent(environment.currentTime, new_request);
                    this_interactive_server.originalRequest[r].Add(new_request);
                }
            }else {
                
            }
        }

        private void GiveResponse(IResponsable status, Response r)
        {
			status.environment.PrintAgent(status.agent,"llega response de "+r.sender.ID);
            bool mark = false;
            var this_interactive_server = status.agent as InteractiveWorker; 
            foreach (var item in this_interactive_server.originalRequest.Keys)
            {
                foreach (var req in this_interactive_server.originalRequest[item])
                {
                    if (req.ID == r.requestID)
                    {
                        mark = true;
                        this_interactive_server.responseRequest[item.ID] += r.body;
                        this_interactive_server.originalRequest[item].Remove(req);
                        if (this_interactive_server.originalRequest[item].Count == 0)
                        {
                            var new_response = new Response(item.ID, status.agent, item.sender, status.environment, this_interactive_server.responseRequest[item.ID]);
							new_response.SetTime(status.environment.currentTime);
                            status.AddEvent(status.environment.currentTime, new_response);
                            this_interactive_server.responseRequest.Remove(item.ID);
                            this_interactive_server.originalRequest.Remove(item);
                        }
                        break;
                    }
                }
                if (mark)
                    break;
            }
        }

        public void AddToRequirmentsDic (string URL, List<string> agents)
        {
            requestNeeds[URL] = agents;
            /* requestNeeds["/"] = new List<Agent> { environment.GetAgent("2"), environment.GetAgent("3"), environment.GetAgent("4") };
           // requestNeeds["google.com"] = new List<Agent> { environment.GetAgent("1") };
            requestNeeds["/other"] = new List<Agent> {environment.GetAgent("5"), environment.GetAgent("6") };
            requestNeeds["/about"] = new List<Agent> { environment.GetAgent("7") }; */
        }
    }

}
