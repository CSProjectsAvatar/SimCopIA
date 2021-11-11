using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Core {
    [TestClass]
    public class OneServerSimulator {
        private double _time; // tiempo global (t)
        private Dictionary<uint, double> _arrTimes; // llave: cliente, valor: tiempo de llegada
        private Dictionary<uint, double> _depTimes; // llave: cliente, valor: tiempo de partida
        private double _tArriv; // tiempo d arribo siguiente (t_A)
        private double _tDep; // tiempo d partida siguiente (t_D)
        private uint _arrivs; // número de arrivos (N_A)
        private uint _departs; // número de partidas (N_D)
        private uint _n; // número de clientes
        private double _maxTime; // tiempo en q el sistema debe cerrar (no arriba + nadie)
        private readonly IEnumerable<Action> _events; // los eventos ordenados en el tiempo
        private readonly Random _rand;
        private readonly ILogger<OneServerSimulator> _log;

        public OneServerSimulator() : this(null) {

        }

        public OneServerSimulator(ILogger<OneServerSimulator> logger) {
            _events = new EventGiver(this);
            _rand = new();
            _log = logger;
        }

        public IDictionary<uint, double> Arrivals => _arrTimes;
        public IDictionary<uint, double> Departures => _depTimes;

        public void Run(double closeTime) {
            Initialize(closeTime);

            foreach (var e in _events) {
                e();
            }
        }

        private void Initialize(double closeTime) {
            _time = _arrivs = _departs = _n = 0u;
            _arrTimes = new();
            _depTimes = new();
            _tArriv = GetT0();
            _tDep = uint.MaxValue;
            _maxTime = closeTime;
        }

        private uint GetT0() {
            return 1;
        }

        /// <summary>
        /// El evento de arribo.
        /// </summary>
        private void Arrival() {
            _log?.LogDebug("Ejecutando Arrivo.");
            
            _time = _tArriv;
            _arrivs++;
            _n++;
            _tArriv = _time + GenArrivalOffset(); // generando el próximo arrivo

            if (_n == 1) {
                _tDep = _time + GenDepartureOffset(); // generando la próxima partida
            }
            _arrTimes[_arrivs] = _time; // registrando el tiempo de este arribo
        }

        /// <summary>
        /// Genera lo q se le suma al tiempo actual para generar un nuevo tiempo de arrivo.
        /// </summary>
        /// <returns></returns>
        private double GenArrivalOffset() {
            return GenTimeOffset();
        }

        private double GenTimeOffset() {
            var lambda = 1.5;

            return -1 / lambda * Math.Log(_rand.NextDouble()); // distribución exponencial
        }

        /// <summary>
        /// El evento de salida.
        /// </summary>
        private void Departure() {
            _log?.LogDebug("Ejecutando Partida.");

            _time = _tDep;
            _departs++;
            _n--;

            if (_n == 0) {
                _tDep = uint.MaxValue;
            } else {
                _tDep = _time + GenDepartureOffset(); // generando la próxima partida
            }
            _depTimes[_departs] = _time; // registrando el tiempo de este arribo
        }

        /// <summary>
        /// Genera lo q se le suma al tiempo actual para generar un nuevo tiempo de partida.
        /// </summary>
        /// <returns></returns>
        private double GenDepartureOffset() {
            return GenTimeOffset();
        }

        /// <summary>
        /// Evento de arribo fuera de tiempo.
        /// </summary>
        private void TimeOutArrival() {
            _log?.LogDebug("Ejecutando Arrivo fuera de tiempo.");

            _tArriv = uint.MaxValue;
        }

        /// <summary>
        /// Evento de cierre.
        /// </summary>
        private void Close() {
            _log?.LogDebug("Ejecutando Cierre.");

            _time = _tDep;
            _departs++;
            _n--;

            if (_n > 0) {
                _tDep = _time + GenDepartureOffset(); // generando la próxima partida
            }
            _depTimes[_departs] = _time; // registrando el tiempo de este arribo
        }

        /// <summary>
        /// Enumera los eventos de manera tal q 100pre te da el próximo evento a ejecutar.
        /// </summary>
        private class EventGiver : IEnumerable<Action> {
            private OneServerSimulator _s;

            public EventGiver(OneServerSimulator oneServerSimulator) {
                _s = oneServerSimulator;
            }

            public IEnumerator<Action> GetEnumerator() {
                while (true) {
                    if (_s._tArriv <= _s._tDep && 
                        _s._tArriv <= _s._maxTime) { // t_A <= t_D  y  t_A <= T

                        yield return _s.Arrival; // arribo

                    } else if (
                        _s._tDep < _s._tArriv &&
                        _s._tDep <= _s._maxTime) { // t_D < t_A  y  t_D <= T

                        yield return _s.Departure; // partida

                    } else if (
                        _s._tArriv <= _s._tDep &&
                        _s._tArriv > _s._maxTime) { // t_A <= t_D  y  t_A > T

                        yield return _s.TimeOutArrival; // arribo fuera d tiempo

                    } else if (
                        _s._tDep <= _s._tArriv &&
                        _s._tDep > _s._maxTime &&
                        _s._n > 0) { // t_D <= t_A  y  t_D > T  y  n > 0

                        yield return _s.Close;

                    } else { // no hay un próximo evento
                        yield break;
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        [TestMethod]
        public void ArrivalsEqualsDepartures() {
            Run(100);

            Assert.AreEqual(_arrivs, _departs);
        }

        [TestMethod]
        public void TimesToInfinity() {
            Run(100);

            Assert.AreEqual(uint.MaxValue, _tArriv);
        }
    }
}
