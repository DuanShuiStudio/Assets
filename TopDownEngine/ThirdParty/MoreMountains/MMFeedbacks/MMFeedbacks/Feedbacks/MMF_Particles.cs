using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈在播放时将播放相关联的粒子系统，并在停止时将其停止
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈在播放时将简单地播放指定的粒子系统（来自你的场景）")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Particles/Particles Play")]
	public class MMF_Particles : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类型的反馈
        public static bool FeedbackTypeAuthorized = true;
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundParticleSystem = FindAutomatedTarget<ParticleSystem>();

#if UNITY_EDITOR
        /// 在检查器中设置此反馈的颜色
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundParticleSystem == null); }
		public override string RequiredTargetText { get { return BoundParticleSystem != null ? BoundParticleSystem.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个绑定的粒子系统（BoundParticleSystem） 才能正常工作。你可以在下面设置一个"; } }
		#endif
		
		public enum Modes { Play, Stop, Pause, Emit }

		[MMFInspectorGroup("Bound Particles", true, 41, true)]
		/// whether to Play, Stop or Pause the target particle system when that feedback is played
		[Tooltip("在播放该反馈时，是播放、停止还是暂停目标粒子系统")]
		public Modes Mode = Modes.Play;
		/// in Emit mode, the amount of particles per emit
		[Tooltip("在发射模式（Emit mode）下，每次发射的粒子数量")]
		[MMFEnumCondition("Mode", (int)Modes.Emit)]
		public int EmitCount = 100;
		/// the particle system to play with this feedback
		[Tooltip("与此反馈一起播放的粒子系统")]
		public ParticleSystem BoundParticleSystem;
		/// a list of (optional) particle systems 
		[Tooltip("一个（可选的）粒子系统列表")]
		public List<ParticleSystem> RandomParticleSystems;
		/// if this is true, the particles will be moved to the position passed in parameters
		[Tooltip("如果这是真的，粒子将被移动到参数中传递的位置")]
		public bool MoveToPosition = false;
		/// if this is true, the particle system's object will be set active on play
		[Tooltip("如果这是真的，粒子系统的对象将在播放时被设置为激活状态")]
		public bool ActivateOnPlay = false;
		/// if this is true, the particle system will be stopped on initialization
		[Tooltip("如果这是真的，粒子系统将在初始化时被停止")]
		public bool StopSystemOnInit = true;
		/// if this is true, the particle system will be stopped on reset
		[Tooltip("如果这是真的，粒子系统将在重置时被停止")]
		public bool StopSystemOnReset = true;
		/// if this is true, the particle system will be stopped on feedback stop
		[Tooltip("如果这是真的，粒子系统将在反馈停止时被停止")]
		public bool StopSystemOnStopFeedback = true;

		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("播放器要考虑的持续时间。这不会影响你的粒子系统，但是一种与 MMF 播放器沟通此反馈持续时间的方式。通常你会希望它与你的实际粒子系统相匹配，并且设置它对于使此反馈与暂停保持配合工作是很有用的")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("是否强制对目标粒子系统应用特定的模拟速度")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("如果 ForceSimulationSpeed 为真，则随机化模拟速度的最小值和最大值。每次此反馈播放时，都会随机化一个新的值。")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		protected ParticleSystem.EmitParams _emitParams;

        /// <summary>
        /// 在初始化时，我们停止粒子系统
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (StopSystemOnInit)
			{
				StopParticles();
			}
		}

        /// <summary>
        /// 在播放时，我们播放粒子系统
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			PlayParticles(position);
		}

        /// <summary>
        /// 在停止时，停止粒子系统
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (StopSystemOnStopFeedback)
			{
				StopParticles();
			}
		}

        /// <summary>
        /// 在重置时，停止粒子系统
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (StopSystemOnReset)
			{
				StopParticles();
			}
		}

        /// <summary>
        /// 播放一个粒子系统
        /// </summary>
        /// <param name="position"></param>
        protected virtual void PlayParticles(Vector3 position)
		{
			if (MoveToPosition)
			{
				if (Mode != Modes.Emit)
				{
					BoundParticleSystem.transform.position = position;
					foreach (ParticleSystem system in RandomParticleSystems)
					{
						system.transform.position = position;
					}	
				}
				else
				{
					_emitParams.position = position;
				}
			}

			if (ActivateOnPlay)
			{
				BoundParticleSystem.gameObject.SetActive(true);
				foreach (ParticleSystem system in RandomParticleSystems)
				{
					system.gameObject.SetActive(true);
				}
			}

			if (RandomParticleSystems.Count > 0)
			{
				int random = Random.Range(0, RandomParticleSystems.Count);
				HandleParticleSystemAction(RandomParticleSystems[random]);
			}
			else if (BoundParticleSystem != null)
			{
				HandleParticleSystemAction(BoundParticleSystem);
			}
		}

        /// <summary>
        /// 如有必要，更改目标粒子系统的模拟速度，并对其调用指定操作
        /// </summary>
        /// <param name="targetParticleSystem"></param>
        protected virtual void HandleParticleSystemAction(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			
			switch (Mode)
			{
				case Modes.Play:
					targetParticleSystem?.Play();
					break;
				case Modes.Emit:
					_emitParams.applyShapeToPosition = true;
					targetParticleSystem.Emit(_emitParams, EmitCount);
					break;
				case Modes.Stop:
					targetParticleSystem?.Stop();
					break;
				case Modes.Pause:
					targetParticleSystem?.Pause();
					break;
			}
		}

        /// <summary>
        /// 停止所有粒子系统
        /// </summary>
        protected virtual void StopParticles()
		{
			foreach(ParticleSystem system in RandomParticleSystems)
			{
				system?.Stop();
			}
			if (BoundParticleSystem != null)
			{
				BoundParticleSystem.Stop();
			}            
		}
	}
}