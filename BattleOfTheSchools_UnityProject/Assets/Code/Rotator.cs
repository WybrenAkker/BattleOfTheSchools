using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour {

	public Vector3 rotation;
	public float rotateSpeed;
	private float randomStartTime;

	void Start(){
		randomStartTime = Random.Range (0f, 0.1f);
	}


	void Update () {
		if (randomStartTime > 0) {
			randomStartTime -= Time.deltaTime;
		}else{
			transform.Rotate (rotation * rotateSpeed * Time.deltaTime);
		}
	}
}
