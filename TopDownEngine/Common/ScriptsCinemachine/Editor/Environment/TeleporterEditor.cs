using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Teleporters的自定义编辑器，在场景视图中绘制它们的名称
    /// </summary>
    [CanEditMultipleObjects]
	[CustomEditor(typeof(Teleporter), true)]
	[InitializeOnLoad]
	public class TeleporterEditor : MMMonoBehaviourUITKEditor 
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawHandles(Teleporter teleporter, GizmoType gizmoType)
		{
			Teleporter t = (teleporter as Teleporter);

			GUIStyle style = new GUIStyle();

            // 绘制路径项编号
            style.normal.textColor = Color.cyan;
			style.alignment = TextAnchor.UpperCenter;
			float verticalOffset = (t.transform.lossyScale.x > 0) ? 1f : 2f;
			Handles.Label(t.transform.position + Vector3.up * (2f + verticalOffset), t.name, style);
		}
	}
}