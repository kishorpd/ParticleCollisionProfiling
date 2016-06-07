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
	List<GameObject> Splitters;

	public Material lineColor;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;

	Ray myRay;      // initializing the ray
	RaycastHit hit; // initializing the raycasthit

	// Use this for initialization
	void Start()
	{
		Particles = new List<GameObject>();
		ParentPrefab = Instantiate(ParentPrefab, new Vector3(-30, 0, 9), Quaternion.identity) as GameObject;
		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		_QuadTree.MainInstance = this;
	}

	// Update is called once per frame
	void Update()
	{
		SpawnParticleOnClick();
	}


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
					ClearQuadtree();
					Debug.Log("Pos:" + (hit.transform.gameObject.transform.position));
					Debug.Log("Scale:" + (hit.transform.gameObject.transform.localScale));
				}
			}
		}
	}

	public void SpawnSplit(int CurrentDepth, Vector3 Center)
	{
		GameObject splittingPrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		splittingPrefab = splittingPrefab.gameObject;
		Vector3 prefabScale = splittingPrefab.transform.localScale;
		Debug.Log("The scale:"+(splittingPrefab.transform.localScale /= (2 * ((CurrentDepth == 0) ? (0.5f) : CurrentDepth))));
		GameObject temp =  Instantiate(splittingPrefab, Center, Quaternion.identity) as GameObject;
		splittingPrefab.transform.localScale = prefabScale;
		//Splitters.Add(temp as GameObject);
	}

	public void ClearQuadtree()
	{
		//foreach (GameObject splitPrefab in Splitters)
		//	Destroy(splitPrefab);
		//Splitters.Clear();
		foreach (GameObject particlePrefab in Particles)
			Destroy(particlePrefab);
		Particles.Clear();
		_QuadTree.Clear();

		Application.LoadLevel(0); 
	}
}