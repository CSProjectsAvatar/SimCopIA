

using System;

namespace DataClassHierarchy
{
    public class DivOp:NumOp
    {
        public DivOp(Expression left, Expression right):base(left, right){ }

        public override double Eval(double left, double right){ 
            return left / right; // @audit check for divide by zero, Exception? False?
        }
    }
}
    