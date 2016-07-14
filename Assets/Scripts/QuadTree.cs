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
	static private GameObject			_SPreviousChildNode = null;
	static private QuadTree				_SPreviousQuadTree = null;
	private Dictionary<int, QuadTree>	_ChildNodes = new Dictionary<int, QuadTree>();
	private bool						_PartitionDrawn = false;

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

			if (_ChildNode != null)
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

				//set the parameter in respective child QuadTree
				if (tempQuadrant != Quadrant)
				{
					_ChildNodes[Quadrant] = new QuadTree(this);
					_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
				}

			}
			else
			{
 				//find the quadrant for particle insert in the quadrant
				//int tempQuadrant = InQuadrant(particleObject);
				if (_ChildNodes.ContainsKey(Quadrant))
					throw new System.InvalidOperationException("uh ohhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhh");
				//if (tempQuadrant != Quadrant)
				{ 
					_ChildNodes[Quadrant] = new QuadTree(this);
					_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
				}
			
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
						SMainInstance.DestroyQuadTreeObject(_SelfPartitionPrefab);
					}
				}
			}
			else 
			{
				_ChildNode = null;
			}
			TotalLeafNodes = 0;
			_CurrentDepth = 0;
			SMaxDepth = 0;

			_ParentNode = null;
			_SelfPartitionPrefab = null;
			_ChildNode = null;
			_SPreviousChildNode = null;
			_ChildNodes = new Dictionary<int, QuadTree>();
			_PartitionDrawn = false;
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
			return (TotalLeafNodes == 1) ?( (ParticleUnderCursor(_ChildNode, CursorPosition))? _ChildNode : null): 
					((_ChildNodes.ContainsKey(Quadrant)) ? 
						_ChildNodes[Quadrant].ParticleUnderCursor(CursorPosition) : null);
		}

	}

	bool ParticleUnderCursor(GameObject particle,Vector3 CursorPosition)
	{
		float Radius = particle.GetComponent<SpriteRenderer>().bounds.extents.x;
		return IsWithinCircle2D(particle.transform.localPosition, CursorPosition, Radius);
	}

	bool IsWithinCircle2D(Vector3 centerOfCircle, Vector3 point, float radiusOfCircle)
	{

		return (((centerOfCircle.x - point.x) * (centerOfCircle.x - point.x) + ((centerOfCircle.y - point.y) * (centerOfCircle.y - point.y))) < radiusOfCircle * radiusOfCircle);
	}

	bool IsWithinCircle3D(Vector3 centerOfCircle, Vector3 point, float radiusOfCircle)
	{

		return (((centerOfCircle.x - point.x) * (centerOfCircle.x - point.x) + ((centerOfCircle.y - point.y) * (centerOfCircle.y - point.y)) + ((centerOfCircle.z - point.z) * (centerOfCircle.z - point.z))) < radiusOfCircle * radiusOfCircle * radiusOfCircle);
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
			Debug.Log("FindParticleParent(obj) is in Quadrant:" + Quadrant);
			return (TotalLeafNodes == 1) ? this :
					((_ChildNodes.ContainsKey(Quadrant)) ?
						_ChildNodes[Quadrant].FindParticleParent(ParticleObject) : null);
		}
	}


	public bool RemoveFromRoot(GameObject ParticleObject)
	{

		//for root node
		if (TotalLeafNodes == 1)
		{
			SMainInstance.ClearQuadtree();
			return true;
		}

			//remove this child 
			//QuadTree deleteChildOfParent = TempParent._ParentNode;
			_ChildNode = null;
			_ChildNodes.Remove(InQuadrant(ParticleObject));
			--TotalLeafNodes;

			//handle last two childrens condition
			if (TotalLeafNodes == 1)
			{
				ClearPartitionDrawn();
				for (int leaf = 0; leaf < 4; ++leaf)
				{
					if (_ChildNodes.ContainsKey(leaf))
					{
						_ChildNode = _ChildNodes[leaf]._ChildNode;
						_ChildNodes.Clear();
						return true;
					}
				}
			}

		return true;


	}
	
		
	public bool Remove(GameObject particleObject)
	{
		if (TotalLeafNodes == 0)
			return false;

		//handle the root node
		QuadTree TempParent = FindParticleParent(particleObject);
		if (TempParent == null)
			return false;

		//handle the root node
		if ((TempParent._CurrentDepth == 0) || (TempParent._ParentNode._CurrentDepth == 0))
		{
			RemoveFromRoot(particleObject);
			return true;
		}

		//now there will be only one root node condition to handle after this
		QuadTree TempParent1 = TempParent;
		QuadTree parentOfChildToBeDelete = TempParent._ParentNode;
		int FoundInQuadrant = -1;
		GameObject TempParticle = null;

		//
		{
			//store the quadrant in which the particle is
			FoundInQuadrant = TempParent._ParentNode.InQuadrant(particleObject);
			//remove this child 
			TempParent._ChildNode = null;
			parentOfChildToBeDelete._ChildNodes.Remove(FoundInQuadrant);


			// decrement all the TotalLeafNodes up in tree
			while (TempParent1._ParentNode != null)
			{
				Debug.Log("--TempParent1.TotalLeafNodes:" + --TempParent1.TotalLeafNodes);
				TempParent1 = TempParent1._ParentNode;
			}
			//decrement the TotalLeafNodes of parent
			--TotalLeafNodes;



			TempParent = parentOfChildToBeDelete;
			//if (TempParent._ParentNode != null)
			{ 
				TempParent1 = TempParent;//._ParentNode;

				//handle the condition for second child
				//store the second particle in tempParticle
				if (TempParent1.TotalLeafNodes == 1)
				{
					for (int leaf = 0; leaf < 4; ++leaf)
					{
						if (leaf == FoundInQuadrant) continue;

						if (TempParent1._ChildNodes.ContainsKey(leaf))
						{
							TempParticle = TempParent1._ChildNodes[leaf]._ChildNode;
							if (TempParticle != null)
							{
								break;
							}
						}
					}


					//TempParticle should not be null
					if (TempParticle != null)
					{ 
						while ((TempParent1.TotalLeafNodes == 1) )
						{
							Debug.Log("^ depth: " + _CurrentDepth);
							//handle the root condition
							if (TempParent1._ParentNode == null)
							{
								SMainInstance.tempRef = TempParticle;
								SMainInstance.ClearAndInsertQuadtree();
								return true;
							}
							TempParent1 = TempParent1._ParentNode;
						}

						//now tempParent has more than one leaf nodes
						//one in the same quadrant as tempParticle and other in remaining quadrant
						FoundInQuadrant = TempParent1.InQuadrant(TempParticle);
						Debug.Log("FoundInQuadrant:" + FoundInQuadrant);
						Debug.Log("TotalLeafNodes:" + TempParent1.TotalLeafNodes);
						TempParent1._ChildNodes[FoundInQuadrant].Clear();
						TempParent1._ChildNodes.Remove(FoundInQuadrant);


						//if there are only two children in tempParent1 then we will again have to fix that for insertion
						if (TempParent1.TotalLeafNodes == 2)
						{
							Debug.Assert((TempParent1._ChildNode == null), "CHILD FOR INSERTION NOT NULLL!!!!");
							for (int leaf = 0; leaf < 4; ++leaf)
							{
								if (leaf == FoundInQuadrant) continue;

								if (TempParent1._ChildNodes.ContainsKey(leaf))
								{
									TempParent1._ChildNode = TempParent1._ChildNodes[leaf]._ChildNode;
									if (TempParent1._ChildNode != null)
									{
										Debug.Log("1234 THIS WAS A MAJOR BUG1!");
										TempParent1._ChildNodes.Clear();
										break;
									}
								}
							}
						}
						//TempParent = TempParent1;

						//while (TempParent._ParentNode != TempParent1)
						//{
						//	Debug.Log("--TempParent1.TotalLeafNodes:" + TempParent.TotalLeafNodes--);
						//	TempParent = TempParent._ParentNode;
						//}

						--TempParent1.TotalLeafNodes;
						TempParent1.Insert(TempParticle);

						return true;
					}
					else
						throw new System.InvalidOperationException("CHILD FOUND NULLLLL!!!!!!");
				}
			}
		}

		return false;
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


	public void CollidesWith(GameObject particle)
	{
		if (_SPreviousQuadTree != null)
		{
 			//clear the previously highlighted 
		Debug.Log("Started to delete highlight");
		//_SPreviousQuadTree = FindParticleParent(particle);
		HighlightChildren(_SPreviousQuadTree, false);
		}


		_SPreviousQuadTree = FindParticleParent(particle);
		HighlightChildren(_SPreviousQuadTree, true);
	}

	void HighlightChildren(QuadTree tempQuadTree, bool toHighlight)
	{
		//QuadTree tempQuadTree = FindParticleParent(particle);
		tempQuadTree = tempQuadTree._ParentNode;
		for (int leaf = 0; leaf < 4; ++leaf)
		{
			if (tempQuadTree._ChildNodes.ContainsKey(leaf))
			{
				if (tempQuadTree._ChildNodes[leaf]._ChildNode != null)
				{
					if (toHighlight)
						tempQuadTree._ChildNodes[leaf]._ChildNode.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
					else
					{
						tempQuadTree._ChildNodes[leaf]._ChildNode.gameObject.GetComponent<SpriteRenderer>().color = Color.blue;
						_SPreviousQuadTree = null;
					}
				}
			}
		}
	}

	public void ClearPartitionDrawn()
	{
			SMainInstance.DestroyQuadTreeObject(_SelfPartitionPrefab);
//		Debug.Log("DEL8888888888888888888888888888888888888888888888888888888");
//		if (TotalLeafNodes > 0)
//		{
//			if (_PartitionDrawn)
//				_PartitionDrawn = false;
//			for (int leaf = 0; leaf < 4; ++leaf)
//			{
//				if (_ChildNodes.ContainsKey(leaf))
//					_ChildNodes[leaf].ClearPartitionDrawn();
//			}
//		}
	}
}
