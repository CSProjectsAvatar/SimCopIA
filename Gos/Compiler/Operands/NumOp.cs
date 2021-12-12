

using System;

namespace DataClassHierarchy
{
    public abstract class NumOp:BinaryExpr
    {
        public NumOp(Expression left, Expression right):base(left, right){ }

        public override (bool, object) TryEval(){
            var (lSuccess, lResult) = Left.TryEval();
            var (rSuccess, rResult) = Right.TryEval();

            var tSuccess = (lSuccess, rSuccess);

            switch(tSuccess){
                case (true, true) when lResult is double lNum && rResult is double rNum:
                    return (true, Eval(lNum, rNum));

                default: throw new Exception("Oper: Invalid operands"); //@audit Ver las excepciones de nosotros
            }
        }
        public abstract double Eval(double left, double right);
    }
}
    