

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class DefFun: AstNode, IStatement
    {
        public string Identifier { get; set; }
        public List<string> Arguments { get; set; }
        public List<IStatement> Body { get; set; }
        public ILogger Log { get; }

        public DefFun(){}
        public DefFun(ILogger logger){
            this.Log = logger;
        }
        public override bool Validate(Context context)
        {
            if(!context.DefFunc(Identifier, Arguments.Count)){
                Log.LogError("Function '" + Identifier + "' is already defined.");
                return false;
            }

            var innerContext = context.CreateChildContext();

            foreach (var arg in Arguments) {
                innerContext.DefVariable(arg);
            }
            foreach (var st in Body) {
                if(!st.Validate(innerContext))
                    return false;
            }

            return true;
        }
    }
}