using UnityEngine;
using System.Collections;

public class ParticleMove : MonoBehaviour
{

	public int radius = 5;
	public Color color = Color.blue;
	public int XRangeMin = -12;
	public int YRangeMin = -5;
	public int XRangeMax = 12;
	public int YRangeMax = 5;

	public bool goUp = true;
	public bool goRight = true;

	public float MinSpeed = 0.1f;
	public float MaxSpeed = 1f;
	float positionX = 1.5f;
	float positionY = 1.5f;
	float speed = 0.0f;
	float stepX = 0.0f;
	float stepY = 0.0f;


	public Vector3 position;
	Vector3 scale;

	// Use this for initialization
	void Start()
	{
		// setPosition();
		//setRadius();
		setDirection();

	}


	// Update is called once per frame
	void Update()
	{
		move();
	}

	// Update is called once per frame
	void setPosition()
	{
		//set random position
		position.x = (float)Random.Range(XRangeMin, XRangeMax);
		position.y = (float)Random.Range(YRangeMin, YRangeMax);
		position.z = 0;
		transform.position = position;
	}

	void setRadius()
	{
		//set scale
		scale.x = radius;
		scale.y = radius;
		scale.z = 0;
		transform.localScale = scale;
	}

	void setDirection()
	{
		//set random direction
		int angle = Random.Range(0, 360);
		speed = Random.Range(MinSpeed, MaxSpeed);

		stepX = Mathf.Abs(speed * Mathf.Sin(angle));
		stepY = Mathf.Abs(speed * Mathf.Cos(angle));

	}

	/*
	void setColor()
	{
		//set random color
		//Create random color
		Color col1 = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), 1.0f);
		Mesh mesh1 = GetComponent<MeshFilter>().mesh;

		//Change colors of meshes
		Vector3[] vertices = mesh1.vertices;
		Color[] colors = new Color[vertices.Length];

		for (int i = 0; i < vertices.Length; i++)
		{
			colors[i] = col1;
			colors[i] = color;
		}
		mesh1.colors = colors; //Set new colors of vertices
	}
	 */

	void move()
	{
		positionX = transform.position.x;
		positionY = transform.position.y;
		if (positionX > XRangeMax)
		{
			goRight = false;
		}

		if (positionX < XRangeMin)
		{
			goRight = true;
		}

		if (positionY > YRangeMax)
		{
			goUp = false;
		}

		if (positionY < YRangeMin)
		{
			goUp = true;
		}

		if (goUp == true)
		{
			position.y = stepY;
		}
		else
		{
			position.y = -stepY;
		}

		if (goRight == true)
		{
			position.x = stepX;
		}
		else
		{
			position.x = -stepX;
		}

		transform.position += position;
	}
}
