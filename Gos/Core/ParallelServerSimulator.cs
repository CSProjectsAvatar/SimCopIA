using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Core
{
    [TestClass]
    public class ParallelServerSimulator
    {
        private uint _totalParallelServers;

        private double _time; // tiempo global (t)
        private Dictionary<uint, double> _arrTimes; // llave: cliente, valor: tiempo de llegada
        private Dictionary<uint, double>[] _depTimes; // array de diccionarios como servidores , llave: cliente, valor: tiempo de psartida
        private Dictionary<uint, double> _allDepTimes;
        private double _tArriv; // tiempo d arribo siguiente (t_A)
       
        //Lista ordenada (futuro heap), que se organiza en base al tiempo de salida de los servidores y tiene como valores (servidor_i,cliente_j)
        private SortedList<double,(uint,uint)> _tDepsData; 
        private Queue<uint> _freeServers; // servidores libres
        private uint _inQueue; // numero en la cola del servidor lider
        private uint _arrivs; // número de arrivos (N_A)
        private uint _n; // número de clientes en el sistema
        private uint _departs; // número de partidas (N_D)
        private double _maxTime; // tiempo en q el sistema debe cerrar (no arriba + nadie)
        

        private readonly IEnumerable<Action> _events; // los eventos ordenados en el tiempo
        private readonly Random _rand;
        private readonly ILogger<ParallelServerSimulator> _log;

        public ParallelServerSimulator(uint totalParallelServers) : this(totalParallelServers,null){

        }
        public ParallelServerSimulator(uint totalParallelServers,ILogger<ParallelServerSimulator> logger) {
            _events = new EventGiver(this);
            _rand = new();
            _totalParallelServers = totalParallelServers;
            _log = logger;
            
        }

        public IDictionary<uint, double> Arrivals => _arrTimes;
        public IDictionary<uint, double>[] DeparturesPerServer => _depTimes;
        public IDictionary<uint, double> Deapertures => _allDepTimes;
    
        public void Run(double closeTime) {
            Initialize(closeTime);

            foreach (var e in _events) {
                e();
            }
        }
        private void Initialize(double closeTime) {
            _time = _arrivs = _departs = _n = 0u;
            _arrTimes = new();

            _depTimes = new Dictionary<uint, double>[this._totalParallelServers];
            for(int i= 0; i<_depTimes.Length ;i++)
                _depTimes[i] = new();

            _allDepTimes = new Dictionary<uint, double>();
            _tArriv = GetT0();
            _inQueue = 0;
            _tDepsData = new SortedList<double, (uint,uint)>();
            
            _freeServers = new Queue<uint>();
            for(uint i=0; i<this._totalParallelServers; i++)
                _freeServers.Enqueue(i);

            _maxTime = closeTime;
        }
         private uint GetT0() {
            return 1;
        }
        /// <summary>
        /// El evento de arribo.
        /// </summary>
        private void Arrival() {
            _log?.LogDebug("Ejecutando Arribo");

            _time = _tArriv;
            _arrivs ++;
            _n++;
            _tArriv = _time + GenArrivalOffset();
            
            if ( _freeServers.Count == 0 ){
                _inQueue++;
            }
            else{
                var s=_freeServers.Dequeue();
                var c= _arrivs; // client 
                _tDepsData.Add(
                                _time + GenDepartureOffset() ,
                                (s,c)
                            );
            }
            _arrTimes[_arrivs] = _time;
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
            _log?.LogDebug("Ejecutando Partida");
           
            //tiempo de salida, servidor del que se sale, cliente que sale
            (double time , uint s, uint c) = GetMin(true);

            _time=time;
            _n--;
            _departs ++;

            if (_inQueue != 0){
                
                _inQueue--;                
                var c1 = _arrivs - _inQueue ;  // primer cliente de la cola que entra al servidor s

                _tDepsData.Add(
                                time + GenDepartureOffset(),
                                (s,c1)
                            );
            } else{
                _freeServers.Enqueue(s);
            }

            _depTimes[s][c] = time; 
            _allDepTimes[c] = time; 
        }

        private double GenDepartureOffset() {
            return GenTimeOffset();
        }

        private void TimeOutArrival(){
            
            _log.LogDebug("Ejecutando Entrada fuera de tiempo.");

            _tArriv = uint.MaxValue;
        }

         /// <summary>
        /// Evento de cierre.
        /// </summary>
        private void Close() {
            _log.LogDebug("Ejecutando Cierre.");

            //tiempo de salida, servidor del que se sale, cliente que sale
            (double time , uint s, uint c) = GetMin(true);

            _time=time;
            _n--;
            _departs ++;

            if (_inQueue != 0){
                
                _inQueue--;                
                var c1 = _arrivs - _inQueue ; // primer cliente de la cola que entra al servidor s

                _tDepsData.Add(
                                time + GenDepartureOffset(),
                                (s,c1)
                            );
            }

            _depTimes[s][c] = time; 
            _allDepTimes[c] = time; 

        }


        /// <summary>
        /// Extrae el proximo cliente de un servidor. 
        /// Se puede ver o extraer segun se pase el bool remove.
        /// </summary>
        /// <returns>
        ///     (tiempo_de_salida, servidor_del_que_se_sale, cliente_que_sale)
        /// </returns>
        
       // [Obsolete("Cambiar por un heap de minimos el _tDepsData !!",false)]
        private (double, uint, uint) GetMin(bool remove=false){
            
            if (this._tDepsData.Count == 0)
                return (uint.MaxValue,0,0);
            var e = this._tDepsData.GetEnumerator();
            e.MoveNext();
            var current = e.Current;
            (double, uint, uint) sol = (current.Key,current.Value.Item1,current.Value.Item2) ;
            
            if (remove)
                _tDepsData.RemoveAt(0);

            return sol;
        }

        /// <summary>
        /// Enumera los eventos de manera tal q 100pre te da el próximo evento a ejecutar.
        /// </summary>
        private class EventGiver : IEnumerable<Action> {
            private ParallelServerSimulator _s;

            public EventGiver(ParallelServerSimulator parallelServerSimulator) {
                _s = parallelServerSimulator;
            }

            public IEnumerator<Action> GetEnumerator() {
                while (true) {
                    
                    double minDeapertureTime =_s.GetMin(false).Item1; //el menor tiempo de salida

                    
                    if (_s._tArriv <= minDeapertureTime && _s._tArriv  <= _s._maxTime){

                        yield return _s.Arrival;

                    } else if ( minDeapertureTime < _s._tArriv && minDeapertureTime < _s._maxTime ){
                        
                        yield return _s.Departure;

                    } else if (_s._tArriv < minDeapertureTime && _s._tArriv > _s._maxTime && _s._n > 0 ){

                        yield return _s.TimeOutArrival;

                    } else if (_s._tArriv > minDeapertureTime && _s._tArriv > _s._maxTime && _s._n > 0 ){

                        yield return _s.Close;

                    } else {

                        yield break;
                        
                    }

                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }
    }
}
