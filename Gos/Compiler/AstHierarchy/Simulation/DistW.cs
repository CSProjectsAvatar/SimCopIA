

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class DistW:Expression
    {
        // @audit Nota la lista que se le pasa por argumento al Distw de simulacion posiblemente no pueda ser null, sino vacia
        
        public override bool Validate(Context context)
        {
            return true;
        }
    }
}