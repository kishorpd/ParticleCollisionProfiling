using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DrawFrustum : MonoBehaviour {

	public GameObject bot;
	 List<GameObject> Vertices;

	 public static List<Vector3> _SClampedLines;
	 public static List<Vector3> _SHierarchyLines;
	 public static List<Vector3> _SPartitionLines;

	public Material lineMaterial;
	private Color mainColor = new Color(1f, 0f, 1f, 1f);
	private Color subColor = new Color(0f, 0.5f, 1f, 0f);

	static public bool DrawClamping = false;
	static public bool DrawHierarchy = false;
	static public bool DrawPartitions = false;

	public Vector3 pos;

	// Use this for initialization
	void Start () {
		pos = new Vector3();
		_SClampedLines = new List<Vector3>();
		_SHierarchyLines = new List<Vector3>();
		_SPartitionLines = new List<Vector3>();
		Vertices = bot.GetComponent<ControlBot>().Vertices;
		Debug.Log("Vertices.Count : " + Vertices.Count);
	}
	
	// Update is called once per frame
	void Update () {
	
		CreateLineMaterial();
	}

	public void ToggleClampView()
	{
		DrawClamping = !DrawClamping;
	}

	public void ToggleHierarchyView()
	{
		DrawHierarchy = !DrawHierarchy;
	}

	void CreateLineMaterial()
	{

		if (!lineMaterial)
		{
			lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
				"SubShader { Pass { " +
				"    Blend SrcAlpha OneMinusSrcAlpha " +
				"    ZWrite Off Cull Off Fog { Mode Off } " +
				"    BindChannels {" +
				"      Bind \"vertex\", vertex Bind \"color\", color }" +
				"} } }");
			lineMaterial.hideFlags = HideFlags.HideAndDontSave;
			lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}


	void OnPostRender()
	{
		GL.PushMatrix();
		// set the current material

		lineMaterial.SetPass(0);
		GL.Begin(GL.LINES);
		GL.Color(Color.cyan);
		DrawVertices();


		GL.End();
		//red
		GL.Begin(GL.LINES);

		GL.Color(Color.red);

		if (DrawClamping)
			DrawClamps();

		if (DrawHierarchy)
			DrawPartitionLines();

		GL.End();
		GL.PopMatrix();
	}

	void DrawVertices()
	{
		for (int i = 0; i < 4; ++i)
		{
			GL.Vertex(Vertices[i].transform.position);
			GL.Vertex(Vertices[i+1].transform.position);
		}
	}

	void DrawClamps()
	{
		foreach (Vector3 line in _SClampedLines)
		{
			GL.Vertex(line);
		}
			_SClampedLines.Clear();
	}

	void DrawHierarchyLines()
	{
		GL.Color(Color.blue);
		//Debug.Log(" No Line ");
		foreach (Vector3 line in _SHierarchyLines)
		{
			GL.Vertex(line);
			//	Debug.Log(" Line " + line);
		}
		_SHierarchyLines.Clear();
	}

	void DrawPartitionLines()
	{
		GL.Color(Color.blue);
		//Debug.Log(" No Line ");
		foreach (Vector3 line in _SPartitionLines)
		{
			GL.Vertex(line);
		}
		_SPartitionLines.Clear();
	}

	public void DrawLinePQ(Vector3 p, Vector3 q)
	{
		GL.Vertex(p);
		GL.Vertex(q);
	}



	void DrawLine(Vector3 center, Vector3 width)
	{
		GL.Vertex(center + width);
		GL.Vertex(center - width);
	}


}
