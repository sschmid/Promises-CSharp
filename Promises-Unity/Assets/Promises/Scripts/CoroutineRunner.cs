using UnityEngine;
using System;
using System.Collections;

namespace Promises
{
	public class CoroutineRunner : MonoBehaviour
	{
		static CoroutineRunner instance;
		
		public static Coroutine<T> StartRoutine<T> (IEnumerator coroutine)
		{
			if (!instance)
			{
				var go = new GameObject("CoroutineManager");
				instance = go.AddComponent<CoroutineRunner>();
			}
			return instance.StartCoroutine<T>(coroutine);
		}
		
		public static Coroutine<T> StartRoutine<T> (IEnumerator coroutine, Action<Coroutine<T>> callback)
		{
			if (!instance)
			{
				var go = new GameObject("CoroutineManager");
				instance = go.AddComponent<CoroutineRunner>();
			}
			return instance.StartCoroutine<T>(coroutine, callback);
		}
		
		public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine)
		{
			Coroutine<T> coroutineObject = new Coroutine<T>();
			coroutineObject.coroutine = StartCoroutine(coroutineObject.InternalRoutine(coroutine));
			return coroutineObject;
		}
		
		public Coroutine<T> StartCoroutine<T>(IEnumerator coroutine, Action<Coroutine<T>> callback)
		{
			Coroutine<T> coroutineObject = new Coroutine<T>();
			coroutineObject.coroutine = StartCoroutine(coroutineObject.InternalRoutine(coroutine));
			StartCoroutine(CallbackRoutine<T>(coroutineObject, callback));
			return coroutineObject;
		}
		
		void Awake ()
		{
			DontDestroyOnLoad(gameObject);
		}
		
		IEnumerator CallbackRoutine<T> (Coroutine<T> coroutine, Action<Coroutine<T>> callback)
		{
			yield return coroutine.coroutine;
			callback(coroutine);
		}
	}
}