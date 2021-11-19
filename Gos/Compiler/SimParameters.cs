namespace Compiler {
    /// <summary>
    /// Contiene los parámetros necesarios para ejecutar la simulación.
    /// </summary>
    internal class SimParameters {
        /// <summary>
        /// Cantidad de seguidores (doers).
        /// </summary>
        public uint Followers { get; set; }

        /// <summary>
        /// Parámetro lambda de la distribución exponencial para determinar tiempos de ocurrencia de los eventos.
        /// </summary>
        public double Lambda { get; set; }

        /// <summary>
        /// Tiempo de cierre del sistema (T). Cuando se arribe a este tiempo, no se recibirán más pedidos.
        /// </summary>
        public double CloseTime { get; set; }
    }
}