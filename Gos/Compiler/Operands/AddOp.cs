

using System;

namespace DataClassHierarchy
{
    public class AddOp:NumOp
    {
        public AddOp(Expression left, Expression right, ILogger logger):base(left, right, logger){ }

        public override (bool, double) TryCompute(double left, double right){
            return (true, left + right);
        }
        public override double Eval(double left, double right){
            return left + right;
        }
    }
}
    