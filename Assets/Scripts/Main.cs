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

	public QuadTree AQuadTree;

	public GameObject ParentPrefab;
	public GameObject ParticlePrefab;

	public Material lineColor;
	public bool DisplayPartitions = false;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;
	
	
	private QuadTree _QuadTree;
	private List<GameObject> _Particles;
	private List<GameObject> _Partitioners;
	private GameObject _ObjectBeingDragged;
	public bool _Paint = false;

	Ray myRay;      // initializing the ray
	RaycastHit hit; // initializing the raycasthit

	enum CursorMode 
	{
		NORMAL,
		DRAGGING
	}

	CursorMode _CursorMode = CursorMode.NORMAL;

	// Use this for initialization
	void Start()
	{
		_Particles = new List<GameObject>();
		_Partitioners = new List<GameObject>();
		ParentPrefab = Instantiate(ParentPrefab, new Vector3(-30, 0, 9), Quaternion.identity) as GameObject;
		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		_QuadTree.MainInstance = this;
		_CursorMode = CursorMode.NORMAL;
	}

	// Update is called once per frame
	void Update()
	{
		SpawnParticleOnClick();
	}


	void SpawnParticleOnClick()
	{
			//if (Input.GetKey(KeyCode.Mouse0))
				switch (_CursorMode)
				{

					case CursorMode.NORMAL:
						{
							//case on cursor mode
							myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
							if (Physics.Raycast(myRay, out hit))
							{
								if ((_Paint) ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0))
								{
									//instantiate and add in the list
									GameObject ParticleObject;
									_Particles.Add(ParticleObject = Instantiate(ParticlePrefab, new Vector3(hit.point.x, hit.point.y, hit.point.z), Quaternion.identity) as GameObject);
									//insert in the QuadTree
									if (!_QuadTree.Insert(ParticleObject))
									{
										_Particles.Remove(ParticleObject);
										Destroy(ParticleObject);
									}
								}
								if (Input.GetMouseButtonUp(1))
								{
									//TODO: fix right click at appropriate time
									//ClearQuadtree();
									//_QuadTree.ParticleUnderCursor(hit.point);
									// 
									_CursorMode = CursorMode.DRAGGING;
								}
							}
							break;
						}

					case CursorMode.DRAGGING:
						{
							myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
							if (Physics.Raycast(myRay, out hit))
							{
								if (Input.GetMouseButtonUp(0))
								{
									TogglePartitionView();
								}
								//
								////case on cursor mode
								//if (Input.GetMouseButtonUp(0))
								//{
								//	_ObjectBeingDragged = _QuadTree.ParticleUnderCursor(hit.point);
								//}
							}

							if (Input.GetMouseButtonUp(1))
							{
								// 
								_CursorMode = CursorMode.NORMAL;
								
							}

							if (_ObjectBeingDragged != null)
							{ 
								Debug.Log("Removed?:" +
								_QuadTree.RemoveParticle(_ObjectBeingDragged)
									);
								_Particles.Remove(_ObjectBeingDragged);
							}
							
							break;
						}
				
				}//switch end
	}

	public void _SpawnPartitioner(int CurrentDepth, Vector3 Center)
	{
		GameObject partitionPrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		partitionPrefab = partitionPrefab.gameObject;
		
		//grab prefab data before spawning
		Vector3 prefabScale = partitionPrefab.transform.localScale;
		Vector3 prefabRotation = partitionPrefab.transform.localEulerAngles;
		
		//set scale acacording to depth
		prefabScale.y /= (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
		partitionPrefab.transform.localScale = prefabScale;

		//instantiated here
		GameObject temp;
		_Partitioners.Add(temp = Instantiate(partitionPrefab, Center, Quaternion.identity) as GameObject);

		//change rotation of the prefab to spawn the same one horizontally
		prefabRotation.z += 90;
		partitionPrefab.transform.localEulerAngles = prefabRotation;
		GameObject temp1;
		_Partitioners.Add(temp1 = Instantiate(partitionPrefab, Center, transform.rotation) as GameObject);

		temp1.transform.eulerAngles = prefabRotation;

		//reset data of prefab!
		{ 
			//reset rotation
			prefabRotation.z -= 90;
			partitionPrefab.transform.localEulerAngles = prefabRotation;

			//reset scale
			prefabScale.y *= (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
			partitionPrefab.transform.localScale = prefabScale;
		}

		Debug.Log("Split at depth:" + CurrentDepth);
	}

	public void ClearQuadtree()
	{
		_DeletePartitioning();
		_QuadTree.Clear();

		foreach (GameObject particlePrefab in _Particles)
			Destroy(particlePrefab);
		_Particles.Clear();

		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		_QuadTree.MainInstance = this;

		//for restarting this level.
		//Application.LoadLevel(0); 
	}

	void _DeletePartitioning()
	{ 
		foreach (GameObject partitionPrefab in _Partitioners)
			Destroy(partitionPrefab);
		_Partitioners.Clear();
	}

	public void TogglePartitionView()
	{
		if (!DisplayPartitions)
		{
			_QuadTree.ViewPartitions();
			DisplayPartitions = true;
		}
		else
		{
			_DeletePartitioning();
			_QuadTree.ClearPartitionDrawn();
			DisplayPartitions = false;
		}
	}

	public void TogglePaintMode()
	{
		_Paint = !_Paint;
	}
}