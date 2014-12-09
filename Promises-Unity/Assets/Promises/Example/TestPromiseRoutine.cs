using UnityEngine;
using System.Collections;
using Promises;

public class TestPromiseRoutine : MonoBehaviour {
	
	void Start () 
	{
		var anotherPromise = Promise.WithCoroutine<object>(TestRoutine2("TestWithAutoStart", 0.5f));
		var lazyPromise = Promise.WithCoroutine<object>(TestRoutine2("Lazy", 2), false);
		var promise = anotherPromise.Then<object>(lazyPromise);

		lazyPromise.OnFulfilled += HandleOnFulfilled1;
		anotherPromise.OnFulfilled += HandleOnFulfilled1;

		lazyPromise.OnFailed += (error) => WoogaDebug.Log(error);
		anotherPromise.OnFailed += (error) => WoogaDebug.Log(error);

		promise.OnFulfilled += (result) => Debug.Log("sequence done");

		promise.Stop();
//		var promise = Promise.WithCoroutine<object>(TestRoutine());;
//
//		var p = Promise.All(promise, anotherPromise);
//
//		p.OnFulfilled += HandleOnFulfilled2;
//		p.OnFailed += HandleOnFailed;
//
//		promise.OnFulfilled += HandleOnFulfilled;
//		promise.OnFailed += HandleOnFailed;
//
//		anotherPromise.OnFulfilled += HandleOnFulfilled1;
//		anotherPromise.OnFailed += HandleOnFailed;
//
//		var badPromise = Promise.WithCoroutine<string>(TestExceptionThrowing());
//		badPromise.OnFailed += HandleOnFailed;
	}

	void HandleOnFulfilled2 (object[] result)
	{
		foreach (object obj in result)
		{
			Debug.Log("obj: " + obj);
		}
	}

	void HandleOnFailed (System.Exception error)
	{
		Debug.Log("failed: " + error.ToString());
	}

	void HandleOnFulfilled1 (object result)
	{
		Debug.Log("result: " + (string)result);
	}

	void HandleOnFulfilled (object result)
	{
		Debug.Log ("result: " + (int)result);
	}

	IEnumerator TestRoutine ()
	{
		yield return new WaitForSeconds(2);
		yield return 20;
	}

	IEnumerator TestRoutine2 (string s, float delay)
	{
		yield return new WaitForSeconds(delay);
		WoogaDebug.Log("TestRoutine running");
		yield return (object)s.ToUpper();
	}

	IEnumerator TestExceptionThrowing ()
	{
		yield return new WaitForSeconds(1);
		string s = null;
		s.Contains("s");

		yield return s;
	}
}
