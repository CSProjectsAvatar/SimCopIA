using System.Collections.Generic;
using System.Linq;
using System;


namespace ServersWithLayers{

    public class Output{
        public Env env {get;}

        public IEnumerable<(int,Request)> Requests {get; private set;}
        public IEnumerable<(int,Response)> Responses {get; private set;}

        //tiempo de response promedio
        public double Average {get; private set;}
        // % request sin respuesta
        public double NotResponsePercent {get; private set;}
        // % request respondidos
        public double ResponsePercent {get; private set;}

        



        public Output(Env env){
            this.env = env;
        }
        
        public void ProcessData(){
            Requests  = env.GetClientRequests();
            Responses = env.GetClientResponses();

            var difs =  (from req in Requests
                        from res in Responses
                        where res.Item2.ReqID == req.Item2.ID
                        select res.Item1 -req.Item1);



            if(difs.Count()!=0)
                Average = difs.Average();


            ResponsePercent = (100 * Responses.Count()) / Requests.Count()  ;

            NotResponsePercent = 100 - ResponsePercent;

        }
    }
}