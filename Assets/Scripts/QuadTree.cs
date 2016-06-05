using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTree {//: MonoBehaviour {

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

	public QuadTree(Vector3 parentCenter, float parentHeight, float parentWidth, QuadTree parent)
	{
		if (parent != null)
		{
			_CurrentDepth = parent._CurrentDepth + 1;
		}
		else
		{
			_CurrentDepth = 0;
		}

		Height = parentHeight;
		Width = parentWidth;
		Center = parentCenter;
		_ParentNode = parent;
		_Particles = new List<GameObject>();
	}


	public void Insert(GameObject particleObject)
	{
		if (_TotalLeafNodes > 3)//insert first element ofchild;
		{
			if (_Particles.Count == 0) //distribute the list
			{
				//insert in the specific QuadTree corresponding to the Quadrant
				Debug.Log("This must be the fifth element everyone is talking about i guess!");
				Insert(particleObject, InQuadrant(particleObject));
			}
			else 
			{
				int Quadrant;
				//clear list of gameObjects and insert in the quadtrees[0],[1],[2],[3];
				for (int leaf = 0; leaf < 4; ++leaf)
				{
					Quadrant = InQuadrant(_Particles[leaf]);
					_ChildNodes[Quadrant] = new QuadTree(GetCenterOfQuadrant(leaf),  Height/2, Width/2, this);
					_ChildNodes[Quadrant].Insert(_Particles[leaf], InQuadrant(_Particles[leaf]));
				}
				Quadrant = InQuadrant(particleObject);
				_ChildNodes[Quadrant].Insert(particleObject, Quadrant);
				_Particles.Clear();
			}
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

	Vector2 GetCenterOfQuadrant(int quadrant)
	{
		Vector2 quadCenter = new Vector2();

		if (quadrant == 0 || quadrant == 3)
		{
			quadCenter.x -= (Width/2);
			quadCenter.y = (quadrant == 3) ? (quadCenter.y - (Height / 2)) : quadCenter.y + (Height / 2);
		}
		else 
		{
			quadCenter.y -= (Height / 2);
			quadCenter.x = (quadrant == 3) ? (quadCenter.x - (Width / 2)) : quadCenter.x + (Width / 2);
		}

		return	quadCenter;
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
