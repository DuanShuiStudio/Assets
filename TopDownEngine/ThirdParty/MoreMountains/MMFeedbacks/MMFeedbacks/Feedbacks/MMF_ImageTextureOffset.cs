using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if MM_UI
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the texture offset of a target UI Image over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您随时间控制目标UI图像的纹理偏移")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Image Texture Offset")]
	public class MMF_ImageTextureOffset : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return ""; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();

        /// 此反馈的可能模式
        public enum Modes { OverTime, Instant }
		//
		public enum MaterialPropertyTypes { Main, TextureID }

		[MMFInspectorGroup("Texture Scale", true, 63, true)]
		/// the UI Image on which to change texture offset on
		[Tooltip("要更改其纹理偏移的UI图像")]
		public Image TargetImage;
		/// whether to target the main texture property, or one specified in MaterialPropertyName
		[Tooltip("是针对主纹理属性还是MaterialPropertyName中指定的属性")]
		public MaterialPropertyTypes MaterialPropertyType = MaterialPropertyTypes.Main;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("例如_MainTex_ST或_MainTex（如果您没有将UseMaterialPropertyBlocks设置为true）")]
		[MMEnumCondition("MaterialPropertyType", (int)MaterialPropertyTypes.TextureID)]
		public string MaterialPropertyName = "_MainTex_ST";
		/// whether the feedback should affect the material instantly or over a period of time
		[Tooltip("反馈是否应该立即影响材质，还是延时影响")]
		public Modes Mode = Modes.OverTime;
		/// how long the material should change over time
		[Tooltip("材质更改的延时时间")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not the values should be relative 
		[Tooltip("是否应该使用相对值")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果为真，调用该反馈将触发它，即使它正在进行中。如果为假，它将阻止任何新的播放，直到当前的播放结束。")] 
		public bool AllowAdditivePlays = false;
        
		[MMFInspectorGroup("Intensity", true, 65)]
		/// the curve to tween the offset on
		[Tooltip("用于调整偏移量的曲线")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve OffsetCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the offset curve's 0 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("将偏移曲线的0重新映射到的值，在最小值和最大值之间随机化 - 如果您不希望有任何随机性，请在最小值和最大值中输入相同的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the offset curve's 1 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("将偏移曲线的1重新映射到的值，在最小值和最大值之间随机化 - 如果您不希望有任何随机性，请在最小值和最大值中输入相同的值")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapOne = Vector2.one;
		/// the value to move the intensity to in instant mode
		[Tooltip("在即时模式下要移动到的强度值")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Vector2 InstantOffset;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;
		protected Material _material;

        /// 此反馈的持续时间是过渡的时长
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasRandomness => true;

        /// <summary>
        /// 在初始化时，我们存储初始的纹理偏移量
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			_material = TargetImage.materialForRendering;

			switch (MaterialPropertyType)
			{
				case MaterialPropertyTypes.Main:
					_initialValue = _material.mainTextureOffset;
					break;
				case MaterialPropertyTypes.TextureID:
					_initialValue = _material.GetTextureOffset(MaterialPropertyName);
					break;
			}
		}

        /// <summary>
        /// 在播放时，我们开始改变偏移量
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            
			switch (Mode)
			{
				case Modes.Instant:
					ApplyValue(InstantOffset * intensityMultiplier);
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(TransitionCo(intensityMultiplier));
					break;
			}
		}

        /// <summary>
        /// 这个协程将随时间修改偏移值
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TransitionCo(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetMaterialValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
			IsPlaying = false;
			_coroutine = null;
			yield return null;
		}

        /// <summary>
        /// 将偏移量应用于目标材质
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetMaterialValues(float time, float intensityMultiplier)
		{
			_newValue.x = MMFeedbacksHelpers.Remap(OffsetCurve.Evaluate(time), 0f, 1f, RemapZero.x, RemapOne.x);
			_newValue.y = MMFeedbacksHelpers.Remap(OffsetCurve.Evaluate(time), 0f, 1f, RemapZero.y, RemapOne.y);

			if (RelativeValues)
			{
				_newValue += _initialValue;
			}

			ApplyValue(_newValue * intensityMultiplier);
		}

        /// <summary>
        /// 将指定的值应用于材质
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void ApplyValue(Vector2 newValue)
		{
			switch (MaterialPropertyType)
			{
				case MaterialPropertyTypes.Main:
					_material.mainTextureOffset = newValue;
					break;
				case MaterialPropertyTypes.TextureID:
					_material.SetTextureOffset(MaterialPropertyName, newValue);
					break;
			}
		}

		/// <summary>
		/// 停止反馈
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			Owner.StopCoroutine(_coroutine);
			_coroutine = null;
		}
	}
}
#endif