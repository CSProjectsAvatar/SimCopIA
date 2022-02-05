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
                case ReqType.Asking when BehaviorsLib.IsAccepted(state, req):

                    Response askR = BehaviorsLib.BuildResponse(state, req);
                    state.Subscribe(askR);
                    break;

                case ReqType.Ping:

                    Response pingR = BehaviorsLib.BuildResponse(state, req);
                    state.Subscribe(pingR);
                    break;

                case ReqType.DoIt:

                    state.AcceptReq(req);
                    break;

                default:
                    break;
            }
        }
    }

}