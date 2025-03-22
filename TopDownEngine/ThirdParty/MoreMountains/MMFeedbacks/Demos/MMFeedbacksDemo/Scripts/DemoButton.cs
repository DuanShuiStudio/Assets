using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
#if MM_UI
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif
using UnityEngine.Scripting.APIUpdating;
namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于处理MMF_PlayerDemo（多媒体播放器演示）和MMFeedbacksDemo（多媒体反馈演示）场景中演示按钮的简单类。
    /// </summary>
    [ExecuteAlways]
	[AddComponentMenu("")]
	public class DemoButton : MonoBehaviour
	{
		[Header("Behaviour行为")]
		public bool NotSupportedInWebGL = false;

		[Header("Bindings绑定")]
		public Button TargetButton;
		public Text ButtonText;
		public Text WebGL;
		public MMF_Player TargetMMF_Player;
		
		protected Color _disabledColor = new Color(255, 255, 255, 0.5f);
		
		protected virtual void OnEnable()
		{
			HandleWebGL();
			TargetButton.onClick.AddListener(OnClickEvent);
		}

		protected void OnDisable()
		{
			TargetButton.onClick.RemoveListener(OnClickEvent);
		}

		public void OnClickEvent()
		{
			TargetMMF_Player?.PlayFeedbacks();
		}

		protected virtual void HandleWebGL()
		{
			if (WebGL != null)
			{
				#if UNITY_WEBGL
					TargetButton.interactable = !NotSupportedInWebGL;    
                    WebGL.gameObject.SetActive(NotSupportedInWebGL);   
					ButtonText.color = NotSupportedInWebGL ? _disabledColor : Color.white;
				#else
					WebGL.gameObject.SetActive(false);
					TargetButton.interactable = true;
					ButtonText.color = Color.white;
				#endif
			}
		}
	}
}
#endif