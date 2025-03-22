using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 一个事件类型，用于从任何类设置遮罩的新大小
    /// </summary>
    public struct MMSpriteMaskEvent
	{
		public enum MMSpriteMaskEventTypes { MoveToNewPosition, ExpandAndMoveToNewPosition, DoubleMask }

		public MMSpriteMaskEventTypes EventType;
		public Vector2 NewPosition;
		public Vector2 NewSize;
		public float Duration;
		public MMTween.MMTweenCurve Curve;

		public MMSpriteMaskEvent(MMSpriteMaskEventTypes eventType, Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			EventType = eventType;
			NewPosition = newPosition;
			NewSize = newSize;
			Duration = duration;
			Curve = curve;
		}

		static MMSpriteMaskEvent e;
		public static void Trigger(MMSpriteMaskEventTypes eventType, Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			e.EventType = eventType;
			e.NewPosition = newPosition;
			e.NewSize = newSize;
			e.Duration = duration;
			e.Curve = curve;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 这个类将自动查找场景中的精灵渲染器、粒子系统和瓦片地图，并根据检查器中设置的SpriteMaskInteraction来更改它们的设置。
    /// 使用NoMask标签在不希望自动设置的对象上
    /// </summary>
    public class MMSpriteMask : MonoBehaviour, MMEventListener<MMSpriteMaskEvent>
	{
        /// 这个遮罩可以操作的可能时间尺度。
        public enum Timescales { Scaled, Unscaled }

		[Header("Scale比例")]
		/// the scale multiplier to apply to the sprite mask
		[Tooltip("应用于精灵遮罩的比例乘数")]
		public float ScaleMultiplier = 100f;

		[Header("Auto setup自动设置")]
		/// whether or not all sprite renderers should be converted
		[Tooltip("是否所有精灵渲染器都应该被转换")]
		public bool AutomaticallySetupSpriteRenderers = true;
		/// whether or not all particle systems should be converted
		[Tooltip("是否所有粒子系统都应该被转换")]
		public bool AutomaticallySetupParticleSystems = true;
		/// whether or not all tilemaps should be converted
		[Tooltip("是否所有瓦片地图都应该被转换")]
		public bool AutomaticallySetupTilemaps = true;

		[Header("Behaviour行为")]

		/// if this is true, this mask will move when catching a sprite mask event
		[Tooltip("如果这是真的，当捕捉到一个精灵遮罩事件时，这个遮罩将会移动")]
		public bool CatchEvents = true;
		/// the timescale this mask operates on
		[Tooltip("这个遮罩操作的时间尺度")]
		public Timescales Timescale = Timescales.Unscaled;
		/// the type of interaction to apply to all renderers
		[Tooltip("应用于所有渲染器的交互类型")]
		public SpriteMaskInteraction MaskInteraction = SpriteMaskInteraction.VisibleInsideMask;

		public virtual float MaskTime { get { float time = (Timescale == Timescales.Unscaled) ? Time.unscaledTime : Time.time; return time; } }

        /// <summary>
        /// 在唤醒时，我们设置对象
        /// </summary>
        protected virtual void Start()
		{
			SetupMaskSettingsAutomatically();
		}

        /// <summary>
        /// 查找遮罩设置并更新它们
        /// </summary>
        protected virtual void SetupMaskSettingsAutomatically()
		{
			if (AutomaticallySetupSpriteRenderers)
			{
				var foundSpriteRenderers = FindObjectsOfType<SpriteRenderer>();
				if (foundSpriteRenderers.Length > 0)
				{
					foreach (SpriteRenderer renderer in foundSpriteRenderers)
					{
						if (!renderer.gameObject.CompareTag("NoMask"))
						{
							renderer.maskInteraction = MaskInteraction;
						}                        
					}
				}                
			}

			if (AutomaticallySetupTilemaps)
			{
				var foundTilemapRenderers = FindObjectsOfType<TilemapRenderer>();
				if (foundTilemapRenderers.Length > 0)
				{
					foreach (TilemapRenderer renderer in foundTilemapRenderers)
					{
						if (!renderer.gameObject.CompareTag("NoMask"))
						{
							renderer.maskInteraction = MaskInteraction;
						}
					}
				}                
			}

			if (AutomaticallySetupParticleSystems)
			{
				var foundParticleSystems = FindObjectsOfType<ParticleSystem>();
				if (foundParticleSystems.Length > 0)
				{
					foreach (ParticleSystem system in foundParticleSystems)
					{
						if (!system.gameObject.CompareTag("NoMask"))
						{
							ParticleSystemRenderer pr = system.GetComponent<ParticleSystemRenderer>();
							pr.maskInteraction = MaskInteraction;
						}                        
					}
				}
			}
		}

        /// <summary>
        /// 将遮罩移动到一个新的大小和位置，持续一定的时间并沿着一定的曲线
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        public virtual void MoveMaskTo(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			StartCoroutine(MoveMaskToCoroutine(newPosition, newSize, duration, curve));            
		}

        /// <summary>
        /// 在扩展到其原始大小/位置后，将遮罩移动到一个新的大小和位置
        /// the destination's size/position
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        public virtual void ExpandAndMoveMaskTo(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			StartCoroutine(ExpandAndMoveMaskToCoroutine(newPosition, newSize, duration, curve));
		}

		protected Vector3 _initialPosition;
		protected Vector3 _initialScale;
		protected Vector3 _newPosition;
		protected Vector3 _newScale;
		protected Vector3 _targetPosition;
		protected Vector3 _targetScale;

        /// <summary>
        /// 移动遮罩的协程
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        protected virtual IEnumerator MoveMaskToCoroutine(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			if (duration > 0)
			{
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;
				_targetPosition = ComputeTargetPosition(newPosition);
				_targetScale = ComputeTargetScale(newSize);
				float startedAt = MaskTime;

				while (MaskTime - startedAt <= duration)
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, duration, _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, duration, _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;

					yield return null;
				}
			}

			this.transform.position = ComputeTargetPosition(newPosition);
			this.transform.localScale = ComputeTargetScale(newSize);
		}

        /// <summary>
        /// 一个协程，它扩展遮罩以覆盖其当前位置和目标区域，然后调整自身大小以匹配目标大小。
        /// </summary>
        /// <param name="newPosition"></param>
        /// <param name="newSize"></param>
        /// <param name="duration"></param>
        /// <param name="curve"></param>
        /// <returns></returns>
        protected virtual IEnumerator ExpandAndMoveMaskToCoroutine(Vector2 newPosition, Vector2 newSize, float duration, MMTween.MMTweenCurve curve)
		{
			if (duration > 0)
			{
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;

				float startedAt = MaskTime;

                // 首先，我们移动到总大小和位置
                _targetScale.x = this.transform.localScale.x / 2f + Mathf.Abs((this.transform.position - (Vector3)newPosition).x) * ScaleMultiplier + ComputeTargetScale(newSize).x / 2f;
				_targetScale.y = this.transform.localScale.y / 2f + Mathf.Abs((this.transform.position - (Vector3)newPosition).y) * ScaleMultiplier + ComputeTargetScale(newSize).y / 2f;
				_targetScale.z = 1f;

				_targetPosition = (
					(this.transform.position + (Vector3.up * this.transform.localScale.y/ ScaleMultiplier / 2f) + (Vector3.left * this.transform.localScale.x/ ScaleMultiplier / 2f))
					+
					((Vector3)newPosition + (Vector3.down * newSize.y / 2f) + (Vector3.right * newSize.x / 2f))
				) / 2f;


				while (MaskTime - startedAt <= (duration / 2f))
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;
                    
					yield return null;
				}

                // 然后，我们移动到最终位置
                startedAt = MaskTime;
				_initialPosition = this.transform.position;
				_initialScale = this.transform.localScale;
				_targetPosition = ComputeTargetPosition(newPosition);
				_targetScale = ComputeTargetScale(newSize);
                
				while (MaskTime - startedAt <= duration / 2f)
				{
					float currentTime = MaskTime - startedAt;

					_newPosition = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialPosition, _targetPosition, curve);
					_newScale = MMTween.Tween(currentTime, 0f, (duration / 2f), _initialScale, _targetScale, curve);

					this.transform.position = _newPosition;
					this.transform.localScale = _newScale;
                    
					yield return null;
				}
			}

			this.transform.position = ComputeTargetPosition(newPosition);
			this.transform.localScale = ComputeTargetScale(newSize);
		}

        /// <summary>
        /// 确定遮罩的新位置
        /// </summary>
        /// <param name="newPosition"></param>
        /// <returns></returns>
        protected virtual Vector3 ComputeTargetPosition(Vector3 newPosition)
		{
			return newPosition;
		}

        /// <summary>
        /// 确定遮罩的比例
        /// </summary>
        /// <param name="newScale"></param>
        /// <returns></returns>
        protected virtual Vector3 ComputeTargetScale(Vector3 newScale)
		{
			return ScaleMultiplier * newScale;
		}

        /// <summary>
        /// 捕捉精灵遮罩事件
        /// </summary>
        /// <param name="spriteMaskEvent"></param>
        public virtual void OnMMEvent(MMSpriteMaskEvent spriteMaskEvent)
		{
			if (!CatchEvents)
			{
				return;
			}

			switch(spriteMaskEvent.EventType)
			{
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition:
					MoveMaskTo(spriteMaskEvent.NewPosition, spriteMaskEvent.NewSize, spriteMaskEvent.Duration, spriteMaskEvent.Curve);
					break;
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition:
					ExpandAndMoveMaskTo(spriteMaskEvent.NewPosition, spriteMaskEvent.NewSize, spriteMaskEvent.Duration, spriteMaskEvent.Curve);
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMSpriteMaskEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMSpriteMaskEvent>();
		}
	}
}