

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public abstract class NumOp:BinaryExpr
    {
        protected ILogger log;
        public NumOp(Expression left, Expression right, ILogger logger):base(left, right){ 
            log = logger;
        }

        public override (bool, object) TryEval(){
            var (lSuccess, lResult) = Left.TryEval();
            if(!lSuccess){
                log.LogError("Left operand could not be Evalued");
                return (false, null);
            }

            var (rSuccess, rResult) = Right.TryEval();
            if(!rSuccess){
                log.LogError("Right operand could not be Evalued");
                return (false, null);
            }

            var tResults = (lResult, rResult);

            switch (tResults)
            {
                case (double lNum, double rNum):
                    var (succ, result) = TryCompute(lNum, rNum);
                    if(!succ){
                        log.LogError("Could not compute {lNum} {this} {rNum}", lNum, this, rNum);
                        return (false, null);
                    }
                    return (true, result);
                
                default:
                    log.LogError($"Could not compute {lResult} {this} {rResult}");
                    return (false, null);
            }
        }
        public abstract (bool, double) TryCompute(double left, double right);
    }
}
    