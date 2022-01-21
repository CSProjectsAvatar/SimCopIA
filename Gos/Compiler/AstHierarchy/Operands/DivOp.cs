

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
        public override (bool, object) TryCompute(object left, object right){
            if(left is double l && right is double r){
                if(r == 0){
                    log.LogError("Can't divide by 0");
                    return (false, null);
                }
                return (true, l / r);
            }
            return (false, null);
        }
    }
}
    