using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Main : MonoBehaviour
{

	public GameObject bot;
	public float AreaOfFrustum = 0;
	public GameObject botFrustumCenter;

	List<GameObject> Vertices;

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


	public QuadTree RootQuadTree;
	public KDTree RootKDTree;
	private List<GameObject> _Particles;
	private List<GameObject> _QuadTreePartitioners;
	private List<GameObject> _KDTreePartitioners;
	private GameObject _ObjectBeingDragged;
	private bool _Paint = false;
	private bool _Grabbed = false;
	private GameObject _PartitionQuadTreePrefab;
	private GameObject _PartitionKDTreePrefab;
	public 	GameObject tempRef = null;
	public List<Vector4> LinesOfFrustrum;
	public int SVisitedParticles = 0;
	public int SVisitedNodes = 0;
	public int SRenderedParticles = 0;

	bool DisplayLinesOfQuadTree = false;

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
		RootQuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		RootKDTree = new KDTree(ParentPrefab, 172f, 172f);
		QuadTree.SMainInstance = this;
		KDTree.SMainInstance = this;
		_CursorMode = CursorMode.NORMAL;
		_PartitionQuadTreePrefab = (GameObject)Resources.Load("Prefabs/QuadSeperator");
		_PartitionKDTreePrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		QuadTreeGrid.SetActive(false);
		KDTreeGrid.SetActive(false);
		Vertices = bot.GetComponent<ControlBot>().Vertices;
		
		CreateQuad();
		DisplayLinesOfPartitions();
		DisplayLinesOfPartitions();
	}

	// Update is called once per frame
	void Update()
	{
		SVisitedParticles = 0;
		SRenderedParticles = 0;
		SVisitedNodes = 0;

		UpdateQuadTree();
		SpawnParticleOnClick();
		DisplayToBeRenderedRegions();
		TextBox.text = "Total Leaf Nodes : " + RootQuadTree.TotalLeafNodes +
						"\n Total Partitions : " + _QuadTreePartitioners.Count +
						"\n Visited Particles : " + (SVisitedParticles + SRenderedParticles) +
						"\n Visited Nodes : " + SVisitedNodes +
						"\n Rendered Particles : " + SRenderedParticles;

	}

	void UpdateQuadTree()
	{
		if (_Particles.Count > 1)
		{ 
		//	foreach (GameObject particleObj in _Particles)
		//	{
			//		RootQuadTree.Remove(particleObj);
		//	}


			RootQuadTree.Clear();//.Remove(_Particles[0]);
			//RootQuadTree.Remove(_Particles[1]);
			_QuadTreePartitioners.Clear();
			foreach (GameObject particleObj in _Particles)
			{
				particleObj.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
				
				AreaOfFrustum = FrustumArea();

				//particleObj.gameObject.GetComponent
				//Debug.Log("particleObj.transform.localPosition: " + particleObj.transform.localPosition);
				RootQuadTree.Insert(particleObj); 
			}
		}
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
									if (!RootQuadTree.Insert(ParticleObject))
									{
										_Particles.Remove(ParticleObject);
										Destroy(ParticleObject);
									}
									//else
									//	RootKDTree.Insert(ParticleObject);
							
									////insert in the QuadTree
									//if (!RootKDTree.Insert(ParticleObject))
									//{
									//	_Particles.Remove(ParticleObject);
									//	Destroy(ParticleObject);
									//}
								}
								if (Input.GetMouseButtonUp(1))
								{
									//TODO: fix right click at appropriate time
									//ClearQuadtree();
									//_QuadTree.ParticleUnderCursor(hit.point);
									// 
									Debug.Log("CHANGEDS!!!!");
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
										//RootQuadTree.CollidesWith(_ObjectBeingDragged);

										RootQuadTree.Remove(_ObjectBeingDragged);
										Vector3 TempPosition = Input.mousePosition;
										TempPosition.z = 9.2f;
										_ObjectBeingDragged.transform.position = Camera.main.ScreenToWorldPoint(TempPosition);
										RootQuadTree.Insert(_ObjectBeingDragged);
									}
									else
								if (Input.GetMouseButtonDown(0))
								{
										_ObjectBeingDragged = RootQuadTree.ParticleUnderCursor(hit.point);
										//Debug.Log("R _ObjectBeingDragged.transform.localScale: " + _ObjectBeingDragged.GetComponent<SpriteRenderer>().bounds.extents.x);
										//Debug.Log("_ObjectBeingDragged.transform.localPosition: " + _ObjectBeingDragged.transform.localPosition);
										//Debug.Log("Mouse Position: " + hit.point);
										//Debug.Log("Is within: " + IsWithinCircle2D(_ObjectBeingDragged.transform.localPosition, hit.point, _ObjectBeingDragged.GetComponent<SpriteRenderer>().bounds.extents.x));
										Debug.Log("FOUND: " + _ObjectBeingDragged);
										if (_ObjectBeingDragged != null)
											_Grabbed = true;
								}
								
								//case on cursor mode
								if (Input.GetMouseButtonUp(0))
								{
									//		_QuadTree.RemoveParticle(_ObjectBeingDragged);
									//	_Particles.Remove(_ObjectBeingDragged);
									//	Destroy(_ObjectBeingDragged);
									Debug.Log("<<<_______________Total leaf nodes before: " + RootQuadTree.TotalLeafNodes);
									if (_ObjectBeingDragged != null)
									{
									//	RootQuadTree.CollidesWith(_ObjectBeingDragged);
										//RootQuadTree.Remove(_ObjectBeingDragged);
										//{ 
										//	_Particles.Remove(_ObjectBeingDragged);
										//	Destroy(_ObjectBeingDragged);
										//}
									}
									//_ObjectBeingDragged = null;
									_Grabbed = false;
									Debug.Log("<<<_______________Total leaf nodes After: " + RootQuadTree.TotalLeafNodes);
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

	public GameObject _SpawnQuadTreePartitioner(int currentDepth, Vector3 center)
	{
		float scaleFactor = (((float)2) * (Mathf.Pow(2, currentDepth - 1)));
		

		Transform linesVerticle = _PartitionQuadTreePrefab.transform.GetChild(0);
		Transform linesHorizontal = _PartitionQuadTreePrefab.transform.GetChild(1);
		Transform bg = _PartitionQuadTreePrefab.transform.GetChild(2);



		bg.gameObject.SetActive(false);

		//grab prefab data before spawning
		Vector3 prefabScale = linesVerticle.localScale;
		Vector3 bgScale = bg.localScale;
		
		//set scale acacording to depth
		prefabScale.y /= scaleFactor;
		bgScale.x /= scaleFactor;
		bgScale.y /= scaleFactor;
		prefabScale.x = 5;
		//bgScale.x = 5;
		//prefabScale.x /= scaleFactor / 2;
		bg.localScale = bgScale;
		linesVerticle.localScale = prefabScale;
		linesHorizontal.localScale = prefabScale;
		prefabScale.y *= scaleFactor;
		bgScale.y *= scaleFactor;
		bgScale.x *= scaleFactor;
		//prefabScale.x *= scaleFactor/2;
		center.z = 10;
		//instantiated here
		GameObject temp;
		_QuadTreePartitioners.Add(temp = Instantiate(_PartitionQuadTreePrefab, center, Quaternion.identity) as GameObject);

		//reset data of prefab!
		{ 
			//reset scale
			linesVerticle.localScale = prefabScale;
			linesHorizontal.localScale = prefabScale;
			bg.localScale = bgScale;
		}

		temp.transform.SetParent(QuadTreeGrid.transform);

	//	Debug.Log("Split at depth:" + CurrentDepth);
		return temp;
	}

	public GameObject _SpawnKDTreePartitioner(float length,bool toRotate, Vector3 center)
	{ 
		GameObject temp;

		Debug.Log("length: " + length);
		//set the scale according to the length

		Vector3 scale = _PartitionKDTreePrefab.transform.localScale;
		//Quaternion rotation = _PartitionKDTreePrefab.transform.rotation;//.localRotation;
		Quaternion rotation = Quaternion.Euler(0, 0, 90);
		scale.y = scale.y * (length / (BoundaryScale));

		_PartitionKDTreePrefab.transform.localScale = scale;

		_KDTreePartitioners.Add(temp = Instantiate(_PartitionKDTreePrefab, center, (toRotate)? rotation: Quaternion.identity) as GameObject);
		temp.transform.SetParent(KDTreeGrid.transform);

		//rescale the prefab to original size
		scale.y = scale.y / (length / (BoundaryScale));

		_PartitionKDTreePrefab.transform.localScale = scale;
	
		return temp;
	}

	public void ClearAndInsertQuadtree()
	{
		_Particles.Remove(tempRef);
		ClearQuadtree();
		_Particles.Add(tempRef);
		RootQuadTree.Insert(tempRef);
		RootKDTree.Insert(tempRef);
	}
	
		
	public void ClearQuadtree()
	{
		_DeletePartitioning();
		RootQuadTree.Clear();
		RootKDTree.Clear();

		foreach (GameObject particlePrefab in _Particles)
			Destroy(particlePrefab);
		_Particles.Clear();

		Vector3 scaleOfParentQuad = ParentPrefab.transform.localScale;
		RootQuadTree = new QuadTree(ParentPrefab.transform.position, 172f, 172f);
		QuadTree.SMainInstance = this;
		KDTree.SMainInstance = this;

		//for restarting this level.
		//Application.LoadLevel(0); 
	}

	void _DeletePartitioning()
	{
		//foreach (GameObject partitionPrefab in _QuadTreePartitioners)
		//	Destroy(partitionPrefab);
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

	public void DestroyQuadTreeObject(GameObject objectToBeDestroyed)
	{
		//_Partitioners.Remove(ObjectToBeDestroyed);
		Destroy(objectToBeDestroyed);
	}

	bool IsWithinCircle2D(Vector3 centerOfCircle, Vector3 point, float radiusOfCircle)
	{

		return (((centerOfCircle.x - point.x) * (centerOfCircle.x - point.x) + ((centerOfCircle.y - point.y) * (centerOfCircle.y - point.y))) < radiusOfCircle * radiusOfCircle);
	}


	void DisplayToBeRenderedRegions()
	{
		//RootQuadTree.CheckForIntersectionWithQuad(botFrustumCenter.transform.position, CreateQuad());
		RootQuadTree.CheckForIntersectionWithQuad(botFrustumCenter.transform.position, RootQuadTree.ClampLines(CreateQuad()));
	}


	public List<Vector4> CreateQuad()
	{
		List<Vector4> lines = new List<Vector4>();

		for (int index = 0; index < 4; ++index)
			lines.Add(CreateLine(index));

		LinesOfFrustrum = lines;// SMainInstance.CreateQuad();

		return lines;
	}

	Vector4 CreateLine(int index)
	{ 
		Vector4 nextLine = new Vector4();
		nextLine.x = Vertices[index].transform.position.x;
		nextLine.y = Vertices[index].transform.position.y;
		nextLine.z = Vertices[index + 1].transform.position.x;
		nextLine.w = Vertices[index + 1].transform.position.y;
		return nextLine;
	}

	float FrustumArea()
	{


		float x = botFrustumCenter.transform.position.x;
		float y = botFrustumCenter.transform.position.y;


		float AreaSumOfTriangles = 0;

		for (int vertex = 0; vertex < (4/*QUAD_SIDES*/); vertex++)
		{
			AreaSumOfTriangles += Mathf.Abs(
				(0.5f) * (
					((x - LinesOfFrustrum[vertex].z) *
						(LinesOfFrustrum[vertex].y - y)) -
					((x - LinesOfFrustrum[vertex].x) *
						(LinesOfFrustrum[vertex].w - y))
					));
		}

		return AreaSumOfTriangles;
	}

	public void DisplayLinesOfPartitions()
	{
		DisplayLinesOfQuadTree = !DisplayLinesOfQuadTree;
		_PartitionQuadTreePrefab.transform.GetChild(0).gameObject.SetActive(DisplayLinesOfQuadTree);
		_PartitionQuadTreePrefab.transform.GetChild(1).gameObject.SetActive(DisplayLinesOfQuadTree);
	}

}