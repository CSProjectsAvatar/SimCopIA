using System;
using System.Collections.Generic;
namespace Utils
{

    public static class UtilsT
    {   // Usar GetTimeOffset() en EventCreator y getReqTimeToProcess()
        internal static Random Rand = new Random(Environment.TickCount);
        internal static double CostByMicro = 1;
        public static void SetCostByMicro(double cost)
        {
            CostByMicro = cost;
        }
        public static double GenTimeOffset(double lambda = 1.5) {
            return -1 / lambda * Math.Log(Rand.NextDouble());
        }
    }
    public class Heap<T> {
        
        
        private List<(int,T)> _data;
        public int Count => this._data.Count;
        public (int, T) First => this._data[0];
        public Heap(){
            this._data = new();
        }
        private void heapfyUp(int k){
            
            int father = Heap<T>.father(k);
            if (k!=0 &&_data[k].Item1 <_data[father].Item1 ){
               swap(k,father) ;
               heapfyUp(father);
            }
        }
        private void HeapfyDown(int index){
            int mini = index; 
            int left = Heap<T>.leftChild(index);
            int right = Heap<T>.rightChild(index);

            if (left < Count && _data[mini].Item1 > _data[left].Item1 )
                mini = left;
            if (right < Count && _data[mini].Item1 > _data[right].Item1 )
                mini = right;
            if (mini != index){
                swap(mini, index);
                HeapfyDown(mini);
            }
        }
        public void Add(int comp,T element){
            _data.Add((comp,element));
            heapfyUp(this.Count-1);
        }
        public (int,T) RemoveMin(){
            if(Count==0) throw new Exception("Heap is Empty!!");
            var sol = _data[0];
            _data[0] = _data[this.Count -1 ];
            _data.RemoveAt(Count-1);
            HeapfyDown(0);
            return sol;
        }
        private void swap(int x, int y){
            var temp = _data[x];
            _data[x]= _data[y];
            _data[y]= temp;
        }
        static int leftChild(int index) => index*2 + 1;
        static int rightChild(int index) => leftChild(index)+1;
        static int father(int index) => (index -1)/2;
        
    }
}