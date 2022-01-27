using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.AstHierarchy {
    class UnionAst : BaseRegexAst {
        public UnionAst(BaseRegexAst left, BaseRegexAst right) {
            Left = left;
            Right = right;
        }

        public BaseRegexAst Left { get; }
        public BaseRegexAst Right { get; }

        public override bool Validate(Context context) {
            return Left.Validate(context) && Right.Validate(context);
        }
    }
}
