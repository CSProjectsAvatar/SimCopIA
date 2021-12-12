

using System;

namespace DataClassHierarchy
{
    public class MultOp:NumOp
    {
        public MultOp(Expression left, Expression right):base(left, right){ }

        public override double Eval(double left, double right){
            return left * right;
        }
    }
}
    