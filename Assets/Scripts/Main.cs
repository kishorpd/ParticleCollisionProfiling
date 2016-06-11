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
		Splitters = new List<GameObject>();
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
				}
			}
		}
	}

	public void SpawnSplit(int CurrentDepth, Vector3 Center)
	{
		GameObject splittingPrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		splittingPrefab = splittingPrefab.gameObject;
		
		//grab prefab data before spawning
		Vector3 prefabScale = splittingPrefab.transform.localScale;
		Vector3 prefabRotation = splittingPrefab.transform.localEulerAngles;
		
		//set scale acacording to depth
		prefabScale.y /= (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
		splittingPrefab.transform.localScale = prefabScale;

		//instantiated here
		GameObject temp;
		Splitters.Add(temp = Instantiate(splittingPrefab, Center, Quaternion.identity) as GameObject);

		//change rotation of the prefab to spawn the same one horizontally
		prefabRotation.z += 90;
		splittingPrefab.transform.localEulerAngles = prefabRotation;
		GameObject temp1;
		Splitters.Add(temp1 = Instantiate(splittingPrefab, Center, transform.rotation) as GameObject);

		temp1.transform.eulerAngles = prefabRotation;

		//reset data of prefab!
		{ 
			//reset rotation
			prefabRotation.z -= 90;
			splittingPrefab.transform.localEulerAngles = prefabRotation;

			//reset scale
			prefabScale.y *= (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
			splittingPrefab.transform.localScale = prefabScale;
		}

		Debug.Log("Split at depth:" + CurrentDepth);
	}

	public void ClearQuadtree()
	{
		foreach (GameObject splitPrefab in Splitters)
			Destroy(splitPrefab);
		Splitters.Clear();

		_QuadTree.Clear();

		foreach (GameObject particlePrefab in Particles)
			Destroy(particlePrefab);
		Particles.Clear();

		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		_QuadTree.MainInstance = this;

		//for restarting this level.
		//Application.LoadLevel(0); 
	}
}