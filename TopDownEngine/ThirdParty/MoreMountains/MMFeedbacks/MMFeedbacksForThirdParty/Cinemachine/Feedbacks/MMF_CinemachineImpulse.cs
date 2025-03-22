using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.FeedbacksForThirdParty
{
	[AddComponentMenu("")]
	#if MM_CINEMACHINE || MM_CINEMACHINE3
	[FeedbackPath("Camera/Cinemachine Impulse")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.Cinemachine")]
	[FeedbackHelp("这种反馈可让你触发一个虚拟相机（Cinemachine）的脉冲事件。要使此功能生效，你需要在你的相机上添加一个虚拟相机（Cinemachine）脉冲监听器。 ")]
	public class MMF_CinemachineImpulse : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检视面板中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.CameraColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif
		public override bool HasRandomness => true;

		#if MM_CINEMACHINE || MM_CINEMACHINE3
		[MMFInspectorGroup("Cinemachine Impulse", true, 28)]
		/// the impulse definition to broadcast
		[Tooltip("要广播的脉冲定义。")]
		public CinemachineImpulseDefinition m_ImpulseDefinition = new CinemachineImpulseDefinition();
		/// the velocity to apply to the impulse shake
		[Tooltip("要应用于脉冲抖动的速度。")]
		public Vector3 Velocity;
		/// whether or not to clear impulses (stopping camera shakes) when the Stop method is called on that feedback
		[Tooltip("当对该反馈调用“停止（Stop）”方法时，是否清除脉冲（停止相机抖动）。 ")]
		public bool ClearImpulseOnStop = false;
		#endif
		
		[Header("Gizmos小控件")]
		/// whether or not to draw gizmos to showcase the various distance properties of this feedback, when applicable. Dissipation distance in blue, impact radius in yellow.
		[Tooltip("在适用的情况下，是否绘制小控件以展示此反馈的各种距离属性。消散距离用蓝色表示，影响半径用黄色表示。 ")]
		public bool DrawGizmos = false;

#if MM_CINEMACHINE
		/// 此反馈的持续时间就是脉冲的持续时间。
		public override float FeedbackDuration { get { return m_ImpulseDefinition != null ? m_ImpulseDefinition.m_TimeEnvelope.Duration : 0f; } }
#elif MM_CINEMACHINE3
        /// 此反馈的持续时间就是脉冲的持续时间。
        public override float FeedbackDuration { get { return m_ImpulseDefinition != null ? m_ImpulseDefinition.TimeEnvelope.Duration : 0f; } }
		#endif

		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			CinemachineImpulseManager.Instance.IgnoreTimeScale = !InScaledTimescaleMode;
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			m_ImpulseDefinition.CreateEvent(position, Velocity * intensityMultiplier);
			#endif
		}

        /// <summary>
        /// 如有需要，停止该动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (!Active || !FeedbackTypeAuthorized || !ClearImpulseOnStop)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			CinemachineImpulseManager.Instance.Clear();
			#endif
		}

        /// <summary>
        /// 在添加反馈时，我们会初始化其虚拟相机（Cinemachine）的脉冲定义。 
        /// </summary>
        public override void OnAddFeedback()
		{
#if MM_CINEMACHINE
			// 设置反馈属性
			if (this.m_ImpulseDefinition == null)
			{
				this.m_ImpulseDefinition = new CinemachineImpulseDefinition();
			}
			this.m_ImpulseDefinition.m_RawSignal = Resources.Load<NoiseSettings>("MM_6D_Shake");
			this.Velocity = new Vector3(5f, 5f, 5f);
#elif MM_CINEMACHINE3
            // 设置反馈属性
            if (this.m_ImpulseDefinition == null)
			{
				this.m_ImpulseDefinition = new CinemachineImpulseDefinition();
			}
			this.m_ImpulseDefinition.RawSignal = Resources.Load<NoiseSettings>("MM_6D_Shake");
			this.Velocity = new Vector3(5f, 5f, 5f);
			#endif
		}

        /// <summary>
        /// 如有必要，绘制消散距离和影响距离的小控件。 
        /// </summary>
        public override void OnDrawGizmosSelectedHandler()
		{
			if (!DrawGizmos)
			{
				return;
			}
			#if MM_CINEMACHINE 
			{
				if ( (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Dissipating)
				     || (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Propagating)
				     || (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy) )
				{
					Gizmos.color = MMColors.Aqua;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.m_DissipationDistance);
				}
				if (this.m_ImpulseDefinition.m_ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy)
				{
					Gizmos.color = MMColors.ReunoYellow;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.m_ImpactRadius);
				}
			}
			#elif MM_CINEMACHINE3
			if (this.m_ImpulseDefinition != null)
			{
				if ( (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Dissipating)
					 || (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Propagating)
					 || (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy) )
				{
					Gizmos.color = MMColors.Aqua;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.DissipationDistance);
				}
				if (this.m_ImpulseDefinition.ImpulseType == CinemachineImpulseDefinition.ImpulseTypes.Legacy)
				{
					Gizmos.color = MMColors.ReunoYellow;
					Gizmos.DrawWireSphere(Owner.transform.position, this.m_ImpulseDefinition.ImpactRadius);
				}
			}
			#endif
		}

        /// <summary>
        /// 自动将一个虚拟相机（Cinemachine）脉冲监听器添加到相机上。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			MMCinemachineHelpers.AutomaticCinemachineShakersSetup(Owner, "CinemachineImpulse");
		}
	}
}