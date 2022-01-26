
using Microsoft.Extensions.Logging;

namespace DataClassHierarchy
{
    ///<summary>
    /// Esta clase solo se utiliza cuando se hace un llamado a una variable ej. a = b + c
    ///</summary>
    public class Variable:Expression
    {
        private readonly ILogger<Variable> _log;

        public string Identifier { get; set; }

        public Variable(ILogger<Variable> logger = null) {
            _log = logger;
        }

        public override bool Validate(Context context)
        {
            if (!context.CheckVar(Identifier)) {
                _log?.LogError(
                    "Line {line}, column {col}: variable '{id}' not defined.",
                    Token.Line,
                    Token.Column,
                    Identifier);
                return false;
            }
            return true;
        }
    }
}