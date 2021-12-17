

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    /// <summary>
    /// Nodo de AST para la resta.
    /// </summary>
    public class SubOp:NumOp
    {
        public SubOp(){}
        public SubOp(Expression left, Expression right):base(left, right){ }

        public override (bool, object) TryCompute(object left, object right){
            if(left is double l && right is double r){
                return (true, l - r);
            }
            return (false, null);
        }
    }
}
    