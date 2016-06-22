using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTree {

	public float Height { get; set; }
	public float Width { get; set; }
	public Vector3 Center { get; set; }
	public static Main SMainInstance { get; set; }

	public int TotalLeafNodes { get; set; }
	private int _CurrentDepth = 0;
	public static int SMaxDepth { get; set; }

	private QuadTree					_ParentNode;
	private GameObject					_SelfPartitionPrefab = null;
	private GameObject					_ChildNode = null;
	private Dictionary<int, QuadTree>	_ChildNodes = new Dictionary<int, QuadTree>();
	private bool _PartitionDrawn = false;

#if DEBUG
	private int __SelfQuadrant = 0;
#endif

	public QuadTree(Vector3 parentCenter, float parentHeight, float parentWidth)
	{
		_CurrentDepth = 0;
		Height = parentHeight;
		Width = parentWidth;
		Center = parentCenter;
		_ParentNode = null;

		TotalLeafNodes = 0;
	}


	private QuadTree(QuadTree parent)
	{
		_CurrentDepth = parent._CurrentDepth + 1;
		Height = parent.Height / 2;
		Width = parent.Width / 2;
		_ParentNode = parent;
	}



	public void SetCenter( Vector3 parentCenter)
	{
		Center = parentCenter;
	}

	public void SetConstructor(QuadTree parent, Vector3 parentCenter)
	{
		_CurrentDepth = parent._CurrentDepth + 1;
		Debug.Log("Depth:" + _CurrentDepth);

		Height = parent.Height * 0.5f;
		Width = parent.Width * 0.5f;
		_ParentNode = parent;
		Center = parentCenter;
	}
		
	public bool Insert(GameObject particleObject)
	{
		//TODO:fix the total nodes increment
		int Quadrant = InQuadrant(particleObject);
		if (TotalLeafNodes == 0)
		{
 			//insert the childnode
			_ChildNode = particleObject;
			++TotalLeafNodes;
			return true;
		}
		else if (TotalLeafNodes == 1)
		{
			if (_ChildNode.transform.position == particleObject.transform.position)
			{
				return false;
			}
		
			//draw split always and turn its visibility off
			DrawSplitSelf();

			//set the cached child{gameobject} to corresponding child QuadTree
			int tempQuadrant = InQuadrant(_ChildNode);
			_ChildNodes[tempQuadrant] = new QuadTree(this);
			_ChildNodes[tempQuadrant].SetCenter(GetCenterOfQuadrant(tempQuadrant));
			//not returning anything from this one because we are in middle of partitioning
			_ChildNodes[tempQuadrant].Insert(_ChildNode);

			//clear the cached childNode
			_ChildNode = null;

			Debug.Log("tempQuadrant = " + tempQuadrant + "Quadrant = " + Quadrant);
			//set the parameter in respective child QuadTree
			if (tempQuadrant != Quadrant)
			{ 
				_ChildNodes[Quadrant] = new QuadTree(this);
				_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
			}

			++TotalLeafNodes;
			return _ChildNodes[Quadrant].Insert(particleObject);
		
		}
		else
		{
			if (!_ChildNodes.ContainsKey(Quadrant))
			{
				_ChildNodes[Quadrant] = new QuadTree(this);
				_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
			}
			++TotalLeafNodes;
			return _ChildNodes[Quadrant].Insert(particleObject);
		}
	}

	public void Clear()
	{
		if(TotalLeafNodes != 0)
		{
			if (_ChildNode == null)
			{
				for (int leaf = 0; leaf < 4; ++leaf)
				{
					if (_ChildNodes.ContainsKey(leaf))
					{
						_ChildNodes[leaf].Clear();
					}
				}
			}
			else 
			{
				_ChildNode = null;
			}
		}
	}

	Vector3 GetCenterOfQuadrant(int quadrant)
	{
		Vector3 quadCenter = this.Center;

		if (quadrant == 0 || quadrant == 3)
		{
			quadCenter.x -= (Width/4);
			quadCenter.y = (quadrant == 3) ? (quadCenter.y - (Height / 4)) : quadCenter.y + (Height / 4);
		}
		else 
		{
			quadCenter.x += (Height / 4);
			quadCenter.y = (quadrant == 2) ? (quadCenter.y - (Width / 4)) : quadCenter.y + (Width / 4);
		}
		return	quadCenter;
	}

	void DrawSplitSelf()
	{
		if (_SelfPartitionPrefab == null)
		{
			_SelfPartitionPrefab = SMainInstance._SpawnQuadTreePartitioner(_CurrentDepth, this.Center);

		}
	}



	int InQuadrant(GameObject particleObject)
	{
		// ___ ___
		//| 0 | 1 |
		//|___|___|
		//| 3 | 2 |
		//|___|___|
		// The quadrants are numbered in this manner.

		return InQuadrant(particleObject.transform.position);

	}

	int InQuadrant(Vector3 position)
	{
		// ___ ___
		//| 0 | 1 |
		//|___|___|
		//| 3 | 2 |
		//|___|___|
		// The quadrants are numbered in this manner.

		return (position.y < Center.y) ?					//if (object is below X axis) {execute first parenthesis} else {second}
				((position.x < Center.x) ? 3 : 2) : 		//if (the object is to left of y axis and below X axis) return 3 else 2
					((position.x < Center.x) ? 0 : 1);		//if (the object is to left of y axis and above X axis) return 0 else 1

	}

	public GameObject ParticleUnderCursor(Vector3 CursorPosition)
	{

		if (TotalLeafNodes == 0)
		{
			//no particle under cursor 
			return null;
		}
		else
		{
			int Quadrant = InQuadrant(CursorPosition);
			return (TotalLeafNodes == 1) ? _ChildNode : 
					((_ChildNodes.ContainsKey(Quadrant)) ? 
						_ChildNodes[Quadrant].ParticleUnderCursor(CursorPosition) : null);
		}

	}

	bool ParticleUnderCursor(GameObject particle,Vector3 CursorPosition)
	{
		float Radius = particle.transform.localScale.y;
		return IsWithinRadius2D(CursorPosition, Radius);
	}

	bool IsWithinRadius2D(Vector3 position, float Radius)
	{
		return (Radius * Radius == ((position.x * position.x) + (position.y * position.y)));
	}

	bool IsWithinRadius3D(Vector3 position, float Radius)
	{
		return (Radius * Radius == ((position.x * position.x) + (position.y * position.y) + (position.z * position.z)));
	}

	GameObject FindParticleObject(GameObject ParticleObject)
	{

		if (TotalLeafNodes == 0)
		{
			return _ChildNode;
		}
		else
		{
			int Quadrant = InQuadrant(ParticleObject);
			return (TotalLeafNodes == 1) ? _ChildNode :
					((_ChildNodes.ContainsKey(Quadrant)) ?
						_ChildNodes[Quadrant].FindParticleObject(ParticleObject) : null);
		}
	}

	QuadTree FindParticleParent(GameObject ParticleObject)
	{
		if (TotalLeafNodes == 0)
		{
			return this;
		}
		else
		{
			int Quadrant = InQuadrant(ParticleObject);
			return (TotalLeafNodes == 1) ? this :
					((_ChildNodes.ContainsKey(Quadrant)) ?
						_ChildNodes[Quadrant].FindParticleParent(ParticleObject) : null);
		}
	}

	public bool RemoveParticle(GameObject ParticleObject)
	{
		QuadTree TempParent = FindParticleParent(ParticleObject);
		if (TempParent != null)
		{
			TempParent._ChildNode = null;
		//	if (TempParent._TotalLeafNodes == 1)
			{ 
				while (TempParent._ParentNode != null)
				{ 
					--TempParent.TotalLeafNodes;


					if (TempParent.TotalLeafNodes == 1)
					{
						Debug.Log("CALLED count me now!");
						SMainInstance.DestroyQuadTreeObject(TempParent._SelfPartitionPrefab);
						TempParent._SelfPartitionPrefab = null;
						
						//update childrens
						for (int leaf = 0; leaf < 4; ++leaf)
						{
							if (TempParent._ChildNodes.ContainsKey(leaf))
							{
								TempParent._ChildNode = TempParent._ChildNodes[leaf]._ChildNode;

								TempParent._ChildNodes[leaf]._ChildNode = null;
								TempParent._ChildNodes[leaf].Clear();
								TempParent._ChildNodes.Clear();
							}
						}
					}

					TempParent._PartitionDrawn = false;
					TempParent = TempParent._ParentNode;
				}
				if (TempParent._ParentNode == null)
					if (TempParent.TotalLeafNodes == 1)
					{
						--TempParent.TotalLeafNodes;
						SMainInstance.DestroyQuadTreeObject(TempParent._SelfPartitionPrefab);
						TempParent._SelfPartitionPrefab = null;
					}
			}
			//if (temp._TotalLeafNodes == 0)
			//{
			//	MainInstance.DestroyObject(temp._SelfPartitionPrefab);
			//	temp._SelfPartitionPrefab = null;
			//}
			return true;
		}

		//object not found
		return false;
	}

	public void ViewPartitions()
	{
		if (TotalLeafNodes > 0)
		{
			if (!_PartitionDrawn)
				 DrawSplitSelf();


			for (int leaf = 0; leaf < 4; ++leaf)
			{
				if (_ChildNodes.ContainsKey(leaf))
					_ChildNodes[leaf].ViewPartitions();
			}
			_PartitionDrawn = true;
		}
	}


	public void SetPartitionVisibility(bool Visibility)
	{

		if (TotalLeafNodes > 0)
		{
			for (int leaf = 0; leaf < 4; ++leaf)
			{
				if (_ChildNodes.ContainsKey(leaf))
					_ChildNodes[leaf].SetPartitionVisibility(Visibility);
			}

			if (_SelfPartitionPrefab != null)
			{
				_SelfPartitionPrefab.SetActive(Visibility);
				_PartitionDrawn = Visibility;
			}
		}
	}

	public void ClearPartitionDrawn()
	{
		if (TotalLeafNodes > 0)
		{
			if (_PartitionDrawn)
				_PartitionDrawn = false;

			for (int leaf = 0; leaf < 4; ++leaf)
			{
				if (_ChildNodes.ContainsKey(leaf))
					_ChildNodes[leaf].ClearPartitionDrawn();
			}
		}
	}
}
