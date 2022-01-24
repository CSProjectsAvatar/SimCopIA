

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
                var child = context.CreateChildContext();
                if (!block.All(st => st.Validate(child)))  // @audit esto ta mal, hay q crear un contexto distinto pa kda validacio'n
                    return false;
            }
            return true;
        }
    }
}