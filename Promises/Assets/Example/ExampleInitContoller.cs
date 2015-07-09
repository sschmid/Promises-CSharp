using UnityEngine;
using Promises;

public class ExampleInitContoller : MonoBehaviour {

    void Awake() {
        MainThreadDispatcher.Init();
    }
}
