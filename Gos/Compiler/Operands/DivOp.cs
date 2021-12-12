

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class DivOp:NumOp
    {
        public DivOp(Expression left, Expression right, ILogger logger):base(left, right, logger){ }

        public override (bool, double) TryCompute(double left, double right){
            if(right == 0){
                log.LogError("Can't divide by 0");
                return (false, 0);
            }
        
            return (true, left / right);
        }
        public override double Eval(double left, double right){ 
            return left / right; // @audit check for divide by zero, Exception? False?
        }
    }
}
    