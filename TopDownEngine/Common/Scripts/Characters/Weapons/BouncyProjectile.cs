using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 弹丸类，会在撞击墙壁时反弹而不是爆炸
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Bouncy Projectile")]
	public class BouncyProjectile : Projectile 
	{
		[Header("Bounciness Tech弹跳技术")]
		/// the length of the raycast used to detect bounces, should be proportionate to the size and speed of your projectile
		[Tooltip("用于检测反弹的射线投射的长度，应该与你的投射物的大小和速度成比例")]
		public float BounceRaycastLength = 1f;
		/// the layers you want this projectile to bounce on
		[Tooltip("你希望这个投射物反弹的层级")]
		public LayerMask BounceLayers = LayerManager.ObstaclesLayerMask;
		/// a feedback to trigger at every bounce
		[Tooltip("每次反弹时触发的反馈")]
		public MMFeedbacks BounceFeedback;

		[Header("Bounciness弹跳性")]
		/// the min and max amount of bounces (a value will be picked at random between both bounds)
		[Tooltip("最小和最大反弹次数（将在两个界限之间随机选择一个值）")]
		[MMVector("Min", "Max")]
		public Vector2Int AmountOfBounces = new Vector2Int(10,10);
		/// the min and max speed multiplier to apply at every bounce (a value will be picked at random between both bounds)
		[Tooltip("每次反弹时应用的最小和最大速度倍数（将在两个界限之间随机选择一个值）")]
		[MMVector("Min", "Max")]
		public Vector2 SpeedModifier = Vector2.one;

		protected Vector3 _positionLastFrame;
		protected Vector3 _raycastDirection;
		protected Vector3 _reflectedDirection;
		protected int _randomAmountOfBounces;
		protected int _bouncesLeft;
		protected float _randomSpeedModifier;

        /// <summary>
        /// 初始化时我们随机化我们的值，刷新我们的2D碰撞体，因为Unity有时候会有些奇怪
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_randomAmountOfBounces = Random.Range(AmountOfBounces.x, AmountOfBounces.y);
			_randomSpeedModifier = Random.Range(SpeedModifier.x, SpeedModifier.y);
			_bouncesLeft = _randomAmountOfBounces;
			if (_collider2D != null)
			{
				_collider2D.enabled = false;
				_collider2D.enabled = true;
			}            
		}

        /// <summary>
        /// 在触发进入2D时，我们调用我们的碰撞结束点
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发进入时，我们调用我们的碰撞结束点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 碰撞结束点
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!BounceLayers.MMContains(collider.layer))
			{
				return;
			}

			_raycastDirection = (this.transform.position - _positionLastFrame);

			if (BoundsBasedOn == WaysToDetermineBounds.Collider)
			{
				RaycastHit hit = MMDebug.Raycast3D(this.transform.position, Direction.normalized, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
				EvaluateHit3D(hit);
			}

			if (BoundsBasedOn == WaysToDetermineBounds.Collider2D)
			{
				RaycastHit2D hit = MMDebug.RayCast(this.transform.position, Direction.normalized, BounceRaycastLength, BounceLayers, MMColors.DarkOrange, true);
				EvaluateHit2D(hit);
			}
		}

        /// <summary>
        /// 决定我们是否应该反弹
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void EvaluateHit3D(RaycastHit hit)
		{
			if (hit.collider != null)
			{
				if (_bouncesLeft > 0)
				{
					Bounce3D(hit);
				}
				else
				{
					_health.Kill();
					_damageOnTouch.HitNonDamageableFeedback?.PlayFeedbacks();
				}
			}
		}

        /// <summary>
        /// 决定我们是否应该反弹
        /// </summary>
        protected virtual void EvaluateHit2D(RaycastHit2D hit)
		{
			if (hit)
			{
				if (_bouncesLeft > 0)
				{
					Bounce2D(hit);
				}
				else
				{
					_health.Kill();
					_damageOnTouch.HitNonDamageableFeedback?.PlayFeedbacks();
				}
			}
		}

        /// <summary>
        /// 如果我们收到一个防止2D碰撞的消息，我们检查是否应该反弹
        /// </summary>
        /// <param name="hit"></param>
        public void PreventedCollision2D(RaycastHit2D hit)
		{
			_raycastDirection = transform.position - _positionLastFrame;
			if (_health.CurrentHealth <= 0)
			{
				return;
			}
			EvaluateHit2D(hit);
		}

        /// <summary>
        /// 如果我们收到一个防止3D碰撞的消息，我们检查是否应该反弹
        /// </summary>
        /// <param name="hit"></param>
        public void PreventedCollision3D(RaycastHit hit)
		{
			_raycastDirection = transform.position - _positionLastFrame;
			if (_health.CurrentHealth <= 0)
			{
				return;
			}
			EvaluateHit3D(hit);
		}

        /// <summary>
        /// 在2D中应用反弹
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void Bounce2D(RaycastHit2D hit)
		{
			BounceFeedback?.PlayFeedbacks();
			_reflectedDirection = Vector3.Reflect(_raycastDirection, hit.normal);
			float angle = Vector3.Angle(_raycastDirection, _reflectedDirection);
			Direction = _reflectedDirection.normalized;
			this.transform.right = _spawnerIsFacingRight ? _reflectedDirection.normalized : -_reflectedDirection.normalized;
			Speed *= _randomSpeedModifier;
			_bouncesLeft--;
		}

        /// <summary>
        /// 在3D中应用反弹
        /// </summary>
        /// <param name="hit"></param>
        protected virtual void Bounce3D(RaycastHit hit)
		{
			BounceFeedback?.PlayFeedbacks();
			_reflectedDirection = Vector3.Reflect(_raycastDirection, hit.normal);
			float angle = Vector3.Angle(_raycastDirection, _reflectedDirection);
			Direction = _reflectedDirection.normalized;
			this.transform.forward = _spawnerIsFacingRight ? _reflectedDirection.normalized : -_reflectedDirection.normalized;
			Speed *= _randomSpeedModifier;
			_bouncesLeft--;
		}

        /// <summary>
        /// 在后期更新时，我们存储我们的位置
        /// </summary>
        protected virtual void LateUpdate()
		{
			_positionLastFrame = this.transform.position;
		}
	}	
}