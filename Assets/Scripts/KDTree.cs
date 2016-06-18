using UnityEngine;
using System.Collections;

public class KDTree{// : MonoBehaviour {


	KDTree _ParentNode = null;
	KDTree	_SmallerNode = null;
	KDTree  _GreaterNode = null;
	public int TotalLeafNodes { get; set; }


	Vector3 _ParentPosition;
	bool _IsAlignedToXAxis;
	static int _MaxDepth;
	static GameObject _ParentPrefab;

	private GameObject _ParticleObject = null;
	private SpriteRenderer _SplitSprite = null;


	struct Max_Min
	{
		public float Max;
		public float Min;
	};


	Max_Min _X;
	Max_Min _Y;

	public KDTree(GameObject parentObject, float parentHeight, float parentWidth)
	{
		//this has to be the main parent node of the whole KDTree

		//ParentObject:  get center and set this as the parent prefab

		_ParentPrefab = parentObject;
		_IsAlignedToXAxis = true;

		//Width
		_X.Max = _ParentPrefab.transform.position.x + parentWidth / 2;
		_X.Min = -_X.Max;


		//Height
		_Y.Max = _ParentPrefab.transform.position.y + parentHeight / 2;
		_Y.Min = -_Y.Max;

		//
		TotalLeafNodes = 1;
	//	AddSprite();
	}


	//KDTree(GameObject particleObject, float parentHeight, float parentWidth)
	//{ 
	//}


	public bool Insert(GameObject particleObject)
	{
		if (_ParticleObject == null)
		{
			_ParticleObject = particleObject;
			_IsAlignedToXAxis = !_ParentNode._IsAlignedToXAxis;
			TotalLeafNodes++;


			return true;
		}
		else 
		{
			if (_IsAlignedToXAxis)
			{
				//set the min and max of the 

			}
			else
			{ 
			}
		}

		return true;
	}

	void AddSprite()
	{
		GUI2D _spritesParent =  _ParentPrefab.AddComponent<GUI2D>();

		Renderer rend = _spritesParent.GetComponent<Renderer>();
		rend.material.mainTexture = Resources.Load("Art/LineOnly") as Texture;

		//SpriteRenderer temp  = new SpriteRenderer();
		////_SplitSprite =  _ParentPrefab.AddComponent<SpriteRenderer>();
		//temp.material.mainTexture = Resources.Load("Art/LineOnly") as Texture;
		//_spritesParent._Particles.Add(temp);
		Debug.Log("KD____________________________________TREEEEEEEEEEEEEEEE");
	}

}
