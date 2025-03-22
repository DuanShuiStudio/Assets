using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Rooms的自定义编辑器，在场景视图中绘制它们的名称
    /// </summary>
    [CanEditMultipleObjects]
	[CustomEditor(typeof(Room), true)]
	[InitializeOnLoad]
	public class RoomEditor : Editor
	{
		[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
		static void DrawHandles(Room room, GizmoType gizmoType)
		{
			Room t = (room as Room);

			GUIStyle style = new GUIStyle();

            // 绘制路径项编号
            style.normal.textColor = MMColors.Pink;
			Handles.Label(t.transform.position + (Vector3.up * 2f) + (Vector3.right * 2f), t.name, style);
		}
	}
}