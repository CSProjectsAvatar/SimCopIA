using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class ForEachAst : AstNode, IStatement {
        private ILogger<ForEachAst> _log;

        public ForEachAst(ILogger<ForEachAst> logger) {
            _log = logger;
        }

        public string Item { get; private set; }
        public string Index { get; private set; }
        public Expression Iterable { get; init; }
        public IEnumerable<IStatement> Code { get; init; }
        public IReadOnlyList<string> Variables { get; init; }

        public override bool Validate(Context context) {
            if (Variables.Count > 2) {
                _log.LogError(
                    Helper.LogPref + "at most two variable names must be provided.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            Item = Variables[^1];
            if (Variables.Count == 2) {
                Index = Variables[0];
            }
            context.OpenLoop = true;

            var child = context.CreateChildContext();
            child.DefVariable(Item);
            if (Index != default) {
                child.DefVariable(Index);
            }
            var ans = Code.All(st => st.Validate(child));

            context.OpenLoop = false;

            return ans;
        }
    }
}
