

using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class DefFun:Statement
    {
        public string Identifier { get; set; }
        public List<string> Arguments { get; set; }
        public Expression Body { get; set; }

        public override bool Validate(Context context)
        {
            var innerContext = context.CreateChildContext();

            foreach (var arg in Arguments) {
                innerContext.DefVariable(arg);
            }
            if(!Body.Validate(innerContext)) {
                return false;
            }

            return context.DefFunc(Identifier, Arguments.Count);
        }
    }
}