using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrianImExporter : MonoBehaviour
{
	Texture2D _mTexture;
	public Texture2D mTexture
	{
		get {
			if (_mTexture == null) {
				_mTexture = new Texture2D(MeshEditor.width, MeshEditor.height);
				mMat.SetTexture("_Terrian", _mTexture);
			}
			return _mTexture;
		}
	}
	Material _mMat;
	public Material mMat
	{
		get {
			if (_mMat == null) {
				_mMat = GetComponent<MeshRenderer>().sharedMaterial;
			}
			return _mMat;
		}
	}
	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{

	}

	public bool ExportHeightMap(RenderData data)
	{
		//todo 打开地址选择面板
		return false;
	}
	public Texture2D LoadTex2D(string filename)
	{
		byte[] bytes;
		bytes = System.IO.File.ReadAllBytes(filename);
		mTexture.LoadImage(bytes);
		mMat.SetTexture("_Terrian",mTexture);
		return mTexture;
	}
	public void OutputTex2D(string filename, RenderData data)
	{
		Texture2D texture = mTexture;
		for (int y = 0; y < texture.height; y++) {
			for (int x = 0; x < texture.width; x++) {
				texture.SetPixel(x, y, data.terrians[y * texture.width + x] / MeshEditor.heightMax);
			}
		}
		texture.Apply();
		System.IO.File.WriteAllBytes(filename, texture.EncodeToPNG());
	}
	
	//刷草的时候
	public void FreshTex(RenderData data)
	{
		Texture2D texture = mTexture;
		for (int y = 0; y < texture.height; y++) {
			for (int x = 0; x < texture.width; x++) {
				texture.SetPixel(x, y, data.terrians[y * texture.width + x] / MeshEditor.heightMax);
			}
		}
		mTexture.Apply();
		//mMat.SetTexture("_Terrian", mTexture);
	}
}
