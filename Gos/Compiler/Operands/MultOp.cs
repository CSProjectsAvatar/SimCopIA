

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class MultOp:NumOp
    {
        public MultOp(){ }
        public MultOp(Expression left, Expression right):base(left, right){ }

        public override (bool, double) TryCompute(double left, double right){
            return (true, left * right);
        }
    }
}
    