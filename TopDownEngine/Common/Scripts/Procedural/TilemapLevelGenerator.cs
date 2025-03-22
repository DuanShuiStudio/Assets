using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using  MoreMountains.Tools;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 此组件添加到您关卡中的一个空对象上，将处理生成一个独特且随机化的瓦片地图
    /// </summary>
    public class TilemapLevelGenerator : MMTilemapGenerator
	{
		[Serializable]
		public class SpawnData
		{
			public GameObject Prefab;
			public int Quantity = 1;
		}
		
		[FormerlySerializedAs("GenerateOnStart")]
		[Header("TopDown Engine Settings设置")]
		/// Whether or not this level should be generated automatically on Awake
		[Tooltip("这个关卡是否应该在 `Awake`（唤醒）阶段自动生成。 ")]
		public bool GenerateOnAwake = false;

		[Header("Bindings绑定")] 
		/// the Grid on which to work
		[Tooltip("要在其上进行操作的网格")]
		public Grid TargetGrid;
		/// the tilemap containing the walls
		[Tooltip("包含墙壁的瓦片地图")]
		public Tilemap ObstaclesTilemap; 
		/// the tilemap containing the walls' shadows
		[Tooltip("包含墙壁阴影的瓦片地图")]
		public MMTilemapShadow WallsShadowTilemap;
		/// the level manager
		[Tooltip("关卡管理器")]
		public LevelManager TargetLevelManager;

		[Header("Spawn生成")] 
		/// the object at which the player will spawn
		[Tooltip("玩家将生成的对象")]
		public Transform InitialSpawn;
		/// the exit of the level
		[Tooltip("关卡的出口")]
		public Transform Exit;
		/// the minimum distance that should separate spawn and exit.
		[Tooltip("生成点和出口之间应保持的最小距离")]
		public float MinDistanceFromSpawnToExit = 2f;

		/// a list of prefabs to spawn and their quantity
		[Tooltip("要生成的预制件列表及其数量")]
		public List<SpawnData> PrefabsToSpawn;
		/// the minimum distance from already spawned elements the prefabs should be spawned at
		[Tooltip("预制件应生成在距离已生成元素多远的最小距离处")]
		public float PrefabsSpawnMinDistance = 2f;
		
		protected const int _maxIterationsCount = 100;
		protected List<Vector3> _filledPositions;

        /// <summary>
        /// 在唤醒时，如果需要，我们生成我们的关卡
        /// </summary>
        protected virtual void Awake()
		{
			if (GenerateOnAwake)
			{
				Generate();
			}
		}

        /// <summary>
        /// 生成一个新关卡
        /// </summary>
        public override void Generate()
		{
			base.Generate();
			_filledPositions = new List<Vector3>();
			HandleWallsShadow();
			PlaceEntryAndExit();
			SpawnPrefabs();
			ResizeLevelManager();
		}

        /// <summary>
        /// 调整关卡管理器的边界以适应新关卡
        /// </summary>
        protected virtual void ResizeLevelManager()
		{
			BoxCollider2D boxCollider = TargetLevelManager.GetComponent<BoxCollider2D>();
            
			Bounds bounds = ObstaclesTilemap.localBounds;
			boxCollider.offset = bounds.center;
			boxCollider.size = new Vector2(bounds.size.x, bounds.size.y);
		}

        /// <summary>
        /// 将生成点和出口移动到空位置
        /// </summary>
        protected virtual void PlaceEntryAndExit()
		{
			UnityEngine.Random.InitState(GlobalSeed);
			int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);
            
			Vector3 spawnPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
			InitialSpawn.transform.position = spawnPosition;
			_filledPositions.Add(spawnPosition);

			Vector3 exitPosition = spawnPosition;
			int iterationsCount = 0;
            
			while ((Vector3.Distance(exitPosition, spawnPosition) < MinDistanceFromSpawnToExit) && (iterationsCount < _maxIterationsCount))
			{
				exitPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
				Exit.transform.position = exitPosition;
				iterationsCount++;
			}
			_filledPositions.Add(Exit.transform.position);
		}

        /// <summary>
        /// 生成在 PrefabsToSpawn 列表中定义的预制件
        /// </summary>
        protected virtual void SpawnPrefabs()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			
			
			UnityEngine.Random.InitState(GlobalSeed);
			int width = UnityEngine.Random.Range(GridWidth.x, GridWidth.y);
			int height = UnityEngine.Random.Range(GridHeight.x, GridHeight.y);

			foreach (SpawnData data in PrefabsToSpawn)
			{
				for (int i = 0; i < data.Quantity; i++)
				{
					Vector3 spawnPosition = Vector3.zero;

					bool tooClose = true;
					int iterationsCount = 0;
					
					while (tooClose && (iterationsCount < _maxIterationsCount))
					{
						spawnPosition = MMTilemap.GetRandomPosition(ObstaclesTilemap, TargetGrid, width, height, false, width * height * 2);
						
						tooClose = false;
						foreach (Vector3 filledPosition in _filledPositions)
						{
							if (Vector3.Distance(spawnPosition, filledPosition) < PrefabsSpawnMinDistance)
							{
								tooClose = true;
								break;
							}
						}
						
						iterationsCount++;
					}
					Instantiate(data.Prefab, spawnPosition, Quaternion.identity);
					_filledPositions.Add(spawnPosition);
				}
			}
		}

        /// <summary>
        /// 将 Walls 图层的内容复制到 WallsShadows 图层，以自动获得漂亮的阴影
        /// </summary>
        protected virtual void HandleWallsShadow()
		{
			if (WallsShadowTilemap != null)
			{
				WallsShadowTilemap.UpdateShadows();
			}
		}
	}    
}