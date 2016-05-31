using UnityEngine;
using System.Collections;


public class Main : MonoBehaviour {

	public float lineWidth = 1.5f;
	public Vector3 StartPoint = new Vector3(-31, 22,0);
	public Vector3 EndPoint =	new Vector3(-31,-22,0);

	public GameObject ParticlePrefab;
	 //List<GameObject> Particles = new List<GameObject>();

	public Material lineColor;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;

	Ray myRay;      // initializing the ray
	RaycastHit hit; // initializing the raycasthit

	// Use this for initialization
	void Start () {
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = lineColor;// new Material(Shader.Find("Particles/Additive"));
		//lineRenderer.SetColors(c1, c2);
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
	}
	
	// Update is called once per frame
	void Update () {
		DrawLine(StartPoint, EndPoint);

		SpawnParticleOnClick();
	}

	void DrawLine(Vector3 Start, Vector3 End)
	{ 
		LineRenderer lineRenderer = GetComponent<LineRenderer>();

		lineRenderer.SetWidth(lineWidth, lineWidth);
		lineRenderer.SetPosition(0, Start);
		lineRenderer.SetPosition(1, End);

	}

	void SpawnParticleOnClick()
	{ 	
		myRay = Camera.main.ScreenPointToRay(Input.mousePosition); 
		if (Physics.Raycast (myRay, out hit))  
		{
			if (Input.GetKey(KeyCode.Mouse0))
			{
				//GameObject obj =
					Instantiate(ParticlePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity);// as GameObject;
			}
		}
	}

}
