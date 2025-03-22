#if MM_UI
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 这种反馈将使你能够随时间改变目标精灵渲染器的透明度（阿尔法通道值）。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈将使你能够随着时间的推移改变目标图像的透明度（阿尔法通道值）。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("UI/Image Alpha")]
	public class MMF_ImageAlpha : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundImage == null); }
		public override string RequiredTargetText { get { return BoundImage != null ? BoundImage.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个绑定图像（BoundImage）才能正常工作。你可以在下方设置一个。 "; } }
		#endif
		public override bool HasCustomInspectors => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundImage = FindAutomatedTarget<Image>();

		/// the possible modes for this feedback
		public enum Modes { OverTime, Instant, ToDestination }

		[MMFInspectorGroup("Target Image", true, 12, true)]
        
		/// the Image to affect when playing the feedback
		[Tooltip("在播放该反馈时要影响的图像。")]
		public Image BoundImage;

		[MMFInspectorGroup("Image Alpha Animation", true, 24)]
		/// whether the feedback should affect the Image instantly or over a period of time
		[Tooltip("该反馈是应该立即对图像产生影响，还是在一段时间内逐步产生影响 ")]
		public Modes Mode = Modes.OverTime;
		/// how long the Image should change over time
		[Tooltip("图像应该在多长时间内发生变化 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ToDestination)]
		public float Duration = 0.2f;
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果这为真，调用该反馈将触发它，即使它正在进行中。如果为假，在当前反馈结束之前，将阻止任何新的播放操作。 ")] 
		public bool AllowAdditivePlays = false;
		/// the alpha to move to in instant mode
		[Tooltip("在即时模式下要变换到的透明度（阿尔法值） ")]
		[MMFEnumCondition("Mode", (int)Modes.Instant)]
		public float InstantAlpha = 1f;
		/// the curve to use when interpolating towards the destination alpha
		[Tooltip("在向目标透明度（阿尔法值）进行插值时所使用的曲线 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime, (int)Modes.ToDestination)]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// the value to which the curve's 0 should be remapped
		[Tooltip("该曲线的0值应重新映射到的值 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float CurveRemapZero = 0f;
		/// the value to which the curve's 1 should be remapped
		[Tooltip("该曲线的1值应重新映射到的值 ")]
		[MMFEnumCondition("Mode", (int)Modes.OverTime)]
		public float CurveRemapOne = 1f;
		/// the alpha to aim towards when in ToDestination mode
		[Tooltip("在“至目标（ToDestination）”模式下要趋向的透明度（阿尔法值） ")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationAlpha = 1f;
		/// if this is true, the target will be disabled when this feedbacks is stopped
		[Tooltip("如果这为真，当此反馈停止时，目标对象将被禁用。 ")] 
		public bool DisableOnStop = false;

		/// 此反馈的持续时间为图像的持续时间，如果是即时反馈，则持续时间为0。 
		public override float FeedbackDuration { get { return (Mode == Modes.Instant) ? 0f : ApplyTimeMultiplier(Duration); } set { Duration = value; } }

		protected Coroutine _coroutine;
		protected Color _imageColor;
		protected Color _initialColor;
		protected float _initialAlpha;

		/// <summary>
		/// 在播放（运行）时，我们会开启我们的图像，并在需要时启动一个随时间推进的协程。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			_initialColor = BoundImage.color;
			Turn(true);
			switch (Mode)
			{
				case Modes.Instant:
					_imageColor = BoundImage.color;
					_imageColor.a = InstantAlpha;
					BoundImage.color = _imageColor;
					break;
				case Modes.OverTime:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
				case Modes.ToDestination:
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}

					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ImageSequence());
					break;
			}
		}

		/// <summary>
		/// 这个协程将修改图像上的各项数值。 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator ImageSequence()
		{
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;
			_imageColor = BoundImage.color;
			_initialAlpha = BoundImage.color.a;
			IsPlaying = true;
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float remappedTime = MMFeedbacksHelpers.Remap(journey, 0f, FeedbackDuration, 0f, 1f);

				SetAlpha(remappedTime);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}
			SetAlpha(FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

		/// <summary>
		/// 在指定的时间（介于0和1之间）设置精灵渲染器上的各种数值。 
		/// </summary>
		/// <param name="time"></param>
		protected virtual void SetAlpha(float time)
		{
			float newAlpha = 0f;
			if (Mode == Modes.OverTime)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, CurveRemapZero, CurveRemapOne, Curve);    
			}
			else if (Mode == Modes.ToDestination)
			{
				newAlpha = MMTween.Tween(time, 0f, 1f, _initialAlpha, DestinationAlpha, Curve);
			}

			_imageColor.a = newAlpha;
            
			BoundImage.color = _imageColor;
		}

		/// <summary>
		/// 在停止时关闭精灵渲染器。
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			IsPlaying = false;
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && DisableOnStop)
			{
				Turn(false);    
			}
            if (_coroutine != null)
            {
                Owner.StopCoroutine(_coroutine);        
            }			
			_coroutine = null;
		}

		/// <summary>
		/// 开启或关闭精灵渲染器。
		/// </summary>
		/// <param name="status"></param>
		protected virtual void Turn(bool status)
		{
			BoundImage.gameObject.SetActive(status);
			BoundImage.enabled = status;
		}
		
		/// <summary>
		/// 在恢复时，我们恢复到初始状态。 
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			BoundImage.color = _initialColor;
		}
	}
}
#endif