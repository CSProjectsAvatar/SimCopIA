using Compiler.Lexer.AstHierarchy;
using DataClassHierarchy;

namespace Compiler.Lexer.AstHierarchy {
    /// <summary>
    /// El nodo correspondiente a la operación ?.
    /// </summary>
    internal class ZeroOnceAst : BaseRegexAst {
        public ZeroOnceAst(BaseRegexAst target) {
            Target = target;
        }

        public BaseRegexAst Target { get; }

        public override bool Validate(Context context) {
            return Target.Validate(context);
        }
    }
}