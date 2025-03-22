using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此脚本添加到一个对象上，当玩家重生时，它会自动重新激活并复活。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Auto Respawn")]
	public class AutoRespawn : TopDownMonoBehaviour, Respawnable 
	{
		[Header("当玩家重生时复活")]
		/// if this is true, this object will respawn at its last position when the player revives
		[Tooltip("如果这是真的，当玩家复活时，这个对象将在它最后的位置复活")]
		public bool RespawnOnPlayerRespawn = true;
		/// if this is true, this object will be repositioned at its initial position when the player revives
		[Tooltip("如果这是真的，当玩家复活时，这个对象将被重新定位到它的初始位置")]
		public bool RepositionToInitOnPlayerRespawn = false;
		/// if this is true, all components on this object will be disabled on kill
		[Tooltip("如果这是真的，当这个对象被杀死时，它上面的所有组件都会被禁用")]
		public bool DisableAllComponentsOnKill = false;        
		/// if this is true, this gameobject will be disabled on kill
		[Tooltip("如果这是真的，当这个GameObject被杀死时，它会被禁用")]
		public bool DisableGameObjectOnKill = true;        

		[Header("Checkpoints检查点")]
		/// if this is true, the object will always respawn, whether or not it's associated to a checkpoint
		[Tooltip("如果这是真的，无论这个对象是否与检查点相关联，它都会一直复活。")]
		public bool IgnoreCheckpointsAlwaysRespawn = true;
		/// if the player respawns at these checkpoints, the object will be respawned
		[Tooltip("如果玩家在这些检查点复活，这个对象将会被复活")]
		public List<CheckPoint> AssociatedCheckpoints;

		[Header("X秒后自动复活")]
		/// if this has a value superior to 0, this object will respawn at its last position X seconds after its death
		[Tooltip("如果这个值大于0，这个对象将在它死亡后X秒在它最后的位置复活")]
		public float AutoRespawnDuration = 0f;
		/// the amount of times this object can auto respawn
		[Tooltip("这个对象可以自动复活的次数，负值表示无限次")]
		public int AutoRespawnAmount = 3;
		/// the remaining amounts of respawns (readonly, controlled by the class at runtime)
		[Tooltip("剩余的复活次数（只读，由运行时的类控制）")]
		[MMReadOnly]
		public int AutoRespawnRemainingAmount = 3;
		/// the effect to instantiate when the player respawns
		[Tooltip("当玩家复活时要实例化的效果")]
		public GameObject RespawnEffect;
		/// the sfx to play when the player respawns
		[Tooltip("当玩家复活时要播放的SFX（音效/视觉效果）")]
		public AudioClip RespawnSfx;

		[FormerlySerializedAs("OnRespawn")]
		[Header("Events事件")] 
		/// a Unity Event to trigger when respawning
		[Tooltip("当复活时触发的Unity事件")]
		public UnityEvent OnReviveEvent;

        // 复活
        public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

		protected MonoBehaviour[] _otherComponents;
		protected Collider2D _collider2D;
		protected Renderer _renderer;
		protected Character _character;
		protected Health _health;
		protected bool _reviving = false;
		protected float _timeOfDeath = 0f;
		protected bool _firstRespawn = true;
		protected Vector3 _initialPosition;
		protected AIBrain _aiBrain;

        /// <summary>
        /// 开始时，我们获取我们的各种组件
        /// </summary>
        protected virtual void Start()
		{
			AutoRespawnRemainingAmount = AutoRespawnAmount;
			_otherComponents = this.gameObject.GetComponents<MonoBehaviour>() ;
			_collider2D = this.gameObject.GetComponent<Collider2D> ();
			_renderer = this.gameObject.GetComponent<Renderer> ();
			_character = this.gameObject.GetComponent<Character>();
			if (_character != null)
			{
				_health = _character.CharacterHealth;
			}
			_aiBrain = this.gameObject.GetComponent<AIBrain>();
			if ((_aiBrain == null) && (_character != null))
			{
				_aiBrain = _character.CharacterBrain;
			}
			_initialPosition = this.transform.position;
		}

        /// <summary>
        /// 当玩家复活时，我们重新初始化这个代理
        /// </summary>
        /// <param name="checkpoint">Checkpoint.</param>
        /// <param name="player">Player.</param>
        public virtual void OnPlayerRespawn (CheckPoint checkpoint, Character player)
		{
			if (RepositionToInitOnPlayerRespawn)
			{
				this.transform.position = _initialPosition;				
			}

			if (RespawnOnPlayerRespawn)
			{
				if (_health != null)
				{
					_health.Revive();
				}
				Revive ();
			}
			AutoRespawnRemainingAmount = AutoRespawnAmount;
		}

        /// <summary>
        /// 在更新时，我们检查是否应该复活这个代理
        /// </summary>
        protected virtual void Update()
		{
			if (_reviving)
			{
				if (_timeOfDeath + AutoRespawnDuration < Time.time)
				{
					if (AutoRespawnAmount == 0)
					{
						return;
					}
					if (AutoRespawnAmount > 0)
					{
						if (AutoRespawnRemainingAmount <= 0)
						{
							return;
						}
						AutoRespawnRemainingAmount -= 1;
					}
					Revive ();
					_reviving = false;
				}
			}
		}

        /// <summary>
        /// 根据在检查器中设置的设置，杀死这个对象并关闭它的部件
        /// </summary>
        public virtual void Kill()
		{
			if (AutoRespawnDuration <= 0f)
			{
                // 对象被设置为非活动状态，以便在复活时能够重新初始化它
                if (DisableGameObjectOnKill)
				{
					gameObject.SetActive(false);	
				}
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						if (component != this)
						{
							component.enabled = false;
						}
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = false;	}
				if (_renderer != null)	{ _renderer.enabled = false; }
				_reviving = true;
				_timeOfDeath = Time.time;
			}
		}

        /// <summary>
        /// 复活这个对象，并再次打开它的部件
        /// </summary>
        public virtual void Revive()
		{
			if (AutoRespawnDuration <= 0f)
			{
                // 对象被设置为非活动状态，以便在复活时能够重新初始化它
                gameObject.SetActive(true);
			}
			else
			{
				if (DisableAllComponentsOnKill)
				{
					foreach (MonoBehaviour component in _otherComponents)
					{
						component.enabled = true;
					}
				}
				
				if (_collider2D != null) { _collider2D.enabled = true;	}
				if (_renderer != null)	{ _renderer.enabled = true; }
				InstantiateRespawnEffect ();
				PlayRespawnSound ();
			}
			if (_health != null)
			{
				_health.Revive();
			}
			if (_aiBrain != null)
			{
				_aiBrain.ResetBrain();
			}
			OnRevive?.Invoke();
			if (OnReviveEvent != null)
			{
				OnReviveEvent.Invoke();
			}
		}

        /// <summary>
        /// 在对象的位置实例化复活效果
        /// </summary>
        protected virtual void InstantiateRespawnEffect()
		{
			// instantiates the destroy effect
			if (RespawnEffect != null)
			{
				GameObject instantiatedEffect=(GameObject)Instantiate(RespawnEffect,transform.position,transform.rotation);
				instantiatedEffect.transform.localScale = transform.localScale;
			}
		}

        /// <summary>
        /// 播放复活声音
        /// </summary>
        protected virtual void PlayRespawnSound()
		{
			if (RespawnSfx != null)
			{
				MMSoundManagerSoundPlayEvent.Trigger(RespawnSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}
	}
}