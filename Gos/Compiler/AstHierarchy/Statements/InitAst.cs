using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class InitAst : AstNode, IStatement {
        private readonly ILogger<InitAst> _log;

        public InitAst(ILogger<InitAst> logger) {
            _log = logger;
        }

        public IEnumerable<IStatement> Code { get; init; }

        public override bool Validate(Context context) {
            if (!context.OpenBehavior) {
                _log.LogError(
                    Helper.LogPref + "init block must be directly inside a behavior block.",
                    Token.Line,
                    Token.Column);
                return false;
            }
            foreach (var st in Code) {
                if (st is not VarAssign va) {
                    _log.LogError(
                        Helper.LogPref + "it must be a variable assignment (e.g. a = 5).",
                        (st as AstNode).Token.Line,
                        (st as AstNode).Token.Column);
                    return false;
                }
                if (!context.DefVariable(va.Variable)) {  // se define la variable en el contexto padre pa q pueda ser usada en to2 el comportamiento
                    _log.LogError(
                        Helper.LogPref + "variable '{id}' already defined.",
                        (st as AstNode).Token.Line,
                        (st as AstNode).Token.Column,
                        va.Variable);
                    return false;
                }
                if (!st.Validate(context)) {
                    return false;
                }
            }
            return true;
        }
    }
}
