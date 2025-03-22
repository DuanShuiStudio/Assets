using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个基本的近战武器类，当使用武器时会激活一个“伤害区域”
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Bomb")]
	public class Bomb : TopDownMonoBehaviour 
	{
        /// 炸弹伤害区域的形状
        public enum DamageAreaShapes { Rectangle, Circle }

		[Header("Explosion爆炸")]
		/// the delay before the bomb explodes
		[Tooltip("炸弹爆炸前的延迟")]
		public float TimeBeforeExplosion = 2f;
		/// a vfx to instantiate when the bomb explodes
		[Tooltip("当炸弹爆炸时触发的视觉特效")]
		public GameObject ExplosionEffect;
		/// a sound to play when the bomb explodes
		[Tooltip("炸弹爆炸时播放的声音")]
		public AudioClip ExplosionSfx;

		[Header("Flicker闪烁")]
		/// whether or not the sprite should flicker before explosion
		[Tooltip("精灵在爆炸前是否应该闪烁")]
		public bool FlickerSprite = true;
		/// the duration before the flicker starts
		[Tooltip("闪烁开始前的持续时间")]
		public float TimeBeforeFlicker = 1f;
		/// the name of the property that should flicker
		[Tooltip("应该闪烁的属性的名称")]
		public string MaterialPropertyName = "_Color";

		[Header("Damage Area伤害区域")]
		/// the collider of the damage area
		[Tooltip("伤害区域的碰撞体")]
		public Collider2D DamageAreaCollider;
		/// the duration of the damage area
		[Tooltip("伤害区域的持续时间")]
		public float DamageAreaActiveDuration = 1f;

		protected float _timeSinceStart;
		protected Renderer _renderer;
		protected MMPoolableObject _poolableObject;
		protected bool _flickering;
		protected bool _damageAreaActive;
		protected Color _initialColor;
		protected Color _flickerColor = new Color32(255, 20, 20, 255);
		protected MaterialPropertyBlock _propertyBlock;

        /// <summary>
        /// 启用时，我们初始化我们的炸弹
        /// </summary>
        protected virtual void OnEnable()
		{
			Initialization ();
		}

        /// <summary>
        /// 初始化炸弹
        /// </summary>
        protected virtual void Initialization()
		{
			if (DamageAreaCollider == null)
			{
				Debug.LogWarning ("这个炸弹没有与之关联的伤害区域: " + this.name + ". 你应该通过它的检查器来设置一个");
				return;
			}
			DamageAreaCollider.isTrigger = true;
			DisableDamageArea ();

			_propertyBlock = new MaterialPropertyBlock();
			_renderer = gameObject.MMGetComponentNoAlloc<Renderer> ();
			if (_renderer != null)
			{
				if (_renderer.sharedMaterial.HasProperty(MaterialPropertyName))
				{
					_initialColor = _renderer.sharedMaterial.GetColor(MaterialPropertyName);    
				}
			}

			_poolableObject = gameObject.MMGetComponentNoAlloc<MMPoolableObject> ();
			if (_poolableObject != null)
			{
				_poolableObject.LifeTime = 0;
			}

			_timeSinceStart = 0;
			_flickering = false;
			_damageAreaActive = false;
		}

        /// <summary>
        /// 在更新时，让我们的炸弹闪烁，激活伤害区域，并在需要时销毁炸弹
        /// </summary>
        protected virtual void Update()
		{
			_timeSinceStart += Time.deltaTime;
            // 闪烁
            if (_timeSinceStart >= TimeBeforeFlicker)
			{
				if (!_flickering && FlickerSprite)
				{
                    //我们让炸弹的精灵闪烁
                    if (_renderer != null)
					{
						StartCoroutine(MMImage.Flicker(_renderer,_initialColor,_flickerColor,0.05f,(TimeBeforeExplosion - TimeBeforeFlicker)));	
					}
				}
			}

            // 激活伤害区域
            if (_timeSinceStart >= TimeBeforeExplosion && !_damageAreaActive)
			{
				EnableDamageArea ();
				_renderer.enabled = false;
				InstantiateExplosionEffect ();
				PlayExplosionSound ();
				_damageAreaActive = true;
			}

			if (_timeSinceStart >= TimeBeforeExplosion + DamageAreaActiveDuration)
			{
				DestroyBomb ();
			}
		}

        /// <summary>
        /// 销毁炸弹
        /// </summary>
        protected virtual void DestroyBomb()
		{
			_renderer.enabled = true;
			_renderer.GetPropertyBlock(_propertyBlock);
			_propertyBlock.SetColor(MaterialPropertyName, _initialColor);
			_renderer.SetPropertyBlock(_propertyBlock);
			if (_poolableObject != null)
			{
				_poolableObject.Destroy ();	
			}
			else
			{
				Destroy (gameObject);
			}

		}

        /// <summary>
        /// 在炸弹的位置实例化一个视觉效果（VFX）
        /// </summary>
        protected virtual void InstantiateExplosionEffect()
		{
            // 实例化销毁效果
            if (ExplosionEffect!=null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(ExplosionEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}

        /// <summary>
        /// 在爆炸时播放声音
        /// </summary>
        protected virtual void PlayExplosionSound()
		{
			if (ExplosionSfx!=null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(ExplosionSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}

        /// <summary>
        /// 启用伤害区域
        /// </summary>
        protected virtual void EnableDamageArea()
		{
			DamageAreaCollider.enabled = true;
		}

        /// <summary>
        /// 禁用伤害区域.
        /// </summary>
        protected virtual void DisableDamageArea()
		{
			DamageAreaCollider.enabled = false;
		}
	}
}