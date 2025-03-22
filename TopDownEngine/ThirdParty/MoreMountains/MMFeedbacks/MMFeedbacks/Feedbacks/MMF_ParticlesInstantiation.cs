using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个反馈会在播放/停止反馈时实例化一个粒子系统并播放/停止它。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("这条反馈将在开始或播放时在指定位置实例化指定的粒子系统，并且可以选择嵌套它们。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Particles/Particles Instantiation")]
	public class MMF_ParticlesInstantiation : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈。
        public static bool FeedbackTypeAuthorized = true;
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
#if UNITY_EDITOR
        ///设置此反馈在检查器中的颜色
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.ParticlesColor; } }
		public override bool EvaluateRequiresSetup() { return (ParticlesPrefab == null); }
		public override string RequiredTargetText { get { return ParticlesPrefab != null ? ParticlesPrefab.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a ParticlesPrefab be set to be able to work properly. You can set one below."; } }
#endif
        /// 确定实例化对象位置的不同方式：
        /// - FeedbackPosition : 对象将在反馈的位置实例化，并加上一个可选的偏移量。
        /// - Transform : 对象将在指定的Transform的位置实例化，并加上一个可选的偏移量
        /// - WorldPosition : 对象将在指定的世界位置向量实例化，并加上一个可选的偏移量
        /// - Script : 调用反馈时传递的位置参数
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }
        /// 可能的传送模式。
        /// - cached : 将缓存粒子系统的副本并重复使用它
        /// - on demand : 每次播放时都会实例化一个新的粒子系统
        public enum Modes { Cached, OnDemand, Pool }

		[MMFInspectorGroup("Particles Instantiation", true, 37, true)]
		/// whether the particle system should be cached or created on demand the first time
		[Tooltip("粒子系统应该是被缓存还是第一次按需创建")]
		public Modes Mode = Modes.Pool;
		
		/// the initial and planned size of this object pool
		[Tooltip("此对象池的初始大小和计划大小。")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("是否即使已经存在相同的预设体也要创建一个新的池")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public bool MutualizePools = false;
		/// if specified, the instantiated object (or the pool of objects) will be parented to this transform 
		[Tooltip("如果指定了，实例化的对象（或对象池）将成为此变换组件的子级 ")]
		[MMFEnumCondition("Mode", (int)Modes.Pool)]
		public Transform ParentTransform;
		
		/// if this is false, a brand new particle system will be created every time
		[Tooltip("如果此选项为假，则每次播放时都会创建一个全新的粒子系统")]
		[MMFEnumCondition("Mode", (int)Modes.OnDemand)]
		public bool CachedRecycle = true;
		/// the particle system to spawn
		[Tooltip("要生成的粒子系统")]
		public ParticleSystem ParticlesPrefab;
		/// the possible random particle systems
		[Tooltip("可能的随机粒子系统")]
		public List<ParticleSystem> RandomParticlePrefabs;
		/// if this is true, the particle system game object will be activated on Play, useful if you've somehow disabled it in a past Play
		[Tooltip("如果此选项为真，则在播放时会激活粒子系统游戏对象，这在你因某种原因禁用它之后是很有用的。\r\n\r\n")]
		public bool ForceSetActiveOnPlay = false;
		/// if this is true, the particle system will be stopped every time the feedback is reset - usually before play
		[Tooltip("如果此选项为真，则每次反馈重置时（通常在播放之前）粒子系统都会停止。\r\n\r\n")]
		public bool StopOnReset = false;
		/// the duration for the player to consider. This won't impact your particle system, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual particle system, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("供播放器考虑的持续时间。这不会影响到你的粒子系统，但是一种向MMF播放器传达这个反馈持续时间的方式。通常你会希望它与你的实际粒子系统相匹配，设置它可以让这个反馈在暂停时也能正常工作。")]
		public float DeclaredDuration = 0f;

		[MMFInspectorGroup("Position", true, 29)]
		/// the selected position mode
		[Tooltip("选定的位置模式")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the position at which to spawn this particle system
		[Tooltip("生成此粒子系统的位置")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform InstantiateParticlesPosition;
		/// the world position to move to when in WorldPosition mode 
		[Tooltip("在“世界位置”模式时，粒子系统需要移动到的世界坐标位置")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetWorldPosition;
		/// an offset to apply to the instantiation position
		[Tooltip("应用于实例化位置的偏移量")]
		public Vector3 Offset;
		/// whether or not the particle system should be nested in hierarchy or floating on its own
		[Tooltip("粒子系统是否应该在层级中嵌套或独立浮动")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform, (int)PositionModes.FeedbackPosition)]
		public bool NestParticles = true;
		/// whether or not to also apply rotation
		[Tooltip("是否也应用旋转")]
		public bool ApplyRotation = false;
		/// whether or not to also apply scale
		[Tooltip("是否也应用缩放")]
		public bool ApplyScale = false;

		[MMFInspectorGroup("Simulation Speed", true, 43, false)]
		/// whether or not to force a specific simulation speed on the target particle system(s)
		[Tooltip("是否强制对目标粒子系统采用特定的模拟速度")]
		public bool ForceSimulationSpeed = false;
		/// The min and max values at which to randomize the simulation speed, if ForceSimulationSpeed is true. A new value will be randomized every time this feedback plays
		[Tooltip("如果“强制模拟速度”为真，则随机化模拟速度的最小值和最大值。每次播放此反馈时都会随机生成一个新值")]
		[MMFCondition("ForceSimulationSpeed", true)]
		public Vector2 ForcedSimulationSpeed = new Vector2(0.1f,1f);

		protected ParticleSystem _instantiatedParticleSystem;
		protected List<ParticleSystem> _instantiatedRandomParticleSystems;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;
		protected Vector3 _scriptPosition;

        /// <summary>
        /// 在初始化时，实例化粒子系统，对其进行定位，并在需要时进行嵌套
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			if (!Active)
			{
				return;
			}
			
			CacheParticleSystem();

			CreatePools(owner);
		}
		
		protected virtual bool ShouldCache => (Mode == Modes.OnDemand && CachedRecycle) || (Mode == Modes.Cached);

		protected virtual void CreatePools(MMF_Player owner)
		{
			if (Mode != Modes.Pool)
			{
				return;
			}

			if ((ParticlesPrefab == null) && (RandomParticlePrefabs.Count == 0))
			{
				return;
			}

			if (!_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					owner.ProxyDestroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = Owner.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = ParticlesPrefab.gameObject;
				_objectPooler.PoolSize = ObjectPoolSize;
				_objectPooler.NestWaitingPool = NestParticles;
				if (ParentTransform != null)
				{
					_objectPooler.transform.SetParent(ParentTransform);
				}
				else
				{
					_objectPooler.transform.SetParent(Owner.transform);
				}
				_objectPooler.MutualizeWaitingPools = MutualizePools;
				_objectPooler.FillObjectPool();
				if ((Owner != null) && (objectPoolGo.transform.parent == null))
				{
					SceneManager.MoveGameObjectToScene(objectPoolGo, Owner.gameObject.scene);    
				}
				_poolCreatedOrFound = true;
			}
			
		}
		
		protected virtual void CacheParticleSystem()
		{
			if (!ShouldCache)
			{
				return;
			}

			InstantiateParticleSystem();
		}

        /// <summary>
        /// 实例化粒子系统
        /// </summary>
        protected virtual void InstantiateParticleSystem()
		{
			Transform newParent = null;
            
			if (NestParticles)
			{
				if (PositionMode == PositionModes.FeedbackPosition)
				{
					newParent = Owner.transform;
				}
				if (PositionMode == PositionModes.Transform)
				{
					newParent = InstantiateParticlesPosition;
				}
			}
			
			if (RandomParticlePrefabs.Count > 0)
			{
				if (ShouldCache)
				{
					_instantiatedRandomParticleSystems = new List<ParticleSystem>();
					foreach(ParticleSystem system in RandomParticlePrefabs)
					{
						ParticleSystem newSystem = GameObject.Instantiate(system, newParent) as ParticleSystem;
						if (newParent == null)
						{
							SceneManager.MoveGameObjectToScene(newSystem.gameObject, Owner.gameObject.scene);    
						}
						newSystem.Stop();
						_instantiatedRandomParticleSystems.Add(newSystem);
					}
				}
				else
				{
					int random = Random.Range(0, RandomParticlePrefabs.Count);
					_instantiatedParticleSystem = GameObject.Instantiate(RandomParticlePrefabs[random], newParent) as ParticleSystem;
					if (newParent == null)
					{
						SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
					}
				}
			}
			else
			{
				if (ParticlesPrefab == null)
				{
					return;
				}
				_instantiatedParticleSystem = GameObject.Instantiate(ParticlesPrefab, newParent) as ParticleSystem;
				_instantiatedParticleSystem.Stop();
				if (newParent == null)
				{
					SceneManager.MoveGameObjectToScene(_instantiatedParticleSystem.gameObject, Owner.gameObject.scene);    
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				PositionParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					PositionParticleSystem(system);
				}
			}
		}

		protected virtual void PositionParticleSystem(ParticleSystem system)
		{
			if (InstantiateParticlesPosition == null)
			{
				if (Owner != null)
				{
					InstantiateParticlesPosition = Owner.transform;
				}
			}

			if (system != null)
			{
				system.Stop();
				
				system.transform.position = GetPosition(Owner.transform.position);
				
				if (ApplyRotation)
				{
					system.transform.rotation = GetRotation(Owner.transform);    
				}

				if (ApplyScale)
				{
					system.transform.localScale = GetScale(Owner.transform);    
				}
            
				system.Clear();
			}
		}

        /// <summary>
        /// 获取该粒子系统的期望旋转
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Quaternion GetRotation(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.rotation;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return Owner.transform.rotation;
				default:
					return Owner.transform.rotation;
			}
		}

        /// <summary>
        /// 获取该粒子系统的期望缩放
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Vector3 GetScale(Transform target)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.localScale;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.localScale;
				case PositionModes.WorldPosition:
					return Owner.transform.localScale;
				case PositionModes.Script:
					return Owner.transform.localScale;
				default:
					return Owner.transform.localScale;
			}
		}

        /// <summary>
        /// 获取位置。
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected virtual Vector3 GetPosition(Vector3 position)
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + Offset;
				case PositionModes.Transform:
					return InstantiateParticlesPosition.position + Offset;
				case PositionModes.WorldPosition:
					return TargetWorldPosition + Offset;
				case PositionModes.Script:
					return _scriptPosition + Offset;
				default:
					return _scriptPosition + Offset;
			}
		}

        /// <summary>
        /// 在播放时，播放反馈
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_scriptPosition = position;
			
			if (Mode == Modes.Pool)
			{
				if (_objectPooler != null)
				{
					_newGameObject = _objectPooler.GetPooledGameObject();
					_instantiatedParticleSystem = _newGameObject.MMFGetComponentNoAlloc<ParticleSystem>();
					if (_instantiatedParticleSystem != null)
					{
						PositionParticleSystem(_instantiatedParticleSystem);
						_newGameObject.SetActive(true);
					}
				}
			}
			else
			{
				if (!ShouldCache)
				{
					InstantiateParticleSystem();
				}
				else
				{
					GrabCachedParticleSystem();
				}
			}
			
			if (_instantiatedParticleSystem != null)
			{
				if (ForceSetActiveOnPlay)
				{
					_instantiatedParticleSystem.gameObject.SetActive(true);
				}
				_instantiatedParticleSystem.Stop();
				_instantiatedParticleSystem.transform.position = GetPosition(position);
				PositionParticleSystem(_instantiatedParticleSystem);
				_instantiatedParticleSystem.gameObject.SetActive(true);
				PlayTargetParticleSystem(_instantiatedParticleSystem);
			}

			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
                    
					if (ForceSetActiveOnPlay)
					{
						system.gameObject.SetActive(true);
					}
					system.Stop();
					system.transform.position = GetPosition(position);
				}
				int random = Random.Range(0, _instantiatedRandomParticleSystems.Count);
				PlayTargetParticleSystem(_instantiatedRandomParticleSystems[random]);
			}
		}

        /// <summary>
        /// 如有必要，强制模拟速度，然后播放目标粒子系统
        /// </summary>
        /// <param name="targetParticleSystem"></param>
        protected virtual void PlayTargetParticleSystem(ParticleSystem targetParticleSystem)
		{
			if (ForceSimulationSpeed)
			{
				ParticleSystem.MainModule main = targetParticleSystem.main;
				main.simulationSpeed = Random.Range(ForcedSimulationSpeed.x, ForcedSimulationSpeed.y);
			}
			targetParticleSystem.Play();
		}

        /// <summary>
        /// 抓取并存储一个随机的粒子预设体
        /// </summary>
        protected virtual void GrabCachedParticleSystem()
		{
			if (RandomParticlePrefabs.Count > 0)
			{
				int random = Random.Range(0, RandomParticlePrefabs.Count);
				_instantiatedParticleSystem = _instantiatedRandomParticleSystems[random];
			}
		}

        /// <summary>
        /// 在停止时，停止反馈。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (_instantiatedParticleSystem != null)
			{
				_instantiatedParticleSystem?.Stop();
			}    
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach(ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}

        /// <summary>
        /// 在重置时，停止反馈
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (InCooldown)
			{
				return;
			}

			if (StopOnReset && (_instantiatedParticleSystem != null))
			{
				_instantiatedParticleSystem.Stop();
			}
			if ((_instantiatedRandomParticleSystems != null) && (_instantiatedRandomParticleSystems.Count > 0))
			{
				foreach (ParticleSystem system in _instantiatedRandomParticleSystems)
				{
					system.Stop();
				}
			}
		}
	}
}