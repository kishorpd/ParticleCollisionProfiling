using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour
{

	public float lineWidth = 1.5f;
	public float BorderOffsetX = 15f;
	public float BorderOffsetY = 15f;
	public float BorderWidth = 120f;
	public float BorderHeight = 95f;
	Vector3 StartPoint = new Vector3(-31, 22, 0);
	Vector3 EndPoint = new Vector3(-31, -22, 0);

	private QuadTree _QuadTree;
	public QuadTree AQuadTree;

	public GameObject ParentPrefab;
	public GameObject ParticlePrefab;
	List<GameObject> Particles;

	public Material lineColor;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;

	Ray myRay;      // initializing the ray
	RaycastHit hit; // initializing the raycasthit

	// Use this for initialization
	void Start()
	{
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = lineColor;// new Material(Shader.Find("Particles/Additive"));
		//lineRenderer.SetColors(c1, c2);
		lineRenderer.SetVertexCount(lengthOfLineRenderer);
		Particles = new List<GameObject>();
		ParentPrefab = Instantiate(ParentPrefab, new Vector3(-30, 0, 9), Quaternion.identity) as GameObject;
		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);

		GameObject testPrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		Instantiate(testPrefab, ParentPrefab.transform.position, Quaternion.identity);
	}

	// Update is called once per frame
	void Update()
	{
		SpawnParticleOnClick();
	}

	///////	void DrawSquare()
	///////	{
	///////		StartPoint.x = BorderWidth - BorderOffsetX;
	///////		StartPoint.y = BorderHeight + BorderOffsetY;
	///////
	///////		EndPoint.x = BorderWidth - BorderOffsetX;
	///////		EndPoint.y = -BorderHeight + BorderOffsetY;
	///////
	///////		//DrawLine(StartPoint, EndPoint);
	///////
	///////
	///////
	///////		LineRenderer lineRenderer = GetComponent<LineRenderer>();
	///////
	///////		lineRenderer.SetWidth(lineWidth, lineWidth);
	///////		lineRenderer.SetVertexCount(4); 
	///////		lineRenderer.SetPosition(0, StartPoint);
	///////		lineRenderer.SetPosition(1, EndPoint);
	///////
	///////		StartPoint.x = -BorderWidth + BorderOffsetX;
	///////		StartPoint.y = BorderHeight + BorderOffsetY;
	///////
	///////		EndPoint.x = -BorderWidth + BorderOffsetX;
	///////		EndPoint.y = -BorderHeight + BorderOffsetY;
	///////		lineRenderer.SetPosition(2, EndPoint);
	///////		lineRenderer.SetPosition(3, StartPoint);
	/////////		DrawLine1(StartPoint, EndPoint);
	///////	}


	/////////void OnDrawGizmosSelected()
	/////////{
	/////////	Gizmos.color = new Color(1, 0, 0, 0.5F);
	/////////	Gizmos.DrawCube(transform.position, new Vector3(100, 1, 1));
	/////////}

	///void DrawLine(Vector3 Start, Vector3 End)
	///{
	///	LineRenderer lineRenderer = GetComponent<LineRenderer>();
	///
	///	lineRenderer.SetWidth(lineWidth, lineWidth);
	///	lineRenderer.SetPosition(0, Start);
	///	lineRenderer.SetPosition(1, End);
	///
	///}


	void SpawnParticleOnClick()
	{
		myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(myRay, out hit))
		{
			//if (Input.GetKey(KeyCode.Mouse0))
			{
				if (Input.GetMouseButtonUp(0))
				{
					//instantiate and add in the list
					GameObject ParticleObject;
					Particles.Add(ParticleObject = Instantiate(ParticlePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity) as GameObject);
					//Debug.Log("Particles.pos:" + hit.point);
					//insert in the QuadTree
					_QuadTree.Insert(ParticleObject);
				}
				if (Input.GetMouseButtonUp(1))
				{
					//TODO: fix right click at appropriate time
					Debug.Log("Pos:" + (hit.transform.gameObject.transform.position));
					Debug.Log("Scale:" + (hit.transform.gameObject.transform.localScale));
				}
			}
		}
	}

}