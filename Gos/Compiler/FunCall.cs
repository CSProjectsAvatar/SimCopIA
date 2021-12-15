

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class FunCall:Statement
    {
        public string Identifier { get; set; }
        public List<Expression> Args { get; set; }

        public FunCall(){ }
        public FunCall(string identifier, List<Expression> args)
        {
            Identifier = identifier;
            Args = args;    
        }

        public override bool Validate(Context context)
        {
            foreach (var expr in Args)
            {
                if (!expr.Validate(context))
                    return false;
            }
            return context.CheckFunc(Identifier, Args.Count);
        }
    }
}