using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public class GoSException : Exception {
        public GoSException(string message) : base(message) {
        }
      
    }
}
