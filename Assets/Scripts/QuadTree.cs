using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class QuadTree {//: MonoBehaviour {

	public float Height { get; set; }
	public float Width { get; set; }
	public Vector3 Center { get; set; }
	
	private int _TotalLeafNodes { get; set; }

	private List<GameObject> Particles;
	private QuadTree ParentNode;
	private QuadTree[] ChildNodes;
	private Rect a;

	public QuadTree(Vector3 parentCenter, float parentHeight, float parentWidth, QuadTree parent)
	{
		//if parent  == null?
		
		Height = parentHeight;
		Width = parentWidth;
		Center = parentCenter;
		Particles = new List<GameObject>();
	}


	public void Insert(GameObject obj)
	{
		if (_TotalLeafNodes > 3)//insert first element ofchild;
		{
			//clear list of gameObjects and insert in the quadtrees[0],[1],[2],[3];
			for (int leaf = 0; leaf < 4; ++leaf )
				Insert(Particles[leaf], InQuadrant(Particles[leaf]));
			Insert(obj, InQuadrant(obj));
			Particles.Clear();
		}
		else 
		{
			Particles.Add(obj);
		}
		++_TotalLeafNodes;
	}

	public void Insert(GameObject obj, int Quadrant)
	{
		if (_TotalLeafNodes == 0)
		{
			//set parent node of the child and insert the obj in the child
			//through a constructor here
			//ChildNodes[Quadrant] = new QuadTree(.......);
		}
	}


	int InQuadrant(GameObject obj)
	{
		// _ _
		//|0|1|
		//|_|_|
		//|3|2|
		//|_|_|

		return (obj.transform.position.y < a.y) ?					//if (object is below X axis) {execute first parenthesis} else {second}
				((obj.transform.position.x < a.x) ? 3 : 2): 		//if (the object is to left of y axis and below X axis) return 3 else 2
					((obj.transform.position.x < a.x) ? 0 : 1);		//if (the object is to left of y axis and above X axis) return 0 else 1

	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
