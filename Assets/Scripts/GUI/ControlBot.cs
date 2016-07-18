using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlBot : MonoBehaviour {

	public float _MoveSpeed = 0.4f;
	float _RotationY = 0.0f;
	public static float SLookSpeed = 1.8f;

	public List<GameObject> Vertices;


	// Use this for initialization
	void Start () {

			
	}
	
	// Update is called once per frame
	void Update () {

			_RotationY += Input.GetAxis("Horizontal") * SLookSpeed;
			if (_RotationY != 0.0f)
			{ 
				_RotationY = Mathf.Clamp(_RotationY, -90, 90);
				transform.localRotation *= Quaternion.AngleAxis(_RotationY, Vector3.back);
				_RotationY = 0;
			}

			if (Input.GetKeyDown(KeyCode.T))
			{
				Debug.Log("TTTTTTTTTTTTTTTTTTTTTTTT");
				Vector3 pos = Vertices[0].transform.localPosition;
				pos.x -= 5;
				Vertices[0].transform.localPosition = pos;
				pos = Vertices[1].transform.localPosition;
				pos.x += 5;
				Vertices[1].transform.localPosition = pos;
			}


			if (Input.GetKeyDown(KeyCode.R))
			{
				Debug.Log("TTTTTTTTTTTTTTTTTTTTTTTT");
				Vector3 pos = Vertices[0].transform.localPosition;
				pos.x += 5;
				Vertices[0].transform.localPosition = pos;
				pos = Vertices[1].transform.localPosition;
				pos.x -= 5;
				Vertices[1].transform.localPosition = pos;
			}


			if (Input.GetKeyDown(KeyCode.F))
			{
				Debug.Log("TTTTTTTTTTTTTTTTTTTTTTTT");
				Vector3 pos = Vertices[0].transform.localPosition;
				pos.x += 5;
				Vertices[0].transform.localPosition = pos;
				pos = Vertices[1].transform.localPosition;
				pos.x -= 5;
				Vertices[1].transform.localPosition = pos;
			}


		transform.position += transform.up * _MoveSpeed * Input.GetAxis("Vertical");

	}
}

