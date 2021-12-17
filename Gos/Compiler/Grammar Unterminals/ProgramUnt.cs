using System.Collections.Generic;
using DataClassHierarchy;

namespace Compiler {
    internal class ProgramUnt : Unterminal
    {
        protected override AstNode SetAst(IReadOnlyList<GramSymbol> derivation)
        {
            List<IStatement> list = new List<IStatement>();
            foreach (var symbol in derivation)
            {   
                list.Add((((Unterminal)symbol).Ast) as IStatement);
            }
            return new ProgramNode(){
                Statements = list
            };
        }
    }
}