using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 在反馈的各个阶段，将对象设置为激活或非激活状态。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈信息可让你为着色器设置全局属性，或启用 / 禁用关键字。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Shader Global")]
	public class MMF_ShaderGlobal : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		#endif

		public enum Modes { SetGlobalColor, SetGlobalFloat, SetGlobalInt, SetGlobalMatrix, SetGlobalTexture, SetGlobalVector, EnableKeyword, DisableKeyword, WarmupAllShaders }

		[MMFInspectorGroup("Shader Global", true, 24)]
		/// the selected mode for this feedback
		[Tooltip("此反馈所选择的模式。")]
		public Modes Mode = Modes.SetGlobalFloat;
		/// the name of the global property
		[Tooltip("全局属性的名称。\r\n")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public string PropertyName = "";
		/// the name ID of the property retrieved by Shader.PropertyToID
		[Tooltip("通过 Shader.PropertyToID 方法获取的属性的名称 ID。")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor, (int)Modes.SetGlobalFloat, (int)Modes.SetGlobalInt, (int)Modes.SetGlobalMatrix, (int)Modes.SetGlobalTexture, (int)Modes.SetGlobalVector)]
		public int PropertyNameID = 0;
		/// a global color property for all shaders
		[Tooltip("所有着色器的全局颜色属性。")]
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalColor)]
		public Color GlobalColor = Color.yellow;
		/// a global float property for all shaders
		[Tooltip("所有着色器的全局浮点属性。")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalFloat)]
		public float GlobalFloat = 1f;
		/// a global int property for all shaders
		[Tooltip("所有着色器的全局整数属性。")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalInt)]
		public int GlobalInt = 1;
		/// a global matrix property for all shaders
		[Tooltip("所有着色器的全局矩阵属性。")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalMatrix)]
		public Matrix4x4 GlobalMatrix = Matrix4x4.identity;
		/// a global texture property for all shaders
		[Tooltip("所有着色器的全局纹理属性")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalTexture)]
		public RenderTexture GlobalTexture;
		/// a global vector property for all shaders
		[Tooltip("所有着色器的全局向量属性")] 
		[MMFEnumCondition("Mode", (int)Modes.SetGlobalVector)]
		public Vector4 GlobalVector;
		/// a global shader keyword
		[Tooltip("全局着色器关键字。")] 
		[MMFEnumCondition("Mode", (int)Modes.EnableKeyword, (int)Modes.DisableKeyword)]
		public string Keyword;

		protected Color _initialColor;
		protected float _initialFloat;
		protected int _initialInt;
		protected Matrix4x4 _initialMatrix;
		protected RenderTexture _initialTexture;
		protected Vector4 _initialVector;

        /// <summary>
        /// 播放时，我们会设置全局着色器属性。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.SetGlobalColor:
					if (PropertyName == "")
					{
						Shader.SetGlobalColor(PropertyNameID, GlobalColor);
					}
					else
					{
						Shader.SetGlobalColor(PropertyName, GlobalColor);
					}
					break;
				case Modes.SetGlobalFloat:
					if (PropertyName == "")
					{
						Shader.SetGlobalFloat(PropertyNameID, GlobalFloat);
					}
					else
					{
						Shader.SetGlobalFloat(PropertyName, GlobalFloat);
					}
					break;
				case Modes.SetGlobalInt:
					if (PropertyName == "")
					{
						Shader.SetGlobalInt(PropertyNameID, GlobalInt);
					}
					else
					{
						Shader.SetGlobalInt(PropertyName, GlobalInt);
					}
					break;
				case Modes.SetGlobalMatrix:
					if (PropertyName == "")
					{
						Shader.SetGlobalMatrix(PropertyNameID, GlobalMatrix);
					}
					else
					{
						Shader.SetGlobalMatrix(PropertyName, GlobalMatrix);
					}
					break;
				case Modes.SetGlobalTexture:
					if (PropertyName == "")
					{
						Shader.SetGlobalTexture(PropertyNameID, GlobalTexture);
					}
					else
					{
						Shader.SetGlobalTexture(PropertyName, GlobalTexture);
					}
					break;
				case Modes.SetGlobalVector:
					if (PropertyName == "")
					{
						Shader.SetGlobalVector(PropertyNameID, GlobalVector);
					}
					else
					{
						Shader.SetGlobalVector(PropertyName, GlobalVector);
					}
					break;
				case Modes.EnableKeyword:
					Shader.EnableKeyword(Keyword);
					break;
				case Modes.DisableKeyword:
					Shader.DisableKeyword(Keyword);
					break;
				case Modes.WarmupAllShaders:
					Shader.WarmupAllShaders();
					break;
			}
		}

        /// <summary>
        /// 在恢复时，我们会恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.SetGlobalColor:
					if (PropertyName == "")
					{
						Shader.SetGlobalColor(PropertyNameID, _initialColor);
					}
					else
					{
						Shader.SetGlobalColor(PropertyName, _initialColor);
					}
					break;
				case Modes.SetGlobalFloat:
					if (PropertyName == "")
					{
						Shader.SetGlobalFloat(PropertyNameID, _initialFloat);
					}
					else
					{
						Shader.SetGlobalFloat(PropertyName, _initialFloat);
					}
					break;
				case Modes.SetGlobalInt:
					if (PropertyName == "")
					{
						Shader.SetGlobalInt(PropertyNameID, _initialInt);
					}
					else
					{
						Shader.SetGlobalInt(PropertyName, _initialInt);
					}
					break;
				case Modes.SetGlobalMatrix:
					if (PropertyName == "")
					{
						Shader.SetGlobalMatrix(PropertyNameID, _initialMatrix);
					}
					else
					{
						Shader.SetGlobalMatrix(PropertyName, _initialMatrix);
					}
					break;
				case Modes.SetGlobalTexture:
					if (PropertyName == "")
					{
						Shader.SetGlobalTexture(PropertyNameID, _initialTexture);
					}
					else
					{
						Shader.SetGlobalTexture(PropertyName, _initialTexture);
					}
					break;
				case Modes.SetGlobalVector:
					if (PropertyName == "")
					{
						Shader.SetGlobalVector(PropertyNameID, _initialVector);
					}
					else
					{
						Shader.SetGlobalVector(PropertyName, _initialVector);
					}
					break;
			}
		}
	}
}