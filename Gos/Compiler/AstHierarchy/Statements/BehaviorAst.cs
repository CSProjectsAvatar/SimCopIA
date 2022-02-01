using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.AstHierarchy.Statements {
    public class BehaviorAst : AstNode, IStatement {
        private readonly ILogger<BehaviorAst> _log;

        public BehaviorAst(ILogger<BehaviorAst> logger) {
            _log = logger;
        }

        public string Name { get; init; }
        public IEnumerable<IStatement> Code { get; init; }

        public override bool Validate(Context context) {
            if (!context.DefBehav(Name, null)) {
                _log.LogError(
                    Helper.LogPref + "behavior already defined.",
                    Token.Line,
                    Token.Column);

                return false;
            }
            var inits = Code
                .OfType<InitAst>()
                .ToList();
                
            if (inits.Count > 1) {
                _log.LogError(
                    Helper.LogPref + "at most one init block must be defined.",
                    inits[1].Token.Line,
                    inits[1].Token.Column);
                return false;
            }
            if (inits.Count != 0 && Code.First() is not InitAst) {  // si hay un bloke init, tiene q ser lo 1ro
                _log.LogError(
                    Helper.LogPref + "the init block must be the first statement in behavior.",
                    inits[0].Token.Line,
                    inits[0].Token.Column);
                return false;
            }
            var child = context.CreateChildContext();
            child.DefVariable(Helper.StatusVar);
            child.OpenBehavior = true;

            var mainCode = Code;
            var ans = true;

            if (inits.Count != 0) {  // hay un bloke init
                ans = inits[0].Validate(child);  // valida'ndolo apart xq e'l no puede acceder a percep
                mainCode = mainCode.Skip(1);
            }
            child.DefVariable(Helper.PercepVar);
            child.DefVariable(Helper.DoneReqsVar);

            ans = ans && mainCode.All(st => st.Validate(child));

            child.OpenBehavior = false;

            return ans;
        }
    }
}
