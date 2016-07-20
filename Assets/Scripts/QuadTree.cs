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


	//static public 

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


	public List<Vector4> ClampLines(List<Vector4> lines)
	{
		List<Vector4> tempLines = new List<Vector4> ();

		foreach (Vector4 line in lines)
		{
			tempLines.Add(line);
		}

		Vector4 tempLine = new Vector4();

		bool wasClamped = false;
		float tempWidth = Width / 2;

		//foreach (Vector4 line in tempLines)
		for (int line = 0; line < tempLines.Count; ++line )
		{
			tempLine = tempLines[line];
			wasClamped = false;
			//							(x,y+w)
			//                +---------------------+
			//                |                     |
			//                |                     |
			//                |                     |
			//  (x-w,y)       |         (x,y)       |  (x+w,y)
			//                |                     |
			//                |                     |
			//                |_____________________|
			//                           (x,y-w)

			//if
			//check for the UPPER BOUNDS for both points of each line
			//else
			//check for the LOWER BOUNDS for both points of each line
			//p(x,y) 

			if (tempLines[line].x > (Center.x + tempWidth))
			{
				tempLine.x = Center.x + tempWidth;
				wasClamped = true;
			}
			else
				//p(x,y) 
				if (tempLines[line].x < (Center.x - tempWidth))
				{
					tempLine.x = Center.x - tempWidth;
					wasClamped = true;
				}


			if (tempLines[line].y > (Center.y + tempWidth))
			{
				tempLine.y = Center.y + tempWidth;
				wasClamped = true;
			}
			else
				if (tempLines[line].y < (Center.y - tempWidth))
				{
					tempLine.y = Center.y - tempWidth;
					wasClamped = true;
				}


		//	//q(z,w)
		//	if (tempLines[line].x > (Center.x + tempWidth))
		//	{
		//		tempLine.z = Center.x + tempWidth;
		//		wasClamped = true;
		//	}
		//	else
		//		//q(z,w)
		//		if (tempLines[line].x < (Center.x - tempWidth))
		//		{
		//			tempLine.z = Center.x - tempWidth;
		//			wasClamped = true;
		//		}
		//
		//
		//	if (tempLines[line].y > (Center.y + tempWidth))
		//	{
		//		tempLine.w = Center.y + tempWidth;
		//		wasClamped = true;
		//	}
		//	else
		//		if (tempLines[line].y < (Center.y - tempWidth))
		//		{
		//			tempLine.w = Center.y - tempWidth;
		//			wasClamped = true;
		//		}
		//
		//
		//	if (wasClamped)
		//	{
		//		//Debug.Log("tempLine : " + tempLine);
		//		if(_ParentNode != null)
		//			Debug.Log("_ParentNode.InQuadrant(Center) : " + _ParentNode.InQuadrant(Center));
		//	}
			
			tempLines[line] = tempLine;
		}


		//now that all the lines are clamped i am worried what I am going to do in future with them
		//hoping for the best


		//now check whether if all lines are in the current quadrant
		int xCount = 0;
		int yCount = 0;

		//test one 

		//  |              | 
		//  |              | |------\   /
		//  |              | |_______\ /
		//  |              | 
		//  |              | 

		float x = tempLines[0].x;
		float y = tempLines[0].y;

		//if either of x and y are same  for all the lines then the frustum lies outside the square
		foreach (Vector4 line in tempLines)
		{
			if (line.x == x)
			{
				xCount++;
				//Debug.Log("line.x : " + line.x + " x : " + x);
			}

			if (line.y == y)
			{
				yCount++;
			}

		}

		for (int line = 0; line < tempLines.Count; ++line)
		{

			if (DrawFrustum.DrawClamping)
			{
				if (_CurrentDepth > 0)// && _ParentNode.InQuadrant(Center) == 0)//this overlays the border of the Frustum
				{
					DrawFrustum._SClampedLines.Add(new Vector3(tempLines[line].x, tempLines[line].y, 0));
					DrawFrustum._SClampedLines.Add(new Vector3(lines[line].x, lines[line].y, 0));
					DrawFrustum._SClampedLines.Add(new Vector3(tempLines[line].z, tempLines[line].w, 0));
					DrawFrustum._SClampedLines.Add(new Vector3(lines[line].z, lines[line].w, 0));
				}
			}
		}

		if ((xCount == 4)
			||
			(yCount == 4))
		{
			//Debug.Log("Frustum lies outside!!!! xCount : " + xCount);
			//Debug.Log("Frustum lies outside!!!! yCount : " + yCount);
			return null;
		}

		return tempLines;
	}


	bool IsWithinFrustrum()
	{
		if (_ChildNode == null)
			return false;

		List<Vector4> LinesOfFrustrum = SMainInstance.LinesOfFrustrum;

		float x = _ChildNode.transform.position.x;
		float y = _ChildNode.transform.position.y;


		//double AreaOfQuad = 0;
		float AreaSumOfTriangles = 0;

	for (int vertex = 0; vertex < (4/*QUAD_SIDES*/); vertex++)
	{
		AreaSumOfTriangles += Mathf.Abs(
			(0.5f)*(
				((x - LinesOfFrustrum[vertex].z)*
					(LinesOfFrustrum[vertex].y - y)) -
				((x - LinesOfFrustrum[vertex].x) *
					(LinesOfFrustrum[vertex].w - y))
				));
	}


	//AreaOfQuad = Mathf.Abs(
	//	(0.5f)*(
	//		(_SLinesOfFrustrum[0].x - _SLinesOfFrustrum[1].x) *
	//		(_SLinesOfFrustrum[0].w - _SLinesOfFrustrum[1].w) -
	//		(_SLinesOfFrustrum[0].z - _SLinesOfFrustrum[1].z) *
	//		(_SLinesOfFrustrum[0].y - _SLinesOfFrustrum[1].y)
	//		));
	//
	//Debug.Log("(int)AreaSumOfTriangles : " + (int)AreaSumOfTriangles + "(int)AreaOfQuad" + (int)AreaOfQuad + "v : " + ((int)AreaSumOfTriangles == (int)AreaOfQuad));

	return ((int)AreaSumOfTriangles == (int)SMainInstance.AreaOfFrustum);

	}

	public void CheckForIntersectionWithQuad(Vector3 center, List<Vector4> lines)
	{
		SMainInstance.SVisitedNodes++;
		//check the number of lines passed in, if there are no partitions then set the color of partition to red
		//turn on the particle contained if it lies in the trapezium

		//
		if (lines == null)
			return;

		if (TotalLeafNodes > 1)
		{ 
		//	Debug.Log(" lines.Count : " + lines.Count);
			switch (lines.Count)
			{
				case 4:
					{
						//the Quad lies within the tree space
						//	   |			|			|
						//	   |			|	___		|
						//	   |			|	\_/		|  here it lies in first quadrant completelyu
						//	   |			|			|  if the partition was present then further thing will get handled in the childNodes
						//	   __________________________
						//	   |			|			|
						//	   |			|			|
						//	   |			|			|
						//	   |			|			|

					
						//so now that we have four lines , let us check in how many quadrants it overlaps with

						for (int leaf = 0; leaf < 4; ++leaf)
						{
							if (_ChildNodes.ContainsKey(leaf))
							{
								//clamp for each qaudrant
								//check if a line is present in the quadrant by midpoint test
									if (_ChildNodes[leaf]._ChildNode != null)
									{ 
									//Debug.Log("leaf : " + leaf + " _CurrentDepth : " + _CurrentDepth);
										if (_ChildNodes[leaf].IsWithinFrustrum())
										{
											_ChildNodes[leaf]._ChildNode.gameObject.GetComponent<SpriteRenderer>().color = Color.cyan;
											SMainInstance.SRenderedParticles++;
											//continue;
										}
										else
										{ 
											_ChildNodes[leaf]._ChildNode.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
											SMainInstance.SVisitedParticles++;
										}
									}
								List<Vector4> tempLines = _ChildNodes[leaf].ClampLines(lines);

								if (tempLines != null)
								{
									_ChildNodes[leaf].CheckForIntersectionWithQuad(center, tempLines);
									if (_CurrentDepth > 0)
									{ 
											Transform bg = _SelfPartitionPrefab.transform.GetChild(2);
											bg.gameObject.SetActive(true);
									}
								}
							}
						}
	
					}
					break;


				case 3:
					{
					
					}
					break;


				case 2:
					{
					}
					break;


				case 1:
					{
					}
					break;


				case 0:
					{
					}
					break;


			}
		}
	}
}
