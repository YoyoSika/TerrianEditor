using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[RequireComponent(typeof(TerrianImExporter))]
public class MeshEditor : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	RenderData mData;
	public static int width = 64;
	public static int height = 32;
	public static float heightMax = 10f;


	public bool showGrid = true;
	public bool showNormal = true;

	[Range(1, 10)]
	public float brushRadius = 5;
	[Range(0.01f, 0.5f)]
	public float strenth = 0.2f;
	[Range(0.01f, 10)]
	public float strenthTex = 0.03f;



	[HideInInspector]
	public Vector3 clickPoint;
	[HideInInspector]
	public bool drawBegin = false;

	public EditMode editMode = EditMode.terrian;

	private MeshFilter mFilter;
	public MeshFilter MFilter
	{
		get {
			if (mFilter == null) {
				mFilter = GetComponent<MeshFilter>();
			}
			return mFilter;

		}

		set {
			mFilter = value;
		}
	}

	private MeshCollider mCollider;
	public MeshCollider MCollider
	{
		get {
			if (mCollider == null) {
				mCollider = GetComponent<MeshCollider>();
			}
			return mCollider;
		}

		set {
			mCollider = value;
		}
	}

	private TerrianImExporter _mImExporter;
	public TerrianImExporter mImExporter
	{
		get {
			if(_mImExporter == null)
				_mImExporter = GetComponent<TerrianImExporter>();
			return _mImExporter;
		}
	}


	private void Awake()
	{
		MFilter = GetComponent<MeshFilter>();
	}
	public void OnMouseClicked(Vector3 point, bool isLeftClick)
	{
		int upordown = isLeftClick ? 1 : -1;
		switch (editMode) {
			case EditMode.terrian: {
					//todo 法线也要跟着做变换
					if (drawBegin && (mData.verticles.Length > 0)) {
						float distance = 10000;
						for (int i = 0; i < mData.verticles.Length; i++) {
							distance = Vector3.Distance(point, mData.verticles[i]);
							if (distance <= brushRadius) {
								mData.verticles[i].y += Mathf.Pow(1 - (distance / brushRadius), 2) * strenth * upordown;
								mData.terrians[i].x = mData.verticles[i].y;
							}
						}
					}
                    RebuildNormals(mData);
                    Fresh();
					break;
				}
			case EditMode.grass: {
					if (drawBegin && (mData.verticles.Length > 0)) {
						float distance = 10000;
						for (int i = 0; i < mData.verticles.Length; i++) {
							distance = Vector3.Distance(point, mData.verticles[i]);
							if (distance <= brushRadius) {
								mData.terrians[i].y += Mathf.Pow(1 - (distance / brushRadius), 2) * strenthTex * upordown;
							}
						}
					}
					Fresh();
					break;
				}
			case EditMode.sand: {
				if (drawBegin && (mData.verticles.Length > 0)) {
					float distance = 10000;
					for (int i = 0; i < mData.verticles.Length; i++) {
						distance = Vector3.Distance(point, mData.verticles[i]);
						if (distance <= brushRadius) {
							mData.terrians[i].z += Mathf.Pow(1 - (distance / brushRadius), 2) * strenthTex * upordown;
						}
					}
				}
				Fresh();
				break;
			}
			case EditMode.snow: {
				if (drawBegin && (mData.verticles.Length > 0)) {
					float distance = 10000;
					for (int i = 0; i < mData.verticles.Length; i++) {
						distance = Vector3.Distance(point, mData.verticles[i]);
						if (distance <= brushRadius) {
							mData.terrians[i].w+= Mathf.Pow(1 - (distance / brushRadius), 2) * strenthTex * upordown;
						}
					}
				}
				Fresh();
				break;
			}

		}
	}
    public void RebuildNormals(RenderData renderData)
    {
        if (renderData.verticles.Length > 0) {
            //用叉积计算每个顶点相邻的四个三角形的法线，然后求法线均值
            //              dot
            //      dot   dot  dot
            //              dot
            //先计算所有三角形的面法线
            Vector3 left = Vector3.one;
            Vector3 right = Vector3.one;
            Vector3 up = Vector3.one;
            Vector3 down = Vector3.one;
            Vector3 center = Vector3.one;
            for (int column = 1; column < width - 1; column++) {
                for (int row = 1; row < height - 1; row++) {
                    center = renderData.verticles[row * width + column];
                    left = renderData.verticles[row * width + column - 1];
                    right = renderData.verticles[row * width + column + 1];
                    up = renderData.verticles[(row + 1) * width + column];
                    down = renderData.verticles[(row - 1) * width + column];

                    Vector3 leftUp = getTriNormal(left,up,center);
                    Vector3 rightUp = getTriNormal(up,right,center);
                    Vector3 leftDown = getTriNormal(left,center,down);
                    Vector3 rightDown = getTriNormal(center,right,down);
                    renderData.normals[row * width + column] = getAverage(leftUp, rightUp, leftDown, rightDown);
                }
            }
        }
    }

    //计算三角形的法线
    public Vector3 getTriNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(a - b, c - b).normalized * -1;
    }
    //计算法线均值
    public  Vector3 getAverage(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        return ((a + b + c+ d)/4).normalized;
    }


    public void Fresh()
	{
		if (mData.verticles.Length > 0) {
			Mesh mesh = MFilter.sharedMesh;
			mesh.vertices = mData.verticles;
			mesh.normals = mData.normals;
			mesh.uv = mData.uvs;
			mesh.triangles = mData.triangles;
			MCollider.sharedMesh = null;
			MCollider.sharedMesh = mesh;

			mImExporter.FreshTex(mData);
		}
	}
	public void ClearMesh()
	{

		if (MFilter.sharedMesh != null) {
			MFilter.sharedMesh.Clear();
		}
		Mesh mesh = new Mesh();
		MFilter.sharedMesh = mesh;
	}
	public void RebuldPlane()
	{
		DestroyImmediate(mImExporter.mTexture);
		mData = BuildPlaneData(width, height, Vector3.up);
		ClearMesh();
		Fresh();
		MCollider.sharedMesh = MFilter.sharedMesh;
	}


	public RenderData BuildPlaneData(int width, int height, Vector3 normal)
	{
		RenderData data = new RenderData();
		data.verticles = new Vector3[width * height];
		data.normals = new Vector3[width * height];
		data.uvs = new Vector2[width * height];
		data.triangles = new int[3 * (2 * (width - 1) * (height - 1))];
		data.terrians = new Vector4[width * height];
		for (int row = 0; row < height; row++) {//i 行
			for (int column = 0; column < width; column++) { // j 列
				data.verticles[row * width + column] = new Vector3(column, 0, row);
				data.normals[row * width + column] = normal;
				data.uvs[row * width + column] = new Vector2( (float)column / (width - 1), (float)row / (height - 1));
                //todo 三角形渲染方向问题 按道理应该逆时针为正面？
                if ((row + 1) < height && (column + 1) < width) {
                    //三角形序号
                    int triStartIndex = row * (2 * (width - 1) * 3) + (2 * column * 3);// 1 0
                                                                                       //与顶点序号对应
                    data.triangles[triStartIndex + 0] = (row + 1) * width + column;//左上
                    data.triangles[triStartIndex + 1] = row * width + column + 1;//右下
                    data.triangles[triStartIndex + 2] = row * width + column;//左下

                    data.triangles[triStartIndex + 3] = (row + 1) * width + column;//左上
                    data.triangles[triStartIndex + 4] = (row + 1) * width + column + 1;//右上
                    data.triangles[triStartIndex + 5] = row * width + column + 1;//右下
                }
            }
		}
		return data;
	}

	void OnDrawGizmos()
	{
		//网格
		if ((showNormal || showGrid )&& mData.verticles != null && mData.verticles.Length > 0) {
			for (int i = 0; i < mData.verticles.Length; i++) {
				Gizmos.color = Color.white;
				if (i == 0) {
					Gizmos.color = Color.red;
				}
				if(showGrid)
					Gizmos.DrawSphere(mData.verticles[i], 0.1f);
				if(showNormal)
					Gizmos.DrawLine(mData.verticles[i], mData.verticles[i] + mData.normals[i] * 2);
			}
		}

		if (drawBegin) {
			//笔刷范围
			Color colorBrush = (Color.blue + Color.white * 4) / 5;
			colorBrush.a = 0.5f;
			Gizmos.color = colorBrush;
			Gizmos.DrawSphere(clickPoint, brushRadius);
		}
	}

	public void Load(string addr)
	{
		Texture2D tex  = mImExporter.LoadTex2D(addr);
		RenderData  data = BuildPlaneData(tex.width, tex.height, Vector3.up);
		//读取高度图，修改mesh
		//法线
		//todo
		//顶点高度
		for (int y = 0; y < tex.height; y++) {
			for (int x = 0; x < tex.width; x++) {
				Color color = tex.GetPixel(x, y);
				data.terrians[y * tex.width + x] = color * heightMax;
				data.verticles[y * tex.width + x].y = color.r * heightMax;
			}
		}
		mData = data;
		Fresh();
	}

	public void Export(string addr)
	{
		mImExporter.OutputTex2D(addr, mData);
	}

}
[System.Serializable]
public class RenderData
{
	public Vector3[] verticles;
	public Vector3[] normals;
	public Vector2[] uvs;
	public int[] triangles;
	public Vector4[] terrians;
}

public enum EditMode
{
	terrian,
	grass,
	sand,
	snow
}