using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Grammar_Unterminals {
    class StatListUnt : Unterminal {
        public IEnumerable<IStatement> Statements { get; private set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            /*
             <stat-list> := <stat> ";"
                          | <stat> ";" <stat-list>
        	              | <block-stat>
                          | <block-stat> <stat-list>
             */
            Statements = new[] { 
                (derivation[0] is StatementUnt stat
                    ? stat.Ast
                    : ((BlokStUnt)derivation[0]).Ast)
                as IStatement
            };
            if (derivation.Count == 3 || derivation.Count == 2 && derivation[1] is StatListUnt) {
                var statsTail = ((StatListUnt)derivation[derivation.Count - 1]).Statements;
                Statements = Statements.Concat(statsTail);
            }
            return null;
        }
    }
}
