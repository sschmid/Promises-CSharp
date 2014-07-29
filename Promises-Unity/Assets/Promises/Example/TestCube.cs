using UnityEngine;

public class TestCube : MonoBehaviour {

	void Start () {
        Application.targetFrameRate = 60;
	}
	
	void Update () {
        if (transform.position.y < -10)
            transform.position = new Vector3(0, 10f, 0);
	}
}
