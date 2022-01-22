using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class ClassUnt : Unterminal {
        internal ClassEnum Class { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <class> := "simplew"
            //          | "distw"
            var t = derivation[0] as Token;
            Class = t.Type switch {
                Token.TypeEnum.SimpleWorker => ClassEnum.Simplew,
                Token.TypeEnum.DistWorker => ClassEnum.Distw,
                _ => throw new ArgumentException("Invalid token.", nameof(derivation))
            };
            return null;
        }

        internal enum ClassEnum {
            Simplew,
            Distw
        }
    }
}
