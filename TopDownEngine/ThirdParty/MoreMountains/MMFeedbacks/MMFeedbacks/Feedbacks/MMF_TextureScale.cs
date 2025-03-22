using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the texture scale of a target material over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这个反馈机制能让你随时间推移控制目标材质的纹理缩放比例。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Texture Scale")]
	public class MMF_TextureScale : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetRenderer == null); }
		public override string RequiredTargetText { get { return TargetRenderer != null ? TargetRenderer.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetRenderer be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRenderer = FindAutomatedTarget<Renderer>();

        /// 这种反馈可能的模式。
        public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Texture Scale", true, 62, true)]
		/// the renderer on which to change texture scale on
		[Tooltip("要在其上更改纹理缩放比例的渲染器。")]
		public Renderer TargetRenderer;
		/// the material index
		[Tooltip("材质索引")]
		public int MaterialIndex = 0;
		/// the property name, for example _MainTex_ST, or _MainTex if you don't have UseMaterialPropertyBlocks set to true
		[Tooltip("属性名称，例如 _MainTex_ST；如果你没有将“使用材质属性块（UseMaterialPropertyBlocks）”设置为 true，则可以使用 _MainTex。 ")]
		public string MaterialPropertyName = "_MainTex_ST";
		/// whether the feedback should affect the material instantly or over a period of time
		[Tooltip("该反馈是应该立即对材质产生影响，还是在一段时间内逐渐产生影响。")]
		public Modes Mode = Modes.OverTime;
		/// how long the material should change over time
		[Tooltip("材质随时间变化所需的时长是多久。")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 0.2f;
		/// whether or not the values should be relative 
		[Tooltip("这些值是否应该是相对值。")]
		public bool RelativeValues = true;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此选项为真，即便该反馈正在进行中，调用它也会触发其再次执行。如果为假，则在当前反馈执行结束之前，将阻止任何新的“播放”操作。 ")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("如果此选项为真，该组件将使用材质属性块，而不是对材质的实例进行操作。")] 
		public bool UseMaterialPropertyBlocks = false;

		[MMFInspectorGroup("Intensity", true, 64)]
		/// the curve to tween the scale on
		[Tooltip("用于对缩放进行补间动画的曲线。")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public AnimationCurve ScaleCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// the value to remap the scale curve's 0 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("用于将缩放曲线的 0 值重新映射的值，该值在其最小值和最大值之间随机确定。如果你不希望有任何随机性，可在最小值和最大值处设置相同的值。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapZero = Vector2.zero;
		/// the value to remap the scale curve's 1 to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("将缩放曲线的“1”重新映射后的值，此值会在设定的最小值和最大值之间随机选取。若你不希望有随机性，可将最小值和最大值设为相同数值。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		[MMFVector("Min", "Max")]
		public Vector2 RemapOne = Vector2.one;
		/// the value to move the intensity to in instant mode
		[Tooltip("在即时模式下要将强度调整到的值。")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Vector2 InstantScale;

		protected Vector2 _initialValue;
		protected Coroutine _coroutine;
		protected Vector2 _newValue;
		protected MaterialPropertyBlock _propertyBlock;
		protected Vector4 _propertyBlockVector;

        /// 此反馈的持续时间即为过渡过程的时长。
        public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasRandomness => true;

        /// <summary>
        /// 在初始化时，我们会存储纹理缩放比例。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (UseMaterialPropertyBlocks)
			{
				_propertyBlock = new MaterialPropertyBlock();
				TargetRenderer.GetPropertyBlock(_propertyBlock);
				_propertyBlockVector.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).w;
				_propertyBlockVector.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).z;
				_initialValue.x = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
				_initialValue.y = TargetRenderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;    
			}
			else
			{
				_initialValue = TargetRenderer.materials[MaterialIndex].GetTextureScale(MaterialPropertyName);    
			}
		}

        /// <summary>
        /// 在播放时，我们会修改纹理缩放比例。
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
					ApplyValue(InstantScale * intensityMultiplier);
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
        /// 这个协程将随着时间推移对目标材质的缩放进行动画处理。
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator TransitionCo(float intensityMultiplier)
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetMaterialValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetMaterialValues(FinalNormalizedTime, intensityMultiplier);
			IsPlaying = true;
			_coroutine = null;
			yield return null;
		}

        /// <summary>
        /// 将缩放比例应用到目标材质上。
        /// </summary>
        /// <param name="time"></param>
        protected virtual void SetMaterialValues(float time, float intensityMultiplier)
		{
			_newValue.x = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.x, RemapOne.x);
			_newValue.y = MMFeedbacksHelpers.Remap(ScaleCurve.Evaluate(time), 0f, 1f, RemapZero.y, RemapOne.y);

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
				_propertyBlockVector.x = newValue.x;
				_propertyBlockVector.y = newValue.y;
				_propertyBlock.SetVector(MaterialPropertyName, _propertyBlockVector);
				TargetRenderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
			}
			else
			{
				TargetRenderer.materials[MaterialIndex].SetTextureScale(MaterialPropertyName, newValue);    
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
        /// 在恢复时，我们会恢复到初始状态。
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