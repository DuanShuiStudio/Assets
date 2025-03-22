using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    ///一种用于在相关动画器上触发动画（布尔值、整数、浮点数或触发器）的反馈，可带随机性或不带随机性
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈将允许您将目标动画器交叉渐变到指定状态")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Animation/Animation Crossfade")]
	public class MMF_AnimationCrossfade : MMF_Feedback 
	{
        /// 一个静态布尔值，用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;

        /// 触发器可能的模式        
        public enum TriggerModes { SetTrigger, ResetTrigger }

        /// 设置值的可能方式
        public enum ValueModes { None, Constant, Random, Incremental }

        /// 为该反馈设置检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a BoundAnimator be set to be able to work properly. You can set one below."; } }
#endif

        /// 此反馈的持续时间为声明的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();
		
		public enum Modes { Seconds, Normalized }

		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("要更新其参数的动画器")]
		public Animator BoundAnimator;
		/// the list of extra animators whose parameters you want to update
		[Tooltip("要更新其参数的额外动画器列表")]
		public List<Animator> ExtraBoundAnimators;
		/// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("供播放器考虑的持续时间。这不会影响您的动画，而是向MMF播放器传达此反馈持续时间的一种方式。通常，您会希望它与实际动画匹配，并且设置它可以使此反馈与保持暂停功能协同工作")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("CrossFade", true, 16)]

		/// the name of the state towards which to transition. That's the name of the yellow or gray box in your Animator
		[Tooltip("要转换到的状态的名称。也就是你的动画器中黄色或灰色框的名称")]
		public string StateName = "NewState";
		/// the ID of the Animator layer you want the crossfade to occur on
		[Tooltip("你希望交叉渐变发生的动画器图层的ID")]
		public int Layer = -1;
		
		/// whether to specify timing data for the crossfade in seconds or in normalized (0-1) values  
		[Tooltip("是否以秒为单位或以归一化（0-1）值指定交叉渐变的时间数据")] 
		public Modes Mode = Modes.Seconds;
		
		/// in Seconds mode, the duration of the transition, in seconds 
		[Tooltip("在秒模式中，过渡的持续时间（以秒为单位）")]
		[MMFEnumCondition("Mode", (int)Modes.Seconds)]
		public float TransitionDuration = 0.1f;
		/// in Seconds mode, the offset at which to transition to, in seconds
		[Tooltip("在秒模式中，要转换到的偏移量（以秒为单位）")]
		[MMFEnumCondition("Mode", (int)Modes.Seconds)]
		public float TimeOffset = 0f;
		
		/// in Normalized mode, the duration of the transition, normalized between 0 and 1
		[Tooltip("在归一化模式中，用0到1之间的数值归一化的过渡持续时间")]
		[MMFEnumCondition("Mode", (int)Modes.Normalized)]
		public float NormalizedTransitionDuration = 0.1f;
		/// in Normalized mode, the offset at which to transition to, normalized between 0 and 1
		[Tooltip("在归一化模式中，要转换到的偏移量（用0到1之间的数值归一化）")]
		[MMFEnumCondition("Mode", (int)Modes.Normalized)]
		public float NormalizedTimeOffset = 0f;
		
		/// according to Unity's docs, 'the time of the transition, normalized'. Really nobody's sure what this does. It's optional. 
		[Tooltip("根据Unity的文档，“过渡时间，归一化”。实际上没人确定这个参数的作用。它是可选的。")]
		public float NormalizedTransitionTime = 0f;

		protected int _stateHashName;

        /// <summary>
        /// 自定义初始化
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			_stateHashName = Animator.StringToHash(StateName);
		}

        /// <summary>
        /// 在播放时，检查是否绑定了动画器并交叉渐变到指定状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("没有为 " + Owner.name + " 设置动画器");
				return;
			}

			CrossFade(BoundAnimator);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				CrossFade(animator);
			}
		}

        /// <summary>
        /// 通过固定时间或常规（归一化）调用进行交叉渐变
        /// </summary>
        /// <param name="targetAnimator"></param>
        protected virtual void CrossFade(Animator targetAnimator)
		{
			switch (Mode)
			{
				case Modes.Seconds:
					targetAnimator.CrossFadeInFixedTime(_stateHashName, TransitionDuration, Layer, TimeOffset, NormalizedTransitionTime);
					break;
				case Modes.Normalized:
					targetAnimator.CrossFade(_stateHashName, NormalizedTransitionDuration, Layer, NormalizedTimeOffset, NormalizedTransitionTime);
					break;
			}
		}
	}
}