namespace Compiler {
    /// <summary>
    /// Contiene los parámetros necesarios para ejecutar la simulación.
    /// </summary>
    internal class AppParameters {
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

        /// <summary>
        /// Costo mensual máximo de mantenimiento del sistema.
        /// </summary>
        public int MonthlyMaintenanceCost { get; set; }

        /// <summary>
        /// Tiempo en milisegundos de corrida de la metaheurística.
        /// </summary>
        public long RunTimeMilliseconds { get; set; }
        
        /// <summary>
        /// Número de individuos del algoritmo genético.
        /// </summary>
        public int Poblation { get; set; }
    }
}