using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
	
	public QuadTree AQuadTree;

	public GameObject ParentPrefab;
	public GameObject QuadTreeGrid;
	public GameObject KDTreeGrid;
	public GameObject ParticlePrefab;

	public Material lineColor;
	bool DisplayQuadPartitions = false;
	bool DisplayKDPartitions = false;
	public float BoundaryScale = 172.0f;

	public Color c1 = Color.yellow;
	public Color c2 = Color.red;
	public int lengthOfLineRenderer = 2;
	public Text TextBox;


	private QuadTree _QuadTree;
	private KDTree _KDTree;
	private List<GameObject> _Particles;
	private List<GameObject> _QuadTreePartitioners;
	private List<GameObject> _KDTreePartitioners;
	private GameObject _ObjectBeingDragged;
	private bool _Paint = false;
	private bool _Grabbed = false;
	private GameObject _PartitionQuadTreePrefab;
	private GameObject _PartitionKDTreePrefab;

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
		_QuadTreePartitioners = new List<GameObject>();
		_KDTreePartitioners = new List<GameObject>();
		ParentPrefab = Instantiate(ParentPrefab, new Vector3(-30, 0, 9), Quaternion.identity) as GameObject;
		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		_KDTree = new KDTree(ParentPrefab, 172f, 172f);
		QuadTree.SMainInstance = this;
		KDTree.SMainInstance = this;
		_CursorMode = CursorMode.NORMAL;
		_PartitionQuadTreePrefab = (GameObject)Resources.Load("Prefabs/QuadSeperator");
		_PartitionKDTreePrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		QuadTreeGrid.SetActive(false);
		KDTreeGrid.SetActive(false);

	}

	// Update is called once per frame
	void Update()
	{
		SpawnParticleOnClick();
		//TextBox.text = "Total Leaf Nodes:" + _QuadTree.TotalLeafNodes + 
		//				"\n Total Partitions" + _Partitioners.Count;

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
							
									//insert in the QuadTree
									if (!_KDTree.Insert(ParticleObject))
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
									if (_Grabbed)
									{
										//Vector3 TempPosition = Input.mousePosition;
										//TempPosition.z = 9.2f;
										//_ObjectBeingDragged.transform.position = Camera.main.ScreenToWorldPoint(TempPosition);
										//_QuadTree.Insert(_ObjectBeingDragged);
									}
								if (Input.GetMouseButtonDown(0))
								{
										_ObjectBeingDragged = _QuadTree.ParticleUnderCursor(hit.point);
										_Grabbed = true;
								}
								
								//case on cursor mode
								if (Input.GetMouseButtonUp(0))
								{
										_QuadTree.RemoveParticle(_ObjectBeingDragged);
									_Particles.Remove(_ObjectBeingDragged);
									Destroy(_ObjectBeingDragged);
									_Grabbed = false;
								}
								
							}

							if (Input.GetMouseButtonUp(1))
							{
								// 
								_CursorMode = CursorMode.NORMAL;
								
							}
					
							break;
						}
				
				}//switch end
	}

	public GameObject _SpawnQuadTreePartitioner(int CurrentDepth, Vector3 Center)
	{
		float scaleFactor = (((float)2) * (Mathf.Pow(2, CurrentDepth - 1)));
		

		Transform linesVerticle = _PartitionQuadTreePrefab.transform.GetChild(0);
		Transform linesHorizontal = _PartitionQuadTreePrefab.transform.GetChild(1);



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
		_QuadTreePartitioners.Add(temp = Instantiate(_PartitionQuadTreePrefab, Center, Quaternion.identity) as GameObject);

		//reset data of prefab!
		{ 
			//reset scale
			linesVerticle.localScale = prefabScale;
			linesHorizontal.localScale = prefabScale;
		}

		temp.transform.SetParent(QuadTreeGrid.transform);

	//	Debug.Log("Split at depth:" + CurrentDepth);
		return temp;
	}

	public GameObject _SpawnKDTreePartitioner(float length,bool toRotate, Vector3 Center)
	{ 
		GameObject temp;

		Debug.Log("length: " + length);
		//set the scale according to the length

		Vector3 scale = _PartitionKDTreePrefab.transform.localScale;
		//Quaternion rotation = _PartitionKDTreePrefab.transform.rotation;//.localRotation;
		Quaternion rotation = Quaternion.Euler(0, 0, 90);
		scale.y = scale.y * (length / (BoundaryScale));

		_PartitionKDTreePrefab.transform.localScale = scale;

		_KDTreePartitioners.Add(temp = Instantiate(_PartitionKDTreePrefab, Center, (toRotate)? rotation: Quaternion.identity) as GameObject);
		temp.transform.SetParent(KDTreeGrid.transform);

		//rescale the prefab to original size
		scale.y = scale.y / (length / (BoundaryScale));

		_PartitionKDTreePrefab.transform.localScale = scale;
	
		return temp;
	}

	public void ClearQuadtree()
	{
		_DeletePartitioning();
		_QuadTree.Clear();
		_KDTree.Clear();

		foreach (GameObject particlePrefab in _Particles)
			Destroy(particlePrefab);
		_Particles.Clear();

		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		_QuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		QuadTree.SMainInstance = this;
		KDTree.SMainInstance = this;

		//for restarting this level.
		//Application.LoadLevel(0); 
	}

	void _DeletePartitioning()
	{
		foreach (GameObject partitionPrefab in _QuadTreePartitioners)
			Destroy(partitionPrefab);
		_QuadTreePartitioners.Clear();
	}

	void _DeactivatePartitioning()
	{
		foreach (GameObject partitionPrefab in _QuadTreePartitioners)
			partitionPrefab.SetActive(true);
	}

	public void TogglePartitionView()
	{
		if (!DisplayQuadPartitions)
		{
			DisplayQuadPartitions = true;
			QuadTreeGrid.SetActive(true);
			//_QuadTree.SetPartitionVisibility(DisplayPartitions);
		}
		else
		{
			DisplayQuadPartitions = false;
			QuadTreeGrid.SetActive(false);
			//_QuadTree.SetPartitionVisibility(DisplayPartitions);
		}
	}

	public void ToggleKDTreeDisplay()
	{
		if (!DisplayKDPartitions)
		{
			DisplayKDPartitions = true;
			KDTreeGrid.SetActive(true);
		}
		else
		{
			DisplayKDPartitions = false;
			KDTreeGrid.SetActive(false);
		}
	}

	public void TogglePaintMode()
	{
		_Paint = !_Paint;
	}

	public void DestroyQuadTreeObject(GameObject ObjectToBeDestroyed)
	{
		//_Partitioners.Remove(ObjectToBeDestroyed);
		Destroy(ObjectToBeDestroyed);
	}
}