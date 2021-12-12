

using System;

namespace DataClassHierarchy
{
    public class AddOp:NumOp
    {
        public AddOp(Expression left, Expression right):base(left, right){ }

        public override double Eval(double left, double right){
            return left + right;
        }
    }
}
    