

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class Worker:Expression
    {
        public override bool Validate(Context context)
        {
            return true;
        }
    }
}