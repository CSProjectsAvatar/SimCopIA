

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public abstract class NumOp:BinaryExpr
    {
        public NumOp(){}
        public NumOp(Expression left, Expression right):base(left, right){ }

        public abstract (bool, double) TryCompute(double left, double right);
    }
}
    