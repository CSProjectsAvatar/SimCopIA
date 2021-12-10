

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class FunCall:Expression
    {
        public string Identifier { get; set; }
        public List<Expression> Args { get; set; }

        public override int Eval()
        {
            throw new System.NotImplementedException();
        }

        public override bool Validate(Context context)
        {
            throw new System.NotImplementedException();
        }
    }
}