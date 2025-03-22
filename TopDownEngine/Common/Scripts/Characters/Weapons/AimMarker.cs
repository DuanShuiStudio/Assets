using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于处理瞄准标记的类，（通常为圆形）视觉元素 
    /// </summary>
    public class AimMarker : TopDownMonoBehaviour
	{
        /// 瞄准标记的可能移动模式。
        public enum MovementModes { Instant, Interpolate }

		[Header("Movement移动")]
		/// The selected movement mode for this aim marker. Instant will move the marker instantly to its target, Interpolate will animate its position over time
		[Tooltip("这个瞄准标记所选择的移动模式。Instant将立即将标记移动到其目标位置，Interpolate将在一段时间内动画化其位置")]
		public MovementModes MovementMode;
		/// an offset to apply to the position of the target (useful if you want, for example, the marker to appear above the target)
		[Tooltip("应用于目标位置的偏移量（例如，如果你想让标记出现在目标上方，这将很有用）")]
		public Vector3 Offset;
		/// When in Interpolate mode, the duration of the movement animation
		[Tooltip("在插值模式下，移动动画的持续时间")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public float MovementDuration = 0.2f;
		/// When in Interpolate mode, the curve to animate the movement on
		[Tooltip("在插值模式下，用于动画化移动的曲线")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public MMTween.MMTweenCurve MovementCurve = MMTween.MMTweenCurve.EaseInCubic;
		/// When in Interpolate mode, the delay before the marker moves when changing target
		[Tooltip("在插值模式下，当改变目标时标记移动之前的延迟")]
		[MMEnumCondition("MovementMode", (int)MovementModes.Interpolate)]
		public float MovementDelay = 0f;

		[Header("Feedbacks反馈")]
		/// A feedback to play when a target is found and we didn't have one already
		[Tooltip("在找到目标且之前没有目标时播放的反馈")]
		public MMFeedbacks FirstTargetFeedback;
		/// a feedback to play when we already had a target and just found a new one
		[Tooltip("当我们已经有一个目标并找到新的目标时播放的反馈")]
		public MMFeedbacks NewTargetAssignedFeedback;
		/// a feedback to play when no more targets are found, and we just lost our last target
		[Tooltip("当我们找不到更多目标且刚刚失去最后一个目标时播放的反馈")]
		public MMFeedbacks NoMoreTargetFeedback;

		protected Transform _target;
		protected Transform _targetLastFrame = null;
		protected WaitForSeconds _movementDelayWFS;
		protected float _lastTargetChangeAt = 0f;

        /// <summary>
        /// 在Awake时，我们初始化我们的反馈和延迟
        /// </summary>
        protected virtual void Awake()
		{
			FirstTargetFeedback?.Initialization(this.gameObject);
			NewTargetAssignedFeedback?.Initialization(this.gameObject);
			NoMoreTargetFeedback?.Initialization(this.gameObject);
			if (MovementDelay > 0f)
			{
				_movementDelayWFS = new WaitForSeconds(MovementDelay);
			}
		}

        /// <summary>
        /// 在Update时，我们检查目标是否改变，并在需要时跟随它
        /// </summary>
        protected virtual void Update()
		{
			HandleTargetChange();
			FollowTarget();
			_targetLastFrame = _target;
		}

        /// <summary>
        /// 使这个对象跟随目标的位置
        /// </summary>
        protected virtual void FollowTarget()
		{
			if (MovementMode == MovementModes.Instant)
			{
				this.transform.position = _target.transform.position + Offset;
			}
			else
			{
				if ((_target != null) && (Time.time - _lastTargetChangeAt > MovementDuration))
				{
					this.transform.position = _target.transform.position + Offset;
				}
			}
		}

        /// <summary>
        /// 为这个瞄准标记设置新目标
        /// </summary>
        /// <param name="newTarget"></param>
        public virtual void SetTarget(Transform newTarget)
		{
			_target = newTarget;

			if (newTarget == null)
			{
				return;
			}

			this.gameObject.SetActive(true);

			if (_targetLastFrame == null)
			{
				this.transform.position = _target.transform.position + Offset;
			}

			if (MovementMode == MovementModes.Instant)
			{
				this.transform.position = _target.transform.position + Offset;
			}
			else
			{
				MMTween.MoveTransform(this, this.transform, this.transform.position, _target.transform.position + Offset, _movementDelayWFS, MovementDelay, MovementDuration, MovementCurve);
			}
		}

        /// <summary>
        /// 检查目标变化，并在需要时触发适当的方法
        /// </summary>
        protected virtual void HandleTargetChange()
		{
			if (_target == _targetLastFrame)
			{
				return;
			}

			_lastTargetChangeAt = Time.time;

			if (_target == null)
			{
				NoMoreTargets();
				return;
			}

			if (_targetLastFrame == null)
			{
				FirstTargetFound();
				return;
			}

			if ((_targetLastFrame != null) && (_target != null))
			{
				NewTargetFound();
			}
		}

        /// <summary>
        /// 当找不到更多目标且我们刚刚失去一个目标时，播放专用反馈
        /// </summary>
        protected virtual void NoMoreTargets()
		{
			NoMoreTargetFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 当找到新目标且之前没有目标时，播放专用反馈
        /// </summary>
        protected virtual void FirstTargetFound()
		{
			FirstTargetFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 当找到新目标且之前有其他目标时，播放专用反馈
        /// </summary>
        protected virtual void NewTargetFound()
		{
			NewTargetAssignedFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 隐藏这个对象
        /// </summary>
        public virtual void Disable()
		{
			this.gameObject.SetActive(false);
		}
	}
}