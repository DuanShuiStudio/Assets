using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the density, color, end and start distance of your scene's fog
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将使你能够为场景中雾气的密度、颜色、起始距离和结束距离设置动画效果。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Renderer/Fog")]
	public class MMF_Fog : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		#endif
		public override bool HasRandomness => true;
		public override bool HasCustomInspectors => true; 

		/// 此反馈可能采用的模式
		public enum Modes { OverTime, Instant }

		[MMFInspectorGroup("Fog", true, 24)]
		/// whether the feedback should affect the sprite renderer instantly or over a period of time
		[Tooltip("该反馈是应该立即影响精灵渲染器，还是在一段时间内产生影响 ")]
		public Modes Mode = Modes.OverTime;
		/// how long the sprite renderer should change over time
		[Tooltip("精灵渲染器随时间进行变化所应持续的时长。 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float Duration = 2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果这为真，调用该反馈将触发它，即使它正在进行中。如果为假，在当前反馈结束之前，将阻止任何新的播放操作。 ")] 
		public bool AllowAdditivePlays = false;

		[MMFInspectorGroup("Fog Density", true, 25)]
		/// whether or not to modify the fog's density
		[Tooltip("是否要修改雾气的密度")]
		public bool ModifyFogDensity = true;
		/// a curve to use to animate the fog's density over time
		[Tooltip("用于随时间对雾气密度设置动画效果的一条曲线 ")]
		public MMTweenType DensityCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's density curve zero value to
		[Tooltip("用于将雾气密度曲线的 0 值重新映射到的那个值 ")]
		public float DensityRemapZero = 0.01f;
		/// the value to remap the fog's density curve one value to
		[Tooltip("用于将雾气密度曲线的 1 值（即曲线中的最大值对应的值）重新映射到的值  ")]
		public float DensityRemapOne = 0.05f;
		/// the value to change the fog's density to when in instant mode
		[Tooltip("在即时模式下用于将雾气密度改变到的那个值 ")]
		public float DensityInstantChange;
        
		[MMFInspectorGroup("Fog Start Distance", true, 26)]
		/// whether or not to modify the fog's start distance
		[Tooltip("是否要修改雾气的起始距离")]
		public bool ModifyStartDistance = true;
		/// a curve to use to animate the fog's start distance over time
		[Tooltip("用于随时间对雾气起始距离设置动画效果的一条曲线 ")]
		public MMTweenType StartDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's start distance curve zero value to
		[Tooltip("用于将雾气起始距离曲线的 0 值重新映射到的值")]
		public float StartDistanceRemapZero = 0f;
		/// the value to remap the fog's start distance curve one value to
		[Tooltip("用于将雾气起始距离曲线的 1 值重新映射到的值 ")]
		public float StartDistanceRemapOne = 0f;
		/// the value to change the fog's start distance to when in instant mode
		[Tooltip("在即时模式下将雾气起始距离改变到的值 ")]
		public float StartDistanceInstantChange;
        
		[MMFInspectorGroup("Fog End Distance", true, 27)]
		/// whether or not to modify the fog's end distance
		[Tooltip("是否要修改雾气的结束距离")]
		public bool ModifyEndDistance = true;
		/// a curve to use to animate the fog's end distance over time
		[Tooltip("用于随时间对雾气结束距离设置动画效果的一条曲线")]
		public MMTweenType EndDistanceCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the fog's end distance curve zero value to
		[Tooltip("用于将雾气结束距离曲线的 0 值重新映射到的值")]
		public float EndDistanceRemapZero = 0f;
		/// the value to remap the fog's end distance curve one value to
		[Tooltip("用于将雾气结束距离曲线的 1 值重新映射到的值 ")]
		public float EndDistanceRemapOne = 300f;
		/// the value to change the fog's end distance to when in instant mode
		[Tooltip("在即时模式下将雾气结束距离改变为的值 ")]
		public float EndDistanceInstantChange;
        
		[MMFInspectorGroup("Fog Color", true, 28)]
		/// whether or not to modify the fog's color
		[Tooltip("是否要修改雾气的颜色")]
		public bool ModifyColor = true;
		/// the colors to apply to the sprite renderer over time
		[Tooltip("随时间应用于精灵渲染器的颜色 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public Gradient ColorOverTime;
		/// the color to move to in instant mode
		[Tooltip("在即时模式下要变换到的颜色")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public Color InstantColor;

		/// 此反馈的持续时间是精灵渲染器的持续时间，如果是即时反馈，则持续时间为0。 
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { if (Mode != Modes.Instant) { Duration = value; } } }
        
		protected Coroutine _coroutine;
		protected Color _initialColor;
		protected float _initialStartDistance;
		protected float _initialEndDistance;
		protected float _initialDensity;

		/// <summary>
		/// 在（游戏）开始运行（播放）时，我们会更改雾气的各项数值。  
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			_initialColor = RenderSettings.fogColor;
			_initialStartDistance = RenderSettings.fogStartDistance;
			_initialEndDistance = RenderSettings.fogEndDistance;
			_initialDensity = RenderSettings.fogDensity;
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			switch (Mode)
			{
				case Modes.Instant:
					if (ModifyColor)
					{
						RenderSettings.fogColor = InstantColor;
					}

					if (ModifyStartDistance)
					{
						RenderSettings.fogStartDistance = StartDistanceInstantChange;
					}

					if (ModifyEndDistance)
					{
						RenderSettings.fogEndDistance = EndDistanceInstantChange;
					}

					if (ModifyFogDensity)
					{
						RenderSettings.fogDensity = DensityInstantChange * intensityMultiplier;
					}
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(FogSequence(intensityMultiplier));
					break;
			}
		}

		/// <summary>
		/// 这个协程将修改雾气设置中的各项数值。 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator FogSequence(float intensityMultiplier)
		{
			IsPlaying = true;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetFogValues(remappedTime, intensityMultiplier);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetFogValues(FinalNormalizedTime, intensityMultiplier);    
			_coroutine = null;      
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// 在指定的时间（介于0和1之间）设置雾气的各种数值。 
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetFogValues(float time, float intensityMultiplier)
		{
			if (ModifyColor)
			{
				RenderSettings.fogColor = ColorOverTime.Evaluate(time); 
			}

			if (ModifyFogDensity)
			{
				RenderSettings.fogDensity = MMTween.Tween(time, 0f, 1f, DensityRemapZero, DensityRemapOne, DensityCurve) * intensityMultiplier;
			}

			if (ModifyStartDistance)
			{
				RenderSettings.fogStartDistance = MMTween.Tween(time, 0f, 1f, StartDistanceRemapZero, StartDistanceRemapOne, StartDistanceCurve);
			}

			if (ModifyEndDistance)
			{
				RenderSettings.fogEndDistance = MMTween.Tween(time, 0f, 1f, EndDistanceRemapZero, EndDistanceRemapOne, EndDistanceCurve);
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
		/// 在恢复时，我们将对象放回其初始位置。 
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			RenderSettings.fogColor = _initialColor;
			RenderSettings.fogStartDistance = _initialStartDistance;
			RenderSettings.fogEndDistance = _initialEndDistance;
			RenderSettings.fogDensity = _initialDensity;
		}
	}
}