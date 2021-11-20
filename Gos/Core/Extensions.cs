using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public static class Extensions {
        /// <summary>
        /// Determina si <paramref name="x"/> <= <paramref name="y"/> sin que surjan problemas asociados a la 
        /// aritmética de punto flotante.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Leq(this double x, double y) {
            return x < y || x.Eq(y);
        }

        /// <summary>
        /// Determina si <paramref name="x"/> = <paramref name="y"/> sin que surjan problemas asociados a la 
        /// aritmética de punto flotante.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Eq(this double x, double y) {
            var eps = 1e-10;

            return Math.Abs(x - y) < eps;
        }

        /// <summary>
        /// Determina si <paramref name="x"/> != <paramref name="y"/> sin que surjan problemas asociados a la 
        /// aritmética de punto flotante.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Neq(this double x, double y) {
            return !x.Eq(y);
        }
    }
}
