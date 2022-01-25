

using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    public class DefFun: AstNode, IStatement
    {
        public string Identifier { get; set; }
        public List<string> Arguments { get; set; }
        public List<IStatement> Body { get; set; }
        private ILogger<DefFun> _log;

        public DefFun(){}
        public DefFun(ILogger<DefFun> logger){
            _log = logger;
        }
        public override bool Validate(Context context)
        {
            if(!context.DefFunc(Identifier, Arguments.Count)){
                _log.LogError(
                    "Line {line}, column {col}: function '{id}' is already defined.",
                    Token.Line,
                    Token.Column,
                    Identifier);
                return false;
            }
            var innerContext = context.CreateChildContext();

            foreach (var arg in Arguments) {
                innerContext.DefVariable(arg);
            }
            innerContext.OpenFunction = true;

            foreach (var st in Body) {
                if(!st.Validate(innerContext))
                    return false;
            }
            innerContext.OpenFunction = false;

            return true;
        }
    }
}