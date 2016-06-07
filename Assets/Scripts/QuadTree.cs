using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTree {// : MonoBehaviour {

	public float Height { get; set; }
	public float Width { get; set; }
	public Vector3 Center { get; set; }
	public Main MainInstance { get; set; }

	private int _TotalLeafNodes { get; set; }
	private int _CurrentDepth = 0;//	{ get; set; }
	public static int _MaxDepth { get; set; }

	private QuadTree			_ParentNode;
	private Dictionary<int, QuadTree> _ChildNodes = new Dictionary<int, QuadTree>();
	private GameObject			_ChildNode;

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
		//Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>Quadrant:" + parentCenter);
		//_Particles = new List<GameObject>();
	}


	private QuadTree(QuadTree parent)
	{
		_CurrentDepth = parent._CurrentDepth + 1;
		//Debug.Log("Depth:" + _CurrentDepth);

		Height = parent.Height / 2;
		Width = parent.Width / 2;
		_ParentNode = parent;
		MainInstance = parent.MainInstance;
	}



	public void SetCenter( Vector3 parentCenter)
	{
		Center = parentCenter;
	}

	public void SetConstructor(QuadTree parent, Vector3 parentCenter)
	{
		_CurrentDepth = parent._CurrentDepth + 1;
		Debug.Log("Depth:" + _CurrentDepth);

		Height = parent.Height / 2;
		Width = parent.Width / 2;
		_ParentNode = parent;
		Center = parentCenter;
	}
		
	public void Insert(GameObject particleObject)
	{

		int Quadrant = InQuadrant(particleObject);
		if (_TotalLeafNodes == 0)
		{
 			//insert the childnode
			_ChildNode = particleObject;
		}
		else if (_TotalLeafNodes == 1)
		{ 
			//set the cached child{gameobject} to corresponding child QuadTree
			int tempQuadrant = InQuadrant(_ChildNode);
			_ChildNodes[tempQuadrant] = new QuadTree(this);
			_ChildNodes[tempQuadrant].SetCenter(GetCenterOfQuadrant(tempQuadrant));
			
			//clear the cached childNode
			_ChildNode = null;
			
			//set the parameter in respective child QuadTree
			_ChildNodes[Quadrant] = new QuadTree(this);
			_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
		
			//spawn split prefab
			DrawSplitSelf();
		}
		else
		{
			if (!_ChildNodes.ContainsKey(Quadrant))
			{
				_ChildNodes[Quadrant] = new QuadTree(this);
				_ChildNodes[Quadrant].SetCenter(GetCenterOfQuadrant(Quadrant));
			}
				_ChildNodes[Quadrant].Insert(particleObject);
		}
		//Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>Quadrant:" + Quadrant + "Depth:" + _CurrentDepth);
		++_TotalLeafNodes;
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
		//Debug.Log("center:" + (Center) + "quadCenter:" + (quadCenter));
		return	quadCenter;
	}

	void DrawSplitSelf()
	{
		MainInstance.SpawnSplit(_CurrentDepth, this.Center); 
	}

	int InQuadrant(GameObject particleObject)
	{
		// ___ ___
		//| 0 | 1 |
		//|___|___|
		//| 3 | 2 |
		//|___|___|

		//Debug.Log("center:" + (Center) + "Depth:" + _CurrentDepth);
		return (particleObject.transform.position.y < Center.y) ?					//if (object is below X axis) {execute first parenthesis} else {second}
				((particleObject.transform.position.x < Center.x) ? 3 : 2) : 		//if (the object is to left of y axis and below X axis) return 3 else 2
					((particleObject.transform.position.x < Center.x) ? 0 : 1);		//if (the object is to left of y axis and above X axis) return 0 else 1

	}

}
