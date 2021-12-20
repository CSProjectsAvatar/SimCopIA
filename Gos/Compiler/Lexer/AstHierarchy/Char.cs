
using System;

namespace DataClassHierarchy
{
    public class Char:Expression
    {
        public char Value { get; set; }

        public override bool Validate(Context context)
        {
            return true;
        }
    }
}