namespace Compiler {
    /// <summary>
    /// Los tipos del lenguaje GoS.
    /// </summary>
    internal enum GosType {
        Number,
        Bool,
        Server,
        List,
        Null,
        ServerStatus,
        Response,

        /// <summary>
        /// Para el tipo de percepcio'n <see cref="ServersWithLayers.Observer"/>
        /// </summary>
        Alarm,
        Request,
        Environment,
        String
    }
}