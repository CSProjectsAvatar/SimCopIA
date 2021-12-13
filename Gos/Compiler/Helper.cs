using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler {
    static class Helper {
        /// <summary>
        /// Convierte un símbolo a una representación en <see cref="string"/>.
        /// </summary>
        /// <param name="gramSymbol"></param>
        /// <returns></returns>
        public static string SymbolTypeToStr(Type gramSymbol) {
            // si es un token, vamos a devolver un string q identifica al tipo d token, mientras q si
            // es un no-terminal, devolvemos el nombre del tipo
            return gramSymbol.Name == nameof(Token)
                ? ((Token.TypeEnum)gramSymbol  // obtener el tipo del token en string
                        .GetMember(nameof(Token.Type))
                        .GetValue(0))
                    .ToString()
                : gramSymbol.Name;
        }
    }
}
