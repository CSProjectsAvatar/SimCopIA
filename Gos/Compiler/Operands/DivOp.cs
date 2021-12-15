

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class DivOp:NumOp
    {
        ILogger<DivOp> log;
        public DivOp(Expression left, Expression right, ILogger<DivOp> logger):base(left, right){
            this.log = logger;
        }

        public override (bool, double) TryCompute(double left, double right){
            if(right == 0){
                log.LogError("Can't divide by 0");
                return (false, 0);
            }
        
            return (true, left / right);
        }
    }
}
    