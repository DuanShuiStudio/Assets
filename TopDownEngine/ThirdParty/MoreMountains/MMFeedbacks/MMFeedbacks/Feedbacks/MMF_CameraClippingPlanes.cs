using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control a camera's clipping planes over time. You'll need a MMCameraClippingPlanesShaker on your camera.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Camera/Clipping Planes")]
	[FeedbackHelp("这个反馈允许你随时间控制相机的剪辑平面。你需要在相机上添加一个MMCameraClippingPlanesShaker")]
	public class MMF_CameraClippingPlanes : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 返回反馈的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Clipping Planes", true, 52)]
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间（以秒为单位）")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在震动后重置震动器值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在震动后重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("是否添加到初始值")]
		public bool RelativeClippingPlanes = false;

		[MMFInspectorGroup("Near Plane", true, 53)]
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeNear = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapNearZero = 0.01f;
		/// the value to remap the curve's 1 to        
		[Tooltip("将曲线的1值重新映射到的值")]
		public float RemapNearOne = 6.25f;

		[MMFInspectorGroup("Far Plane", true, 54)]
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeFar = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to        
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapFarZero = 1000f;
		/// the value to remap the curve's 1 to        
		[Tooltip("将曲线的1值重新映射到的值")]
		public float RemapFarOne = 5000f;

        /// <summary>
        /// 触发相应的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			feedbacksIntensity = ComputeIntensity(feedbacksIntensity, position);
			
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne,
				RelativeClippingPlanes,
				feedbacksIntensity, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 在停止时，我们停止过渡
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne, stop: true);
		}

        /// <summary>
        /// 在恢复时，我们恢复初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMCameraClippingPlanesShakeEvent.Trigger(ShakeNear, FeedbackDuration, RemapNearZero, RemapNearOne, 
				ShakeFar, RemapFarZero, RemapFarOne, restore: true);
		}
	}
}