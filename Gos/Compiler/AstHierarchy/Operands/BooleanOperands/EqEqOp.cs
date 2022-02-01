

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class EqEqOp:BinaryExpr
    {
        public EqEqOp(){}
        public EqEqOp(Expression left, Expression right):base(left, right){ }
    }
}
    