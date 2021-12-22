using DataClassHierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.AstHierarchy {
    class SetAst : BaseRegexAst {
        public SetAst(IEnumerable<BaseRegexAst> items) {
            Items = items;
        }

        public IEnumerable<BaseRegexAst> Items { get; }

        public override bool Validate(Context context) {
            return Items.Aggregate(true, (accum, i) => accum && i.Validate(context));  // validando todos los elementos
        }
    }
}
