using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    [TestClass]
    public class OneLeaderFollowersSimulator {
        private double _time; // tiempo global (t)
        private Dictionary<uint, double> _arrTimes; // llave: cliente, valor: tiempo de llegada
        private Dictionary<uint, double>[] _depTimes; // [i] = llave: cliente, valor: tiempo de partida del i-ésimo servidor
        private double _tLArriv; // siguiente tiempo d arribo al líder (t_A_1)
        private double _tFArriv; // siguiente tiempo d arribo a los seguidores (t_A_2)
        private uint _arrivs; // número de arribos (N_A)
        private uint _departs; // número de partidas (N_D)
        private uint _n1; // número de clientes del líder
        private uint _n; // número de clientes en el sistema
        private double _maxTime; // tiempo en q el sistema debe cerrar (no arriba + nadie; T)
        private Queue<uint> _freeServers; // servidores libres
        private uint _inQueue; // numero en la cola del servidor líder
        private uint _follows; // cantd d seguidores
        private SortedList<double, (uint, uint)> _tDepsData; // lista ordenada (futuro heap), que se organiza en base al tiempo de salida de los servidores y tiene como valores (servidor_i, cliente_j)
        private readonly IEnumerable<Action> _events; // los eventos ordenados en el tiempo
        private readonly Random _rand;
        private readonly ILogger<OneLeaderFollowersSimulator> _log;
        private readonly double _lambda;

        public Dictionary<uint, double> Arrivals => _arrTimes;

        public OneLeaderFollowersSimulator() : this(5, 1.5, null) {

        }

        public OneLeaderFollowersSimulator(uint followers, double lambda) : this(followers, lambda, null) {

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="followers">Cantidad de seguidores (doers).</param>
        /// <param name="logger"></param>
        public OneLeaderFollowersSimulator(uint followers, ILogger<OneLeaderFollowersSimulator> logger) :
                this(followers, 1.5, logger) {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="followers">Cantidad de seguidores (doers).</param>
        /// <param name="lambda">Parámetro lambda de la distribución exponencial para determinar tiempos de 
        /// ocurrencia de los eventos.</param>
        /// <param name="logger"></param>
        public OneLeaderFollowersSimulator(
                uint followers,
                double lambda,
                ILogger<OneLeaderFollowersSimulator> logger) {
            _events = new EventGiver(this);
            _rand = new();
            _log = logger;
            _follows = followers;
            _lambda = lambda;
        }

        public Dictionary<uint, double> GetDepartures() {
            return _depTimes
                .SelectMany(d => d)
                .ToDictionary(d => d.Key, d => d.Value);
        }

        /// <summary>
        /// Tiempo de cierre del sistema (T). Cuando se arribe a este tiempo, no se recibirán más pedidos.
        /// </summary>
        /// <param name="closeTime"></param>
        public void Run(double closeTime) {
            Initialize(closeTime);

            foreach (var e in _events) {
                e();
            }
        }

        private void Initialize(double closeTime) {
            _time = _arrivs = _departs = _n1 = _n = 0u;
            _arrTimes = new();

            _depTimes = new Dictionary<uint, double>[_follows];
            for (int i = 0; i < _depTimes.Length; i++)
                _depTimes[i] = new();

            _tLArriv = GetT0();
            _tFArriv = double.MaxValue;
            _inQueue = 0;
            _tDepsData = new SortedList<double, (uint, uint)>();

            _freeServers = new Queue<uint>();
            for (uint i = 0; i < _follows; i++)
                _freeServers.Enqueue(i);

            _maxTime = closeTime;
        }

        private uint GetT0() {
            return 1;
        }

        /// <summary>
        /// Evento de arribo al líder.
        /// </summary>
        private void ArrivalToLeader() {
            _log?.LogDebug("Ejecutando arribo al líder");

            _time = _tLArriv;
            _arrivs++;
            _n1++;
            _n++;
            _tLArriv = _time + GenArrivalToLeaderOffset(); // generando próximo arribo al líder

            if (_n1 == 1) {
                _tFArriv = _time + GenArrivalToFollowersOffset(); // generando próximo arribo a seguidores
            }
            _arrTimes[_arrivs] = _time;
        }

        /// <summary>
        /// Evento de arribo fuera de tiempo al líder.
        /// </summary>
        private void TimeOutArrivalToLeader() {
            _log?.LogDebug("Ejecutando arribo fuera de tiempo al líder");

            _tLArriv = double.MaxValue;
        }

        /// <summary>
        /// Evento de arribo a los seguidores.
        /// </summary>
        private void ArrivalToFollowers(bool late) {
            _log?.LogDebug("Ejecutando arribo " + (late ? "fuera de tiempo " : "") + "a los seguidores");

            _time = _tFArriv;
            _n1--;

            if (_n1 != 0) {
                _tFArriv = _time + GenArrivalToFollowersOffset();
            } else {
                _tFArriv = double.MaxValue;
            }
            var client = _arrivs - _n1; // ID del cliente = N_A - n1

            if (_freeServers.Count == 0) {
                _inQueue++;

                _log?.LogDebug($"Cliente {client} hace cola.");
            } else {
                var serv = _freeServers.Dequeue();
                _tDepsData.Add(
                    _time + GenDepartureOffset(),
                    (serv, client));
            }
        }

        /// <summary>
        /// El evento de salida y cierre (son equivalentes).
        /// </summary>
        private void Departure(bool isClose) {
            _log?.LogDebug("Ejecutando " + (isClose ? "Cierre" : "Partida"));

            //tiempo de salida, servidor del que se sale, cliente que sale
            (double depTime, uint serv, uint outClient) = GetMinDep(true);

            _time = depTime;
            _departs++;
            _n--;

            if (_inQueue != 0) {

                _inQueue--;
                var inClient = _arrivs - _n1 - _inQueue;  // primer cliente de la cola que entra al servidor s

                _tDepsData.Add(
                    depTime + GenDepartureOffset(),
                    (serv, inClient));
            } else {
#if DEBUG
                if (_freeServers.Contains(serv)) {
                    throw new InvalidOperationException("Servidor ya libre.");
                }
#endif

                _freeServers.Enqueue(serv);
            }

            _depTimes[serv][outClient] = depTime; // registrando partida del cliente por el servidor
        }

        // <summary>
        /// Genera lo q se le suma al tiempo actual para generar un nuevo tiempo de arribo al líder.
        /// </summary>
        /// <returns></returns>
        private double GenArrivalToLeaderOffset() {
            return GenTimeOffset();
        }

        // <summary>
        /// Genera lo q se le suma al tiempo actual para generar un nuevo tiempo de arribo a los
        /// seguidores.
        /// </summary>
        /// <returns></returns>
        private double GenDepartureOffset() {
            return GenTimeOffset();
        }

        private double GenTimeOffset() {

            return -1 / _lambda * Math.Log(_rand.NextDouble()); // distribución exponencial
        }

        private double GenArrivalToFollowersOffset() {
            return GenTimeOffset();
        }

        #region tests
        [TestMethod]
        public void ArrivalsEqualsDepartures() {
            Run(100);

            Assert.AreEqual(_arrivs, _departs);
        }

        [TestMethod]
        public void TimesToInfinity() {
            Run(100);

            Assert.AreEqual(double.MaxValue, _tLArriv);
            Assert.AreEqual(double.MaxValue, _tFArriv);
        }

        [TestMethod]
        public void AllServerAreFreeTest() {
            Run(100);

            Assert.AreEqual(_follows, (uint)_freeServers.Count); // conteo
            Assert.AreEqual(_follows, (uint)new HashSet<uint>(_freeServers).Count); // unicidad
            Assert.AreEqual(0, _tDepsData.Count); // info vacía
        }

        [TestMethod]
        public void EmptyParallelQueue() {
            Run(100);

            Assert.AreEqual(0u, _inQueue);
        }

        [TestMethod]
        public void EmptySystem() {
            Run(100);

            Assert.AreEqual(0u, _n);
        }
        #endregion

        /// <summary>
        /// Enumera los eventos de manera tal q 100pre te da el próximo evento a ejecutar.
        /// </summary>
        private class EventGiver : IEnumerable<Action> {
            private OneLeaderFollowersSimulator _s;

            public EventGiver(OneLeaderFollowersSimulator simulator) {
                _s = simulator;
            }

            public IEnumerator<Action> GetEnumerator() {
                while (true) {
                    double minDep = _s.GetMinDep(false).Item1; //el menor tiempo de salida
                    double minTime = Times(minDep).Min();

                    if (
                            _s._tLArriv.Eq(minTime) && // t_A_1 <= to los demás tiempos
                            _s._tLArriv.Leq(_s._maxTime)) { // t_A_1 <= T
                        yield return _s.ArrivalToLeader;
                    } else if (
                            _s._tLArriv.Neq(double.MaxValue) && // no ha llegado un evento d este tipo antes (ver https://github.com/CSProjectsAvatar/SimCopIA/issues/4)
                            _s._tLArriv.Eq(minTime) && // t_A_1 <= to los demás tiempos
                            _s._tLArriv > _s._maxTime) { // t_A_1 > T
                        yield return _s.TimeOutArrivalToLeader;
                    } else if (
                            _s._tFArriv.Eq(minTime) && // t_A_2 <= to los demás tiempos
                            _s._tFArriv.Leq(_s._maxTime)) { // t_A_2 <= T
                        yield return () => _s.ArrivalToFollowers(false);
                    } else if (
                            _s._tFArriv.Neq(double.MaxValue) && // no ha llegado un evento d este tipo antes (ver https://github.com/CSProjectsAvatar/SimCopIA/issues/4)
                            _s._tFArriv.Eq(minTime) && // t_A_2 <= to los demás tiempos
                            _s._tFArriv > _s._maxTime) { // t_A_2 > T
                        yield return () => _s.ArrivalToFollowers(true);
                    } else if (
                            minDep.Eq(minTime) && // t_D_i <= to los demás tiempos
                            minDep.Leq(_s._maxTime)) { // t_D_i <= T

                        yield return () => _s.Departure(false);
                    } else if (
                            minDep.Eq(minTime) && // t_D_i <= to los demás tiempos
                            minDep > _s._maxTime && // t_D_i > T
                            minDep.Neq(double.MaxValue) && // pa q no c llame más d la cuenta (ver https://github.com/CSProjectsAvatar/SimCopIA/issues/5)
                            _s._n > 0) { // n > 0

                        yield return () => _s.Departure(true);
                    } else {
                        yield break;
                    }
                }
            }

            /// <summary>
            /// Returns all the time variables but the global.
            /// </summary>
            /// <param name="minDepartureTime"></param>
            /// <returns></returns>
            private IEnumerable<double> Times(double minDepartureTime) {
                return new[] { _s._tLArriv, _s._tFArriv, minDepartureTime };
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        /// <summary>
        /// Extrae el proximo cliente de un servidor. 
        /// Se puede ver o extraer segun se pase el bool remove.
        /// </summary>
        /// <returns>
        ///     (tiempo_de_salida, servidor_del_que_se_sale, cliente_que_sale)
        /// </returns>
        private (double, uint, uint) GetMinDep(bool remove = false) {

            if (this._tDepsData.Count == 0)
                return (double.MaxValue, 0, 0);
            var e = this._tDepsData.GetEnumerator();
            e.MoveNext();
            var current = e.Current;
            (double, uint, uint) sol = (current.Key, current.Value.Item1, current.Value.Item2);

            if (remove)
                _tDepsData.RemoveAt(0);

            return sol;
        }
    }
}
