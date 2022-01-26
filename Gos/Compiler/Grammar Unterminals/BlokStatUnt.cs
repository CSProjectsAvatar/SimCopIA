using System.Collections.Generic;
using System.Linq;
using Compiler.Grammar_Unterminals;
using DataClassHierarchy;

namespace Compiler {
    internal class BlockStUnt : Unterminal {
        public IEnumerable<string> Args { get; set; }

        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation) {
            // <block-stat> := <if> 
            //               | <def-func>
            //               | <inf-loop>
            //               | <foreach>
            return derivation[0] switch{
                IfUnt ifUnt => ifUnt.Ast,
                DefFunUnt defFuncUnt => defFuncUnt.Ast,
                InfLoopUnt u => u.Ast,
                ForEachUnt u => u.Ast,
                _ => throw new System.Exception("BlockStUnt.SetAst: grammar symbol no esperado")
            };
        
        }
    }
}