using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈将实例化相关对象（通常是VFX，但不一定），并可选地为它们创建一个对象池以提高性能
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈允许您在反馈的位置（加上可选的偏移量）实例化检查器中指定的对象。您还可以选择在初始化时（自动）创建一个对象池以节省性能。在这种情况下，您需要指定池大小（通常是您计划在场景中每个给定时间拥有的这些实例化对象的最大数量）。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Instantiate Object")]
	public class MMF_InstantiateObject : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 定位实例化对象的不同方式：
        /// - FeedbackPosition : 对象将在反馈的位置实例化，加上一个可选的偏移量
        /// - Transform : 对象将在指定的Transform的位置实例化，加上一个可选的偏移量
        /// - WorldPosition :对象将在指定的世界位置向量处实例化，加上一个可选的偏移量
        /// - Script : 在调用反馈时传递的位置参数
        public enum PositionModes { FeedbackPosition, Transform, WorldPosition, Script }

        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (GameObjectToInstantiate == null); }
		public override string RequiredTargetText { get { return GameObjectToInstantiate != null ? GameObjectToInstantiate.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个要实例化的游戏对象，以便正常工作。您可以在下面设置一个"; } }
		#endif

		[MMFInspectorGroup("Instantiate Object", true, 37, true)]
		/// the object to instantiate
		[Tooltip("要实例化的对象")]
		[FormerlySerializedAs("VfxToInstantiate")]
		public GameObject GameObjectToInstantiate;

		[MMFInspectorGroup("Position", true, 39)]
		/// the chosen way to position the object 
		[Tooltip("选择的定位对象的方式")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// the chosen way to position the object 
		[Tooltip("选择的定位对象的方式")]
		public bool AlsoApplyRotation = false;
		/// the chosen way to position the object 
		[Tooltip("选择的定位对象的方式")]
		public bool AlsoApplyScale = false;
		/// the transform at which to instantiate the object
		[Tooltip("实例化对象的变换")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.Transform)]
		public Transform TargetTransform;
		/// the transform at which to instantiate the object
		[Tooltip("实例化对象的变换")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.WorldPosition)]
		public Vector3 TargetPosition;
		/// the position offset at which to instantiate the object
		[Tooltip("实例化对象的位置偏移")]
		[FormerlySerializedAs("VfxPositionOffset")]
		public Vector3 PositionOffset;

		/// if this is true, instantiation position will be randomized between RandomizeMin and RandomizeMax 
		[Tooltip("如果为真，实例化位置将在RandomizeMin和RandomizeMax之间随机化")]
		public bool RandomizePosition = false;
		/// the minimum value we'll randomize our position with
		[Tooltip("我们将用来随机化位置的最小值")]
		[MMFCondition("RandomizePosition", true)]
		public Vector3 RandomizedPositionMin = Vector3.zero; 
		/// the maximum value we'll randomize our position with
		[Tooltip("我们将用来随机化位置的最大值")]
		[MMFCondition("RandomizePosition", true)]
		public Vector3 RandomizedPositionMax = Vector3.one;

		[MMFInspectorGroup("Parent", true, 47)]
		/// if specified, the instantiated object will be parented to this transform 
		[Tooltip("如果指定了，实例化的对象将成为此变换的子对象 ")]
		public Transform ParentTransform;

		[MMFInspectorGroup("Object Pool", true, 40)]
		/// whether or not we should create automatically an object pool for this object
		[Tooltip("是否应该为此对象自动创建一个对象池")]
		[FormerlySerializedAs("VfxCreateObjectPool")]
		public bool CreateObjectPool;
		/// the initial and planned size of this object pool
		[Tooltip("此对象池的初始大小和计划大小")]
		[MMFCondition("CreateObjectPool", true)]
		[FormerlySerializedAs("VfxObjectPoolSize")]
		public int ObjectPoolSize = 5;
		/// whether or not to create a new pool even if one already exists for that same prefab
		[Tooltip("是否创建一个新的池，即使对于相同的预制件已经存在一个池")]
		[MMFCondition("CreateObjectPool", true)] 
		public bool MutualizePools = false;
		/// the transform the pool of objects will be parented to
		[Tooltip("对象池将作为其子对象的变换")]
		[MMFCondition("CreateObjectPool", true)] 
		public Transform PoolParentTransform;

		protected MMMiniObjectPooler _objectPooler; 
		protected GameObject _newGameObject;
		protected bool _poolCreatedOrFound = false;
		protected Vector3 _randomizedPosition = Vector3.zero;

        /// <summary>
        /// 在初始化时，如果需要的话，我们会创建一个对象池
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);

			if (Active && CreateObjectPool && !_poolCreatedOrFound)
			{
				if (_objectPooler != null)
				{
					_objectPooler.DestroyObjectPool();
					owner.ProxyDestroy(_objectPooler.gameObject);
				}

				GameObject objectPoolGo = new GameObject();
				objectPoolGo.name = Owner.name+"_ObjectPooler";
				_objectPooler = objectPoolGo.AddComponent<MMMiniObjectPooler>();
				_objectPooler.GameObjectToPool = GameObjectToInstantiate;
				_objectPooler.PoolSize = ObjectPoolSize;
				if (PoolParentTransform != null)
				{
					_objectPooler.transform.SetParent(PoolParentTransform);
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

        /// <summary>
        /// 在播放时，我们从对象池或从头开始实例化指定的对象
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (GameObjectToInstantiate == null))
			{
				return;
			}
            
			if (_objectPooler != null)
			{
				_newGameObject = _objectPooler.GetPooledGameObject();
				if (_newGameObject != null)
				{
					PositionObject(position);
					_newGameObject.SetActive(true);
				}
			}
			else
			{
				_newGameObject = GameObject.Instantiate(GameObjectToInstantiate) as GameObject;
				if (_newGameObject != null)
				{
					SceneManager.MoveGameObjectToScene(_newGameObject, Owner.gameObject.scene);
					PositionObject(position);    
				}
			}
		}

		protected virtual void PositionObject(Vector3 position)
		{
			_newGameObject.transform.position = GetPosition(position);
			if (AlsoApplyRotation)
			{
				_newGameObject.transform.rotation = GetRotation();    
			}
			if (AlsoApplyScale)
			{
				_newGameObject.transform.localScale = GetScale();    
			}
			if (ParentTransform != null)
			{
				_newGameObject.transform.SetParent(ParentTransform);
			}
		}

        /// <summary>
        /// 获取该粒子系统的预期位置
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        protected virtual Vector3 GetPosition(Vector3 position)
		{
			if (RandomizePosition)
			{
				_randomizedPosition.x = UnityEngine.Random.Range(RandomizedPositionMin.x, RandomizedPositionMax.x);
				_randomizedPosition.y = UnityEngine.Random.Range(RandomizedPositionMin.y, RandomizedPositionMax.y);
				_randomizedPosition.z = UnityEngine.Random.Range(RandomizedPositionMin.z, RandomizedPositionMax.z);
			}
	        
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.position + PositionOffset + _randomizedPosition;
				case PositionModes.Transform:
					return TargetTransform.position + PositionOffset + _randomizedPosition;
				case PositionModes.WorldPosition:
					return TargetPosition + PositionOffset + _randomizedPosition;
				case PositionModes.Script:
					return position + PositionOffset + _randomizedPosition;
				default:
					return position + PositionOffset + _randomizedPosition;
			}
		}


        /// <summary>
        /// 获取该粒子系统的预期旋转
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Quaternion GetRotation()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.rotation;
				case PositionModes.Transform:
					return TargetTransform.rotation;
				case PositionModes.WorldPosition:
					return Quaternion.identity;
				case PositionModes.Script:
					return Owner.transform.rotation;
				default:
					return Owner.transform.rotation;
			}
		}

        /// <summary>
        /// 获取该粒子系统的预期缩放
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Vector3 GetScale()
		{
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					return Owner.transform.localScale;
				case PositionModes.Transform:
					return TargetTransform.localScale;
				case PositionModes.WorldPosition:
					return Owner.transform.localScale;
				case PositionModes.Script:
					return Owner.transform.localScale;
				default:
					return Owner.transform.localScale;
			}
		}
	}
}