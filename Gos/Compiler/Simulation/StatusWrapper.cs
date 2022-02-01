using ServersWithLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Simulation {
    class StatusWrapper : IServerStatus {
        private Status _status;

        public StatusWrapper(Status status) {
            _status = status;
        }

        public List<object> AcceptedReqs => _status.AceptedRequests
            .OfType<object>()
            .ToList();

        public bool CanProcess => _status.HasCapacity;
    }
}
