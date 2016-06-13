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

	private GameObject _PartitionPrefab;

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
		_PartitionPrefab = (GameObject)Resources.Load("Prefabs/QuadSeperator");
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

	public GameObject _SpawnPartitioner(int CurrentDepth, Vector3 Center)
	{
		float scaleFactor = (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
		

		Transform linesVerticle = _PartitionPrefab.transform.GetChild(0);
		Transform linesHorizontal = _PartitionPrefab.transform.GetChild(1);



		//grab prefab data before spawning
		Vector3 prefabScale = linesVerticle.localScale;
		
		//set scale acacording to depth
		prefabScale.y /= scaleFactor;
		prefabScale.x = 5;
		//prefabScale.x /= scaleFactor / 2;
		linesVerticle.localScale = prefabScale;
		linesHorizontal.localScale = prefabScale;
		prefabScale.y *= scaleFactor;
		//prefabScale.x *= scaleFactor/2;
		Center.z = 10;
		//instantiated here
		GameObject temp;
		_Partitioners.Add(temp = Instantiate(_PartitionPrefab, Center, Quaternion.identity) as GameObject);

		//reset data of prefab!
		{ 
			//reset scale
			linesVerticle.localScale = prefabScale;
			linesHorizontal.localScale = prefabScale;
		}

		if (!DisplayPartitions)
		{
			temp.SetActive(false);
		}
		else
		{ 
			temp.SetActive(true);
		}

		Debug.Log("Split at depth:" + CurrentDepth);
		return temp;
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

	void _DeactivatePartitioning()
	{
	}

	public void TogglePartitionView()
	{
		if (!DisplayPartitions)
		{
			foreach (GameObject partitionPrefab in _Partitioners)
				partitionPrefab.SetActive(true);
			DisplayPartitions = true;
		}
		else
		{
			foreach (GameObject partitionPrefab in _Partitioners)
				partitionPrefab.SetActive(false);
			DisplayPartitions = false;
		}
	}

	public void TogglePaintMode()
	{
		_Paint = !_Paint;
	}
}