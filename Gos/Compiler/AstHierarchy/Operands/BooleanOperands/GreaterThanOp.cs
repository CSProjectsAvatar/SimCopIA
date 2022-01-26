

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class GreaterThanOp:BinaryExpr
    {
        public GreaterThanOp(){}
        public GreaterThanOp(Expression left, Expression right):base(left, right){ }

        public override (bool, object) TryCompute(object left, object right){
            if(left is double l && right is double r){
                return (true, l > r);
            }
            return (false, null);
        }
    }
}
    