using Compiler.AstHierarchy;
using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class LeftValUnt : Unterminal {
        public IEnumerable<Expression> Idxs { get; private set; }
        public string Id { get; private set; }
        public bool IsListIdx => Idxs.Any();

        public uint Line { get; private set; }
        public uint Column { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <left-val> := ID
                         | <left-val> "[" <math> "]"
             */
            if (derivation[0] is Token tk) {
                Id = tk.Lexem;
                Line = tk.Line;
                Column = tk.Column;
                Idxs = Enumerable.Empty<Expression>();
            } else if (derivation[0] is LeftValUnt lv) {
                Id = lv.Id;
                Line = lv.Line;
                Column = lv.Column;
                Idxs = lv.Idxs.Append((derivation[2] as MathUnt).Ast as Expression);
            }
            return null;
        }
    }
}
