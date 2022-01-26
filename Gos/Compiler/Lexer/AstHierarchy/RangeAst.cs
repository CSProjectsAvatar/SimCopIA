using DataClassHierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.Lexer.AstHierarchy {
    /// <summary>
    /// El nodo correspondiente a la operación <code>a-z</code>
    /// </summary>
    class RangeAst : BaseRegexAst {
        public RangeAst(char left, char right, ILogger<RangeAst> logger) {
            Left = left;
            Right = right;
            _log = logger;
        }

        public char Left { get; }
        public char Right { get; }

        private readonly ILogger<RangeAst> _log;

        public override bool Validate(Context context) {
            if (Left > Right) {
                _log.LogError("Char at left must be first than char at right, however {left} > {right}.", Left, Right);
                return false;
            }
            if (char.IsLower(Left) ^ char.IsLower(Right)) {  // capitalizacio'n distinta
                _log.LogError("Both chars must be lower-cased or both upper-cased, however we have {left} and {right}.",
                    Left, Right);
                return false;
            }
            return true;
        }
    }
}
