

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class MultOp:NumOp
    {
        public MultOp(Expression left, Expression right, ILogger logger):base(left, right, logger){ }


           public override (bool, double) TryCompute(double left, double right){
            return (true, left * right);
        }
    }
}
    