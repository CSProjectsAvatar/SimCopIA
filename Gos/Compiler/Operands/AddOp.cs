

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class AddOp:NumOp
    {
        public AddOp(Expression left, Expression right):base(left, right){ }

        public override (bool, double) TryCompute(double left, double right){
            return (true, left + right);
        }
    }
}
    