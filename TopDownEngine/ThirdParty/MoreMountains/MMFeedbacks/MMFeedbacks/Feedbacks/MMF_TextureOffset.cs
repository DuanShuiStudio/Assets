using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the texture offset of a target material over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈将使你能够随着时间的推移控制目标材质的纹理偏移量。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Texture Offset")]
	public class MMF_TextureOffset : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRenderer == null); }
		public override string RequiredTargetText { get { return TargetRenderer != null ? TargetRenderer.name : "";  } }
		public override string RequiresSetupText { get { return "这种反馈需要设置一个目标渲染器才能正常工作。你可以在下面设置一个。 "; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRenderer = FindAutomatedTarget<Renderer>();

        /// 这种反馈可能的模式
        public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Texture Scale", true, 63, true)]
		/// the renderer on which to change texture offset on
		[Tooltip("要在其上更改纹理偏移的渲染器 ")]
		public Renderer TargetRenderer;
		/// the material index
		[Tooltip("材质索引")]
		public int MaterialIndex = 0;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("属性名称，例如“_MainTex_ST” ，或者如果你没有将“UseMaterialPropertyBlocks”设置为“true”，则属性名称为“_MainTex” 。 ")]
		public string MaterialPropertyName = "_MainTex_ST";
		/// whether the feedback should affect the material instantly or over a period of time
		[Tooltip("反馈是应该立即对材质产生影响，还是在一段时间内逐渐产生影响。 ")]
		public Modes Mode = Modes.OverTime;
		/// how long the material should change over time
		[Tooltip("这种材质随时间变化应该持续多长时间。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not the values should be relative 
		[Tooltip("这些值是否应该是相对的。 ")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果这为真，那么即使反馈操作正在进行中，调用该反馈也会触发它。如果这为假，那么在当前反馈操作结束之前，将阻止任何新的“播放（Play）”操作。  ")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("如果这是真的，那么这个组件将使用材质属性块，而不是在材质实例上进行操作。")] 
		public bool UseMaterialPropertyBlocks = false;
        
		[MMFInspectorGroup("Intensity", true, 65)]
		/// the curve to tween the offset on
		[Tooltip("用于对偏移量进行补间（过渡）的曲线 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve OffsetCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the offset curve's 0 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("用于将偏移曲线的0值重新映射到的值，该值在其最小值和最大值之间随机生成。如果你不希望有任何随机性，则在最小值和最大值中输入相同的值。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the offset curve's 1 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("将偏移曲线的 1 值重新映射后得到的值，该值会在设定的最小值和最大值之间随机确定。如果你不想要随机效果，就在最小值和最大值处设置相同的值。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapOne = Vector2.one;
		/// the value to move the intensity to in instant mode
		[Tooltip("在即时模式下要将强度调整到的值。")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Vector2 InstantOffset;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;
		protected MaterialPropertyBlock _propertyBlock;
		protected Vector4 _propertyBlockVector;

        /// 此反馈的持续时间即过渡过程的时长。
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasRandomness => true;

        /// <summary>
        /// 在初始化时，我们会存储初始的纹理偏移量。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
            
			if (UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
				_propertyBlockVector.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;
				_initialValue.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).z;
				_initialValue.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).w;    
			}
			else
			{
				_initialValue = TargetRenderer.materials[MaterialIndex].GetTextureOffset(MaterialPropertyName);    
			}
		}

        /// <summary>
        /// 在播放时，我们开始进行偏移量的更改。
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
        /// 这个协程将随时间推移修改偏移值。
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
        /// 将偏移量应用到目标材质上。
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
        /// 将指定的值应用到材质上。
        /// </summary>
        /// <param name="newValue"></param>
        protected virtual void ApplyValue(Vector2 newValue)
		{
			if (UseMaterialPropertyBlocks)
			{
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.z = newValue.x;
				_propertyBlockVector.w = newValue.y;
				_propertyBlock.SetVector(MaterialPropertyName, _propertyBlockVector);
				TargetRenderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
			}
			else
			{
				TargetRenderer.materials[MaterialIndex].SetTextureOffset(MaterialPropertyName, newValue);    
			}
		}

        /// <summary>
        /// 停止此反馈。
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

        /// <summary>
        /// 在恢复操作时，我们会恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			ApplyValue(_initialValue);
		}
	}
}