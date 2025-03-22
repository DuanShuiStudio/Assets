using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace  MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于生成对象（通常为物品拾取器，但不一定）的类
    /// 该生成操作可以随时由任何脚本触发，并带有自动挂钩
    /// 用于受伤或死亡后触发战利品
    /// </summary>
    public class Loot : TopDownMonoBehaviour
	{
        /// 定义战利品类型
        public enum LootModes { Unique, LootTable, LootTableScriptableObject }

		[Header("Loot Mode战利品模式")]
        /// 战利品模式 :
        /// - unique : 将允许你生成单个对象
        /// - loot table : 将允许你定义本地战利品表（仅适用于此特定敌人/对象）
        /// - loot definition : LootTable可脚本化对象 (created by 鼠标右键 > Create > MoreMountains > TopDown Engine > Loot Definition
        /// 然后，此战利品定义可以在其他战利品对象中重复使用。
        [Tooltip("the selected loot mode : - unique : 将允许你生成单个对象，  - loot table : 将允许你定义本地战利品表（仅适用于此特定敌人/对象）， - loot definition : LootTable可自定义对象 (created by 鼠标右键 > Create > MoreMountains > TopDown Engine > Loot Definition. 此战利品定义可以在其他战利品对象中重复使用.")]
		public LootModes LootMode = LootModes.Unique;

		/// the object to loot, when in LootMode
		[Tooltip("处于LootMode时的战利品对象")]
		[MMEnumCondition("LootMode", (int) LootModes.Unique)]
		public GameObject GameObjectToLoot;
        
		/// a loot table defining what objects to spawn
		[Tooltip("定义要生成哪些对象的战利品表")]
		[MMEnumCondition("LootMode", (int) LootModes.LootTable)]
		public MMLootTableGameObject LootTable;
        
		/// a loot table scriptable object defining what objects to spawn
		[Tooltip("一个战利品表自定义对象，定义要生成的对象")]
		[MMEnumCondition("LootMode", (int) LootModes.LootTableScriptableObject)]
		public MMLootTableGameObjectSO LootTableSO;

		[Header("Conditions")] 
		/// if this is true, loot will happen when this object dies
		[Tooltip("如果这是真的，当这个对象死亡时，就会生成战利品")]
		public bool SpawnLootOnDeath = true;
		/// if this is true, loot will happen when this object takes damage
		[Tooltip("如果这是真的，当这个物体受到伤害时，就会生成战利品")]
		public bool SpawnLootOnDamage = false;
        
		[Header("Pooling战利品池化")] 
		/// if this is true, lootables will be pooled
		[Tooltip("如果为真，战利品表会生成池统一管理")]
		public bool PoolLoot = false;
		/// determines the size of the pool for each object in the loot table
		[Tooltip("确定战利品表中每个对象的池大小")]
		[MMCondition("PoolLoot", true)]
		public int PoolSize = 20;
		/// a unique name for this pool, has to be common between all Loot objects sharing the same loot table if you want to mutualize their pools
		[Tooltip("如果你想共享它们的池，即必须在共享同一战利品表的所有战利品对象之间通用，那么这个池必须为唯一名称")]
		[MMCondition("PoolLoot", true)]
		public string MutualizedPoolName = "";
        
		[Header("Spawn产生")] 
		/// if this is false, spawn won't happen
		[Tooltip("如果为假，不会生成战利品")]
		public bool CanSpawn = true;
		/// a delay (in seconds) to wait for before spawning loot
		[Tooltip("在生成战利品之前等待的延迟（以秒为单位）")]
		public float Delay = 0f; 
		/// the minimum and maximum quantity of objects to spawn 
		[Tooltip("要生成的对象的最小和最大数量")]
		[MMVector("Min","Max")]
		public Vector2 Quantity = Vector2.one;
		/// the position, rotation and scale objects should spawn at
		[Tooltip("应该生成对象的位置、旋转和缩放")]
		public MMSpawnAroundProperties SpawnProperties;
		/// if this is true, loot will be limited to MaximumQuantity, any new loot attempt beyond that will have no outcome. If this is false, loot is unlimited and can happen forever.
		[Tooltip("如果这是真的，战利品将被限制在最大数量，超过这个数量的任何新的战利品尝试都将没有结果。如果这是假的，战利品生成是无限的，可以永远发生")]
		public bool LimitedLootQuantity = true;
		/// The maximum quantity of objects that can be looted from this Loot object
		[Tooltip("可以从该对象中掠夺的最大对象数量")]
		[MMCondition("LimitedLootQuantity", true)]
		public int MaximumQuantity = 100;
		/// The remaining quantity of objects that can be looted from this Loot object, displayed for debug purposes 
		[Tooltip("可从此战利品对象中掠夺的剩余对象数量，显示用于调试目的")]
		[MMReadOnly]
		public int RemainingQuantity = 100;

		[Header("Collisions碰撞")] 
		/// Whether or not spawned objects should try and avoid obstacles 
		[Tooltip("生成的对象是否应尝试避开障碍物")]
		public bool AvoidObstacles = false;
        /// 碰撞检测可能运行的模式
        public enum DimensionModes { TwoD, ThreeD}
		/// whether collision detection should happen in 2D or 3D
		[Tooltip("碰撞检测应该在2D还是3D进行")]
		[MMCondition("AvoidObstacles", true)]
		public DimensionModes DimensionMode = DimensionModes.TwoD;
		/// the layer mask containing layers the spawned objects shouldn't collide with 
		[Tooltip("包含图层的层掩码，生成的对象不应与之碰撞")]
		[MMCondition("AvoidObstacles", true)]
		public LayerMask AvoidObstaclesLayerMask = LayerManager.ObstaclesLayerMask;
		/// the radius around the object within which no obstacle should be found
		[Tooltip("生成的对象多少半径内强制无障碍物")]
		[MMCondition("AvoidObstacles", true)]
		public float AvoidRadius = 0.25f;
		/// the amount of times the script should try finding another position for the loot if the last one was within an obstacle. More attempts : better results, higher cost
		[Tooltip("如果上一次尝试的位置在障碍物内，脚本应该尝试为战利品寻找另一个位置的次数。更多的尝试会带来更好的结果，但成本也会更高")]
		[MMCondition("AvoidObstacles", true)]
		public int MaxAvoidAttempts = 5;
        
		[Header("Feedback反馈")] 
		/// A MMFeedbacks to play when spawning loot. Only one feedback will play. If you want one per item, it's best to place it on the item itself, and have it play when the object gets instantiated. 
		[Tooltip("生成战利品时播放的MM反馈。如果希望每个物品都播放一次，最好将其放置在物品自身上，并在对象实例化时播放")]
		public MMFeedbacks LootFeedback;

		[Header("Debug调试")] 
		/// if this is true, gizmos will be drawn to show the shape within which loot will spawn
		[Tooltip("如果这是真的，为了使战利品生成的形状更耀眼，将绘制粒子")]
		public bool DrawGizmos = false;
		/// the amount of gizmos to draw
		[Tooltip("绘制的粒子数量")]
		public int GizmosQuantity = 1000;
		/// the color the gizmos should be drawn with
		[Tooltip("绘制粒子时应使用的颜色")]
		public Color GizmosColor = MMColors.LightGray;
		/// the size at which to draw the gizmos
		[Tooltip("绘制粒子的大小")]
		public float GimosSize = 1f;
		/// a debug button used to trigger a loot
		[Tooltip("用于触发战利品的调试按钮")]
		[MMInspectorButton("SpawnLootDebug")] 
		public bool SpawnLootButton;
        
		public static List<MMSimpleObjectPooler> SimplePoolers = new List<MMSimpleObjectPooler>();
		public static List<MMMultipleObjectPooler> MultiplePoolers = new List<MMMultipleObjectPooler>();
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			SimplePoolers = new List<MMSimpleObjectPooler>();
			MultiplePoolers = new List<MMMultipleObjectPooler>();
		}

		protected Health _health;
		protected GameObject _objectToSpawn;
		protected GameObject _spawnedObject;
		protected Vector3 _raycastOrigin;
		protected RaycastHit2D _raycastHit2D;
		protected Collider[] _overlapBox;
		protected MMSimpleObjectPooler _simplePooler;
		protected MMMultipleObjectPooler _multipleObjectPooler;

        /// <summary>
        /// 在Awake时，如果有的话，我们会获取生命值组件，并初始化我们的战利品表
        /// </summary>
        protected virtual void Awake()
		{
			_health = this.gameObject.GetComponentInParent<Health>();
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInChildren<Health>();
			}
			InitializeLootTable();
			InitializePools();
			ResetRemainingQuantity();
		}

        /// <summary>
        /// 将剩余数量重置为最大数量
        /// </summary>
        public virtual void ResetRemainingQuantity()
		{
			RemainingQuantity = MaximumQuantity;
		}

        /// <summary>
        /// 计算相关战利品表的权重
        /// </summary>
        public virtual void InitializeLootTable()
		{
			switch (LootMode)
			{
				case LootModes.LootTableScriptableObject:
					if (LootTableSO != null)
					{
						LootTableSO.ComputeWeights();
					}
					break;
				case LootModes.LootTable:
					LootTable.ComputeWeights();
					break;
			}
		}

		protected virtual void InitializePools()
		{
			if (!PoolLoot)
			{
				return;
			}

			switch (LootMode)
			{
				case LootModes.Unique:
					_simplePooler = FindSimplePooler();
					break;
				case LootModes.LootTable:
					_multipleObjectPooler = FindMultiplePooler();
					break;
				case LootModes.LootTableScriptableObject:
					_multipleObjectPooler = FindMultiplePooler();
					break;
			}
		}

		protected virtual MMSimpleObjectPooler FindSimplePooler()
		{
			foreach (MMSimpleObjectPooler simplePooler in SimplePoolers)
			{
				if (simplePooler.GameObjectToPool == GameObjectToLoot)
				{
					return simplePooler;
				}
			}
            // 如果我们没有找到一个，我们就创建一个
            GameObject newObject = new GameObject("[MMSimpleObjectPooler] "+GameObjectToLoot.name);
			MMSimpleObjectPooler pooler = newObject.AddComponent<MMSimpleObjectPooler>();
			pooler.GameObjectToPool = GameObjectToLoot;
			pooler.PoolSize = PoolSize;
			pooler.NestUnderThis = true;
			pooler.FillObjectPool();            
			pooler.Owner = SimplePoolers;
			SimplePoolers.Add(pooler);
			return pooler;
		}
        
		protected virtual MMMultipleObjectPooler FindMultiplePooler()
		{
			foreach (MMMultipleObjectPooler multiplePooler in MultiplePoolers)
			{
				if ((multiplePooler != null) && (multiplePooler.MutualizedPoolName == MutualizedPoolName)) 
				{
					return multiplePooler;
				}
			}
            // 如果我们没有找到一个，我们就创建一个
            GameObject newObject = new GameObject("[MMMultipleObjectPooler] "+MutualizedPoolName);
			MMMultipleObjectPooler pooler = newObject.AddComponent<MMMultipleObjectPooler>();
			pooler.MutualizeWaitingPools = true;
			pooler.MutualizedPoolName = MutualizedPoolName;
			pooler.NestUnderThis = true;
			pooler.Pool = new List<MMMultipleObjectPoolerObject>();
			if (LootMode == LootModes.LootTable)
			{
				foreach (MMLootGameObject loot in LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject();
					objectToPool.PoolSize = PoolSize * (int)loot.Weight;
					objectToPool.GameObjectToPool = loot.Loot;
					pooler.Pool.Add(objectToPool);
				}
			}
			else if (LootMode == LootModes.LootTableScriptableObject)
			{
				foreach (MMLootGameObject loot in LootTableSO.LootTable.ObjectsToLoot)
				{
					MMMultipleObjectPoolerObject objectToPool = new MMMultipleObjectPoolerObject
					{
						PoolSize = PoolSize * (int)loot.Weight,
						GameObjectToPool = loot.Loot
					};
					pooler.Pool.Add(objectToPool);
				}
			}
			pooler.FillObjectPool();
			pooler.Owner = MultiplePoolers;
			MultiplePoolers.Add(pooler);
			return pooler;
		}

        /// <summary>
        /// 此方法在应用延迟后（如果有的话）生成指定的战利品
        /// </summary>
        public virtual void SpawnLoot()
		{
			if (!CanSpawn)
			{
				return;
			}
			StartCoroutine(SpawnLootCo());
		}

        /// <summary>
        ///由检查器按钮调用的调试方法
        /// </summary>
        protected virtual void SpawnLootDebug()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("This debug button is only meant to be used while in Play Mode.");
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// 用于在延迟后生成战利品的协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator SpawnLootCo()
		{
			yield return MMCoroutine.WaitFor(Delay);
			int randomQuantity = Random.Range((int)Quantity.x, (int)Quantity.y + 1);
			for (int i = 0; i < randomQuantity; i++)
			{
				SpawnOneLoot();
			}
			LootFeedback?.PlayFeedbacks();
		}

		protected virtual void Spawn(GameObject gameObjectToSpawn)
		{
			if (PoolLoot)
			{
				switch (LootMode)
				{
					case LootModes.Unique:
						_spawnedObject = _simplePooler.GetPooledGameObject();
						break;
					case LootModes.LootTable: case LootModes.LootTableScriptableObject:
						_spawnedObject = _multipleObjectPooler.GetPooledGameObject();
						break;
				}
			}
			else
			{
				_spawnedObject = Instantiate(gameObjectToSpawn);    
			}
		}

        /// <summary>
        /// 生成一个单独的战利品对象，没有延迟，也不管定义的数量是多少
        /// </summary>
        public virtual void SpawnOneLoot()
		{
			_objectToSpawn = GetObject();

			if (_objectToSpawn == null)
			{
				return;
			}

			if (LimitedLootQuantity && (RemainingQuantity <= 0))
			{
				return;
			}

			Spawn(_objectToSpawn);

			if (AvoidObstacles)
			{
				bool placementOK = false;
				int amountOfAttempts = 0;
				while (!placementOK && (amountOfAttempts < MaxAvoidAttempts))
				{
					MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);
                    
					if (DimensionMode == DimensionModes.TwoD)
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_raycastHit2D = Physics2D.BoxCast(_raycastOrigin + Vector3.right * AvoidRadius, AvoidRadius * Vector2.one, 0f, Vector2.left, AvoidRadius, AvoidObstaclesLayerMask);
						if (_raycastHit2D.collider == null)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
					else
					{
						_raycastOrigin = _spawnedObject.transform.position;
						_overlapBox = Physics.OverlapBox(_raycastOrigin, Vector3.one * AvoidRadius, Quaternion.identity, AvoidObstaclesLayerMask);
                        
						if (_overlapBox.Length == 0)
						{
							placementOK = true;
						}
						else
						{
							amountOfAttempts++;
						}
					}
				}
			}
			else
			{
				MMSpawnAround.ApplySpawnAroundProperties(_spawnedObject, SpawnProperties, this.transform.position);    
			}
			if (_spawnedObject != null)
			{
				_spawnedObject.gameObject.SetActive(true);
			}
			_spawnedObject.SendMessage("OnInstantiate", SendMessageOptions.DontRequireReceiver);

			if (LimitedLootQuantity)
			{
				RemainingQuantity--;	
			}
		}

        /// <summary>
        /// 获取应该生成的对象。
        /// </summary>
        /// <returns></returns>
        protected virtual GameObject GetObject()
		{
			_objectToSpawn = null;
			switch (LootMode)
			{
				case LootModes.Unique:
					_objectToSpawn = GameObjectToLoot;
					break;
				case LootModes.LootTableScriptableObject:
					if (LootTableSO == null)
					{
						_objectToSpawn = null;
						break;
					}
					_objectToSpawn = LootTableSO.GetLoot();
					break;
				case LootModes.LootTable:
					_objectToSpawn = LootTable.GetLoot()?.Loot;
					break;
			}

			return _objectToSpawn;
		}

        /// <summary>
        /// 被击中时，如果需要的话，我们会生成战利品
        /// </summary>
        protected virtual void OnHit()
		{
			if (!SpawnLootOnDamage)
			{
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// 死亡时，如果需要的话，我们会生成战利品。
        /// </summary>
        protected virtual void OnDeath()
		{
			if (!SpawnLootOnDeath)
			{
				return;
			}

			SpawnLoot();
		}

        /// <summary>
        /// 在启用时，如果需要的话，我们开始监听死亡和被击中的事件
        /// </summary>
        protected virtual void OnEnable()
		{
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnHit += OnHit;
			}
		}

        /// <summary>
        /// 在禁用时，如果需要的话，我们停止监听死亡和被击中的事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnHit -= OnHit;
			}
		}

        /// <summary>
        /// 在绘制粒子时，我们展示对象在生成战利品时将出现的形状
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (DrawGizmos)
			{
				MMSpawnAround.DrawGizmos(SpawnProperties, this.transform.position, GizmosQuantity, GimosSize, GizmosColor);    
			}
		}

	}
}