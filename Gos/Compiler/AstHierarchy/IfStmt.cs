

using System.Collections.Generic;
using System.Linq;

namespace DataClassHierarchy {
    public class IfStmt:Expression, IStatement {
        public IEnumerable<Expression> Conditions { get; set; }  // @audit pon esto priva2, pa eso hay q refactorizar algunos tests
        public IReadOnlyList<IEnumerable<IStatement>> Thens { get; set; }  // @audit pon esto priva2, pa eso hay q refactorizar algunos tests

        public override bool Validate(Context context) {
            if (!Conditions.All(c => c.Validate(context))) {
                return false;
            }
            foreach (var block in Thens) {
                if (!block.All(st => st.Validate(context)))
                    return false;
            }
            return true;
        }
    }
}