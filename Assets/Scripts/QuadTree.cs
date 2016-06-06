using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTree : MonoBehaviour {

	public float Height { get; set; }
	public float Width { get; set; }
	public Vector3 Center { get; set; }

	private int _TotalLeafNodes { get; set; }
	private int _CurrentDepth = 0;//	{ get; set; }
	public static int _MaxDepth { get; set; }

	private List<GameObject>	_Particles;
	private QuadTree			_ParentNode;
	private QuadTree[]			_ChildNodes;
	private Rect a;

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
		_Particles = new List<GameObject>();
	}


	private QuadTree(QuadTree parent)
	{
		_CurrentDepth = parent._CurrentDepth + 1;
		Debug.Log("Depth:" + _CurrentDepth);

		Height = parent.Height / 2;
		Width = parent.Width / 2;
		_ParentNode = parent;
		_Particles = new List<GameObject>();
	}



	public void SetCenter(Vector3 parentCenter)
	{ 
		Center = parentCenter;
	}
		
	public void Insert(GameObject particleObject)
	{

		if (_TotalLeafNodes == 2)//insert first element ofchild;
			DrawSplitSelf();

		if (_TotalLeafNodes > 3)//insert first element ofchild;
		{
			DrawSplitSelf();
			//insert in the specific QuadTree corresponding to the Quadrant
			//Insert(particleObject);
			if (_Particles.Count != 0) //distribute the list
			{
				int Quadrant;
				//clear list of gameObjects and insert in the quadtrees[0],[1],[2],[3];
				_ChildNodes = new QuadTree[4];// (this);
				for (Quadrant = 0; Quadrant < 4;++Quadrant )
				{
					_ChildNodes[Quadrant] = new QuadTree(this);
				}

				for (int leaf = 0; leaf < 4; ++leaf)
				{
					Quadrant = InQuadrant(_Particles[leaf]);
					Debug.Log("Leaf:" + (leaf) + "Quadrant:" + (Quadrant));
					_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(leaf));
					_ChildNodes[Quadrant].Insert(_Particles[leaf]);
					
#if DEBUG
					_ChildNodes[Quadrant].__SelfQuadrant = Quadrant;
#endif
				}
				_Particles.Clear();
			}
				Debug.Log("This must be the fifth element everyone is talking about i guess!");
				_ChildNodes[InQuadrant(particleObject)].Insert(particleObject);
		}
		else 
		{
			_Particles.Add(particleObject);
		}

		++_TotalLeafNodes;
	}

	public void Insert(GameObject particleObject, int quadrant)
	{
		if (_TotalLeafNodes == 0)
		{
			//ChildNodes[Quadrant] = new QuadTree(.......);
			//_ChildNodes[Quadrant] = new QuadTree(GetCenterOfQuadrant(quadrant), Height/2, Width/2, particleObject, parent);
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
		Debug.Log("This.center:" + (this.Center) + "quadCenter:" + (quadCenter));
		return	quadCenter;
	}

	void DrawSplitSelf()
	{

#if DEBUG
		Debug.Log("DrawSplitSelf---->This.__SelfQuadrant:" + (this.__SelfQuadrant));
		Debug.Log("DrawSplitSelf---->This._CurrentDepth:" + (this._CurrentDepth));
#endif
		Debug.Log("DrawSplitSelf---->This.center:" + (this.Center));
		Debug.Log("DrawSplitSelf---->This._TotalLeafNodes:" + (this._TotalLeafNodes));
		GameObject testPrefab = (GameObject)Resources.Load("Prefabs/Seperator");
		testPrefab = testPrefab.gameObject;
		Vector3 prefabScale = testPrefab.transform.localScale;
		testPrefab.transform.localScale /= (2 * ((_CurrentDepth == 0)? (0.5f):_CurrentDepth));
		Instantiate(testPrefab, this.Center, Quaternion.identity);
		testPrefab.transform.localScale = prefabScale;
	}

	int InQuadrant(GameObject particleObject)
	{
		// ___ ___
		//| 0 | 1 |
		//|___|___|
		//| 3 | 2 |
		//|___|___|

		return (particleObject.transform.position.y < a.y) ?					//if (object is below X axis) {execute first parenthesis} else {second}
				((particleObject.transform.position.x < a.x) ? 3 : 2) : 		//if (the object is to left of y axis and below X axis) return 3 else 2
					((particleObject.transform.position.x < a.x) ? 0 : 1);		//if (the object is to left of y axis and above X axis) return 0 else 1

	}

}
