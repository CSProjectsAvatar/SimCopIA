using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServersWithLayers.Behaviors;

namespace ServersWithLayers
{

    public static class FallenLeaderBehav
    {

        private static string countPingStr = "countPing";
        private static string maxPingStr = "maxPing";
        private static string initialPotenceStr = "initialPotence";
        private static string lastTSeeLeaderStr = "lastTimeSeeLeader";


        internal static void BehavInit(Status state, Dictionary<string, object> vars)
        {
            vars[initialPotenceStr] = 3;
            vars[maxPingStr] = 3;
            vars[countPingStr] = 0;

            vars[lastTSeeLeaderStr] = 0;
        }

        public static void Behavior(Status st, Perception perce, Dictionary<string, object> vars)
        {
            int initP = (int)vars[initialPotenceStr];
            int countPing = (int)vars[countPingStr];
            int maxPing = (int)vars[maxPingStr];
            int lastTSeeLdr = (int)vars[lastTSeeLeaderStr];

            if (perce is Message msg && msg.Sender == st.MicroService.LeaderId) {// Envio del Lider
                vars[countPingStr] = 0;
                vars[lastTSeeLeaderStr] = Env.Time;
                return;
            }

            var waitTime = (int)Math.Pow(2, initP + countPing);
            if (Env.Time - lastTSeeLdr >= waitTime)
            {
                if (countPing >= maxPing) // Mucho Tiempo sin saber del lider
                {
                    vars[countPingStr] = 0;
                    st.MicroService.ChangeLeader(st.serverID); //me pongo de lider
                    // Envio PING a todos mis CoWorkers
                    foreach (var item in BehaviorsLib.CreatePingRequests(st)) {
                        st.Subscribe(item);
                    }
                }
                else
                {   // Construyo PING Request
                    Request pingRequest = new Request(st.serverID, st.MicroService.LeaderId, ReqType.Ping);
                    int time = new Random().Next(waitTime/2);

                    st.SubscribeIn(time, pingRequest); // Envio PING
                    st.SubscribeIn(waitTime, new Observer(st.serverID)); // Pongo Alarma
                    vars[countPingStr] = countPing + 1;

                    st.SubscribeIn(waitTime, new Observer(st.serverID));  // pa repetir el PING si el li'der sigue cai'2
                }
            }

        }

    }

}