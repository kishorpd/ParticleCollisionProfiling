using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {

	public float lineWidth = 0.1f;
	public Vector3 StartPoint = new Vector3(-31, 22,0);
	public Vector3 EndPoint =	new Vector3(-31,-22,0);

	public Material lineColor;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;

	// Use this for initialization
	void Start () {

		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = lineColor;// new Material(Shader.Find("Particles/Additive"));
		//lineRenderer.SetColors(c1, c2);
		lineRenderer.SetWidth(lineWidth, lineWidth);
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
	}
	
	// Update is called once per frame
	void Update () {
		LineRenderer lineRenderer = GetComponent<LineRenderer>();

		lineRenderer.SetPosition(0, StartPoint);
		lineRenderer.SetPosition(1, EndPoint);
	}
}
