

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
        public FunCall(ILogger<FunCall> logger)
        {
            _log = logger;
        }

        public override bool Validate(Context context)
        {
            if (!context.CheckFunc(Identifier, Args.Count)) {
                _log?.LogError(
                    "Line {line}, column {col}: function '{id}' is not defined with {args} arguments.",
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