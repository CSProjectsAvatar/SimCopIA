

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace DataClassHierarchy
{
    public class FunCall:Expression, IStatement
    {
        public string Identifier { get; set; }
        public List<Expression> Args { get; set; }

        private readonly ILogger<FunCall> _log;

        public FunCall(){ }
        public FunCall(string identifier, List<Expression> args, ILogger<FunCall> logger)
        {
            Identifier = identifier;
            Args = args;
            _log = logger;
        }

        public override bool Validate(Context context)
        {
            if (!context.CheckFunc(Identifier, Args.Count)) {
                _log?.LogError(
                    "Line {line}, column {col}: function '{id}' is already defined with {args} arguments.",
                    Token.Line,
                    Token.Column,
                    Identifier,
                    Args.Count);
                return false;
            }
            foreach (var expr in Args)
            {
                if (!expr.Validate(context))
                    return false;
            }
            return true;
        }
    }
}