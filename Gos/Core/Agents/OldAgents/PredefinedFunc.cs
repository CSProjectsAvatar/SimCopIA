namespace Agents
{
    static class PredefinedFunctions{
        public static class Workers{
        #region Worker Functions
            public static void WorkerGettingRequest(IRequestable status, Request r){
                if(!status.IsAvailable){ 
                    
                    var res = new Response(r.ID,status.agent,r.sender,status.environment,"Servidor no disponible");
                    res.SetTime(status.environment.currentTime);
                    status.AddEvent(status.environment.currentTime, res );
                    status.environment.PrintAgent(status.agent,"Llega requst pero no esta disponible.");
                    return;
                }
                
                status.environment.PrintAgent(status.agent,$"LLega request desde {r.sender}.");
    
                            
                Observer o = new Observer(status.agent,status.environment, r);
    
                int time = status.environment.currentTime + Worker.TimeOffset();
                status.AddEvent(time,o);
            }
            public static void WorkerProcessRequest(IRequestable status, Request r)
            {
                if (status.IsAvailable)
                {
                    status.AddRequestToProcessed(r);
                    if (status.GetListRequest().Count >= (status.agent as Worker).TotalRequests)
                        status.SetAvailibility(false);
                }
            }
            public static void WorkerSendResponse(IObservable status,Observer o){
                Request originalRequest = (o.Object as Request);
                Response response = new(originalRequest.ID,status.agent,originalRequest.sender,status.environment,$"Cosas de servidor simple {status.agent.ID}.");
                response.SetTime(status.environment.currentTime);
                status.AddEvent(status.environment.currentTime,response);
            }
            public static void WorkerSetAvailableAfterSendResponse(IObservable status, Observer o){
                Request originalRequest = (o.Object as Request);
                status.DeleteRequest(originalRequest);            
                status.SetAvailibility(true);
            }
        #endregion
        }
    }
}