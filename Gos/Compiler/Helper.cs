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
        public static string SymbolTypeToStr(GramSymType gramSymbol) {
            // si es un token, vamos a devolver un string q identifica al tipo d token, mientras q si
            // es un no-terminal, devolvemos el nombre del tipo
            return gramSymbol.ToString();
        }

        public static bool Inherits(this Type t, Type ancestor) {
            Type @base = t;

            do {
                if (@base.Name == ancestor.Name) {
                    return true;
                }
                @base = @base.BaseType;

            } while (@base != null);

            return false;
        }

        /// <summary>
        /// Devuelve una función que ejecuta <paramref name="action"/> y sólo puede ser llamada una vez.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <returns></returns>
        internal static Action<T> Once<T>(Action<T> action) {
            var called = false;
            return t => {
                if (called) {
                    throw new InvalidOperationException("Function can only be called once.");
                }
                called = true;
                action(t);
            };
        }
    }
}
