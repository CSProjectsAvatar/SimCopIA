

using System;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class EqEqOp:BinaryExpr
    {
        public EqEqOp(){}
        public EqEqOp(Expression left, Expression right):base(left, right){ }

        public override (bool, object) TryCompute(object left, object right){  // @audit ESTOS TryCompute LE KITAN EL SENTI2 A LOS VISITORS
            if(left is double l && right is double r){  // @audit ESTOS CHEKEOS SE ESTA'N HACIEN2 DOBLE, TANTO AKI' COMO EN EL VISITOR. CREA UN NUEVO VISITOR Q C ENKRGUE D DEVOLVER TODOS LOS POSIBLES PARES D TIPOS Q C ADMITEN
                return (true, l == r);
            }
            return (false, null);
        }
    }
}
    