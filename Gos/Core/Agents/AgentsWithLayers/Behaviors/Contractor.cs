using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{
    public static class ContractorBehav
    {
        internal static void Behavior(Status state, Perception r, Dictionary<string, object> vars)
        {
            if (r is not Request)
                return;
            Request req = r as Request;

            switch (req.Type)
            {
                case ReqType.Asking when IsAccepted(state, req):

                    Response response = BehaviorsLib.BuildResponse(state, req);
                    state.Subscribe(response);
                    break;

                case ReqType.DoIt or ReqType.Ping:

                    state.AcceptReq(req);
                    break;

                default:
                    break;
            }
        }
        // Accepts a request under certain conditions
        private static bool IsAccepted(Status st, Request req)
        {
            return st.HasCapacity;
        }
    }

}