using UnityEngine;
using System;
using System.Collections;

namespace Promises
{
	public class Coroutine<T>
	{
		public T Value {
			get{
				if(e != null){
					throw e;
				}
				return returnVal;
			}
		}
		
		private T returnVal;
		private Exception e;
		public Coroutine coroutine;

		private bool isRunning = true;
		public bool IsRunning { get { return isRunning; } }
		
		public IEnumerator InternalRoutine(IEnumerator coroutine){
			while(isRunning){
				try{
					if(!coroutine.MoveNext()){
						yield break;
					}
				}
				catch(Exception e){
					this.e = e;
					yield break;
				}
				object yielded = coroutine.Current;
				if(yielded != null && !(yielded is YieldInstruction || yielded is IEnumerator || yielded is WWW || yielded is AssetBundleCreateRequest)){
					returnVal = (T)yielded;
					yield break;
				}
				else{
					yield return coroutine.Current;
				}
			}
		}

		public void Stop ()
		{
			isRunning = false;
		}
	}
}
