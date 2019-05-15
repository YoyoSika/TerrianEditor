using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[CustomEditor(typeof(MeshEditor))]
public class MeshEditorInspector : Editor
{
	MeshEditor mInstance;

	void OnEnable()
	{
		mInstance = (MeshEditor)target;
	}

	public override void OnInspectorGUI()
	{
		base.DrawDefaultInspector();
		MeshEditor.width = EditorGUILayout.IntField("Width", MeshEditor.width);
		MeshEditor.height = EditorGUILayout.IntField("Height", MeshEditor.height);
		EditorGUILayout.FloatField("HeightMax", MeshEditor.heightMax);
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		Color colorBuf = GUI.color;
		GUI.color = mInstance.drawBegin ? Color.red : colorBuf;
		if (GUILayout.Button("Edit", GUILayout.Height(48))) {
			mInstance.drawBegin = !mInstance.drawBegin;
		}
		GUI.color = colorBuf;

		if (GUILayout.Button("Render", GUILayout.Height(48))) {
			mInstance.Fresh();
		}
		if (GUILayout.Button("RebuldPlane", GUILayout.Height(48))) {
			mInstance.RebuldPlane();
		}
		if (GUILayout.Button("ClearMesh", GUILayout.Height(48))) {
			mInstance.ClearMesh();
		}
		if (GUILayout.Button("Load", GUILayout.Height(48))) {
			string addr = EditorUtility.OpenFilePanel("Load", Application.dataPath + @"/Assets/texture", "png");
			if (addr.Length > 0) {
				mInstance.Load(addr);
			}

		}
		if (GUILayout.Button("Save", GUILayout.Height(48))) {
			string addr = EditorUtility.SaveFilePanel("Save", Application.dataPath+@"/Assets/texture", "terrian1","png");
			if (addr.Length > 0) {
				mInstance.Export (addr);
				AssetDatabase.Refresh();
			}
		}
	}

	void OnSceneGUI()
	{
        if (mInstance.drawBegin) {
            //让Scene View的鼠标选择被禁用
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
		if (Event.current.type == EventType.mouseMove) {
			SceneView.RepaintAll();
			RaycastHit hitInfo;
			bool isHitted = Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hitInfo);
			if(isHitted)
				mInstance.clickPoint = hitInfo.point;
		}

		if (Event.current.type == EventType.MouseDown  || Event.current.type == EventType.MouseDrag) {
			RaycastHit hitInfo;
			bool isHitted = Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hitInfo);
			if(isHitted)
				mInstance.clickPoint = hitInfo.point;

			if (Event.current.button == 0) {
				mInstance.OnMouseClicked(hitInfo.point,true);
			}
			if (Event.current.button == 1) {
				mInstance.OnMouseClicked(hitInfo.point,false);
			}
			if (mInstance.drawBegin && Event.current.button != 2 && isHitted && Event.current.type != EventType.MouseDrag)
				Event.current.Use();
			SceneView.RepaintAll();
		}


	}

}
