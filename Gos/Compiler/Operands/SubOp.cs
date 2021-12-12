

using System;

namespace DataClassHierarchy
{
    public class SubOp:NumOp
    {
        public SubOp(Expression left, Expression right):base(left, right){ }

        public override double Eval(double left, double right){
            return left - right;
        }
    }
}
    