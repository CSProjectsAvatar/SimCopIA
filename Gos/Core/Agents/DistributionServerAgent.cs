using System;
using System.Collections.Generic;

namespace Agents
{
    //to do
    public class DistributionServer:Agent{
        public List<string> workers {get;}
        public Func<List<string>, int> selectionProtocol{get;}
        public DistributionServer( Environment env, string ID, List<Agent> workers):base(env,ID){

            throw new NotImplementedException("Falta por implementar DistributionServer");
           /*  this.workers = new();
            selectionProtocol = delegate (List<string> workers){
                Random r = new();
                return r.Next(workers.Count); 
            }; */
        }
        
    }
}