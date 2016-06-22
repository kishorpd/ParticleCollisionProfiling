using UnityEngine;
using System.Collections;

public class KDTree{


	private KDTree	_ParentNode = null;
	private KDTree	_LowerNode = null;
	private KDTree  _MajorNode = null;
	public int TotalLeafNodes { get; set; }
	public static Main SMainInstance { get; set; }


	Vector3 _ParentPosition;
	bool _IsAlignedToXAxis;
	static int _SMaxDepth;
	static GameObject _SParentPrefab;

	private GameObject _ParticleObject = null;
	private GameObject _SelfPartitionPrefab = null;


	struct Max_Min
	{
		public float Max;
		public float Min;
	};


	private Max_Min _X;
	private Max_Min _Y;

	public KDTree(GameObject parentObject, float parentHeight, float parentWidth)
	{
		//this has to be the main parent node of the whole KDTree

		//ParentObject:  get center and set this as the parent prefab
		_SParentPrefab = parentObject;
		_IsAlignedToXAxis = false;

		//Width
		_X.Max = _SParentPrefab.transform.position.x + parentWidth / 2;
		_X.Min = _SParentPrefab.transform.position.x - parentWidth / 2;
	
		//Height
		_Y.Max = _SParentPrefab.transform.position.y + parentHeight / 2;
		_Y.Min = _SParentPrefab.transform.position.y - parentHeight / 2;

		//
		TotalLeafNodes = 1;
	//	AddSprite();
	}


	KDTree(KDTree parent, float min, float max)
	{
		_IsAlignedToXAxis = !parent._IsAlignedToXAxis;

		if (_IsAlignedToXAxis)
		{
			//Width
			_X.Max = max;
			_X.Min = min;
			_Y = parent._Y;
		}
		else 
		{
			//Height
			_Y.Max = max;
			_Y.Min = min;
			_X = parent._X;
		}


		//
		++TotalLeafNodes;

	}


	public bool Insert(GameObject particleObject)
	{
		if (_ParticleObject == null)
		{
			_ParticleObject = particleObject;
			DrawSplitSelf();
			TotalLeafNodes++;



			if (_IsAlignedToXAxis)
			{
				//set the min and max of the 
				float splitAt = particleObject.transform.position.y;
				_LowerNode = new KDTree(this, _Y.Min, splitAt);
				_MajorNode = new KDTree(this, splitAt, _Y.Max);
			}
			else
			{
				float splitAt = particleObject.transform.position.x;
				_LowerNode = new KDTree(this, _X.Min, splitAt);
				_MajorNode = new KDTree(this, splitAt, _X.Max);
			}
		
			return true;
		}
		else 
		{
			//TODO: look if there is a possibility of optimizing this
			if (_IsAlignedToXAxis)
			{
				if (particleObject.transform.position.y < _ParticleObject.transform.position.y)
				{
					//set the min and max of the 
					_LowerNode.Insert(particleObject);
				}
				else
				{
					_MajorNode.Insert(particleObject);
				}
			}
			else
			{
				if (particleObject.transform.position.x < _ParticleObject.transform.position.x)
				{
					//set the min and max of the 
					_LowerNode.Insert(particleObject);
				}
				else
				{
					_MajorNode.Insert(particleObject);
				}
			}

		}
		return true;
	}

	void DrawSplitSelf()
	{
		if (_SelfPartitionPrefab == null)
		{
			Vector2 tempPosition = _ParticleObject.transform.position;
			float length = 0.0f;
			if (_IsAlignedToXAxis)
			{
				length =  _X.Max - _X.Min;
				tempPosition.x = _X.Min + (length / 2);
			}
			else
			{
				length = _Y.Max - _Y.Min;
				tempPosition.y =  _Y.Min + (length / 2);
			}
			_SelfPartitionPrefab = SMainInstance._SpawnKDTreePartitioner(Mathf.Abs(length), _IsAlignedToXAxis, tempPosition);

		}
	}

	public void Clear()
	{
		if (_LowerNode != null)
			_LowerNode.Clear();
		if (_MajorNode != null)
			_MajorNode.Clear();

		_LowerNode = null;
		_MajorNode = null;
		_ParticleObject = null;
		SMainInstance.DestroyQuadTreeObject(_SelfPartitionPrefab);
		_SelfPartitionPrefab = null;
		_ParentNode = null;
	}

	public void ClearStaticData()
	{
		_SMaxDepth = 0;
	}
}
