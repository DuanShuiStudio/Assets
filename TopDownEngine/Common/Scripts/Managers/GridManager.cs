using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在您的使用CharacterGridMovement的场景中需要一个管理器。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Grid Manager")]
	public class GridManager : MMSingleton<GridManager>
	{
        /// 调试模式的可能类型
        public enum DebugDrawModes { TwoD, ThreeD }

		[Header("Grid网格")]

		/// the origin of the grid in world space
		[Tooltip("世界空间中网格的原点")]
		public Transform GridOrigin;
		/// the size of each square grid cell
		[Tooltip("每个正方形网格单元的大小")]
		public float GridUnitSize = 1f;

		[Header("Debug调试")]

		/// whether or not to draw the debug grid
		[Tooltip("是否绘制调试网格")]
		public bool DrawDebugGrid = true;
		/// the mode in which to draw the debug grid
		[MMCondition("DrawDebugGrid", true)]
		[Tooltip("绘制调试网格的模式")]
		public DebugDrawModes DebugDrawMode = DebugDrawModes.TwoD;
		/// the size (in squares of the debug grid)
		[MMCondition("DrawDebugGrid", true)]
		[Tooltip("调试网格的大小（以正方形为单位）")]
		public int DebugGridSize = 30;
		/// the color to use to draw the debug grid lines
		[MMCondition("DrawDebugGrid", true)]
		[Tooltip("用于绘制调试网格线的颜色")]
		public Color CellBorderColor = new Color(60f, 221f, 255f, 1f);
		/// the color to use to draw the debug grid cells backgrounds
		[MMCondition("DrawDebugGrid", true)]
		[Tooltip("用于绘制调试网格单元背景的颜色")]
		public Color InnerColor = new Color(60f, 221f, 255f, 0.3f);
        /// 当前被占用的所有单元列表
        [HideInInspector]
		public List<Vector3> OccupiedGridCells;
        /// 一个存储所有在网格上移动的对象注册的最后位置的字典
        [HideInInspector]
		public Dictionary<GameObject, Vector3Int> LastPositions;
        ///一个存储所有在网格上移动的对象注册的下一个位置的字典
        [HideInInspector]
		public Dictionary<GameObject, Vector3Int> NextPositions;

		protected Vector3 _newGridPosition;
		protected Vector3 _debugOrigin = Vector3.zero;
		protected Vector3 _debugDestination = Vector3.zero;
		protected Vector3Int _workCoordinate = Vector3Int.zero;

        /// <summary>
        /// 支持进入播放模式的静态初始化
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

        /// <summary>
        /// 在开始时，我们初始化我们的列表和字典
        /// </summary>
        protected virtual void Start()
		{
			OccupiedGridCells = new List<Vector3>();
			LastPositions = new Dictionary<GameObject, Vector3Int>();
			NextPositions = new Dictionary<GameObject, Vector3Int>();
		}

        /// <summary>
        /// 如果指定坐标的单元格被占用，则返回true，否则返回false
        /// </summary>
        /// <param name="cellCoordinates"></param>
        /// <returns></returns>
        public virtual bool CellIsOccupied(Vector3 cellCoordinates)
		{
			return OccupiedGridCells.Contains(cellCoordinates);
		}

        /// <summary>
        /// 将指定的单元格标记为已占用
        /// </summary>
        /// <param name="cellCoordinates"></param>
        public virtual void OccupyCell(Vector3 cellCoordinates)
		{
			if (!OccupiedGridCells.Contains(cellCoordinates))
			{
				OccupiedGridCells.Add(cellCoordinates);
			}
		}

        /// <summary>
        /// 将指定的单元格标记为未占用
        /// </summary>
        /// <param name="cellCoordinates"></param>
        public virtual void FreeCell(Vector3 cellCoordinates)
		{
			if (OccupiedGridCells.Contains(cellCoordinates))
			{
				OccupiedGridCells.Remove(cellCoordinates);
			}
		}

        /// <summary>
        /// 设置在网格上移动的指定对象的下一个位置
        /// 下一个位置是对象到达其目标网格单元时所处的位置。
        /// </summary>
        /// <param name="trackedObject"></param>
        /// <param name="cellCoordinates"></param>
        public virtual void SetNextPosition(GameObject trackedObject, Vector3Int cellCoordinates)
		{
			// we add that to our dictionary
			if (NextPositions.ContainsKey(trackedObject))
			{
				NextPositions[trackedObject] = cellCoordinates;
			}
			else
			{
				NextPositions.Add(trackedObject, cellCoordinates);
			}
		}

        /// <summary>
        /// 设置在网格上移动的指定对象的最后位置。
        /// 最后位置是对象上次经过一个完整瓦片时所处的位置
        /// </summary>
        public virtual void SetLastPosition(GameObject trackedObject, Vector3Int cellCoordinates)
		{
            // 我们将它添加到我们的字典中
            if (LastPositions.ContainsKey(trackedObject))
			{
				LastPositions[trackedObject] = cellCoordinates;

			}
			else
			{
				LastPositions.Add(trackedObject, cellCoordinates);
			}
		}

        /// <summary>
        /// 返回网格位置对应的世界坐标
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public virtual Vector3Int WorldToCellCoordinates(Vector3 worldPosition)
		{
			_newGridPosition = (worldPosition - GridOrigin.position) / GridUnitSize;

			_workCoordinate.x = Mathf.FloorToInt(_newGridPosition.x);
			_workCoordinate.y = Mathf.FloorToInt(_newGridPosition.y);
			_workCoordinate.z = Mathf.FloorToInt(_newGridPosition.z);

			return _workCoordinate;
		}

        /// <summary>
        /// 将单元格坐标转换为其世界位置
        /// </summary>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        public virtual Vector3 CellToWorldCoordinates(Vector3Int coordinates)
		{
			_newGridPosition = (Vector3)coordinates * GridUnitSize + GridOrigin.position;
			_newGridPosition += Vector3.one * (GridUnitSize / 2f);
			return _newGridPosition;
		}

        /// <summary>
        /// 返回指定世界位置向量的网格位置
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        [System.Obsolete("从TopDown Engine的v1.8版本开始，该方法已被废弃，您应该使用WorldToCellCoordinates代替")]

        public virtual Vector3 ComputeGridPosition(Vector3 targetPosition)
		{
			_newGridPosition = (targetPosition - GridOrigin.position) / GridUnitSize;
			_newGridPosition.x = MMMaths.RoundToNearestHalf(_newGridPosition.x);
			_newGridPosition.y = MMMaths.RoundToNearestHalf(_newGridPosition.y);
			_newGridPosition.z = MMMaths.RoundToNearestHalf(_newGridPosition.z);
            
			return _newGridPosition;
		}

        /// <summary>
        /// 计算以网格单位指定的Vector3的网格位置
        /// </summary>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        [System.Obsolete("从TopDown Engine的v1.8版本开始，该方法已被废弃，您应该使用WorldToCellCoordinates代替")]
		public virtual Vector3 ComputeWorldPosition(Vector3 targetPosition)
		{
			return GridOrigin.position + (targetPosition * GridUnitSize);
		}

        /// <summary>
        /// 在绘制Gizmos时，绘制一个调试网格
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (!DrawDebugGrid)
			{
				return;
			}

			Gizmos.color = CellBorderColor;

			if (DebugDrawMode == DebugDrawModes.ThreeD)
			{
				int i = -DebugGridSize;
				// 绘制线条
				while (i <= DebugGridSize)
				{
					_debugOrigin.x = GridOrigin.position.x - DebugGridSize * GridUnitSize;
					_debugOrigin.y = GridOrigin.position.y;
					_debugOrigin.z = GridOrigin.position.z + i * GridUnitSize;

					_debugDestination.x = GridOrigin.position.x + DebugGridSize * GridUnitSize;
					_debugDestination.y = GridOrigin.position.y;
					_debugDestination.z = GridOrigin.position.z + i * GridUnitSize;

					Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

					_debugOrigin.x = GridOrigin.position.x + i * GridUnitSize;
					_debugOrigin.y = GridOrigin.position.y;
					_debugOrigin.z = GridOrigin.position.z - DebugGridSize * GridUnitSize; ;

					_debugDestination.x = GridOrigin.position.x + i * GridUnitSize;
					_debugDestination.y = GridOrigin.position.y;
					_debugDestination.z = GridOrigin.position.z + DebugGridSize * GridUnitSize;

					Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

					i++;
				}

                // 绘制单元格
                Gizmos.color = InnerColor;
				for (int a = -DebugGridSize; a < DebugGridSize; a++)
				{
					for (int b = -DebugGridSize; b < DebugGridSize; b++)
					{
						if ((a%2 == 0) && (b%2 != 0))
						{
							DrawCell3D(a, b);
						}
						if ((a%2 != 0) && (b%2 == 0))
						{
							DrawCell3D(a, b);
						}
					}
				}
			}
			else
			{
				int i = -DebugGridSize;
                // 绘制线条
                while (i <= DebugGridSize)
				{
					_debugOrigin.x = GridOrigin.position.x - DebugGridSize * GridUnitSize;
					_debugOrigin.y = GridOrigin.position.y + i * GridUnitSize;
					_debugOrigin.z = GridOrigin.position.z;

					_debugDestination.x = GridOrigin.position.x + DebugGridSize * GridUnitSize;
					_debugDestination.y = GridOrigin.position.y + i * GridUnitSize;
					_debugDestination.z = GridOrigin.position.z;

					Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

					_debugOrigin.x = GridOrigin.position.x + i * GridUnitSize;
					_debugOrigin.y = GridOrigin.position.y - DebugGridSize * GridUnitSize; ;
					_debugOrigin.z = GridOrigin.position.z;

					_debugDestination.x = GridOrigin.position.x + i * GridUnitSize;
					_debugDestination.y = GridOrigin.position.y + DebugGridSize * GridUnitSize;
					_debugDestination.z = GridOrigin.position.z;

					Debug.DrawLine(_debugOrigin, _debugDestination, CellBorderColor);

					i++;
				}

                // 绘制单元格
                Gizmos.color = InnerColor;
				for (int a = -DebugGridSize; a < DebugGridSize; a++)
				{
					for (int b = -DebugGridSize; b < DebugGridSize; b++)
					{
						if ((a % 2 == 0) && (b % 2 != 0))
						{
							DrawCell2D(a, b);
						}
						if ((a % 2 != 0) && (b % 2 == 0))
						{
							DrawCell2D(a, b);
						}
					}
				}
			}
		}

        /// <summary>
        /// 绘制一个2D调试单元格
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        protected virtual void DrawCell2D(int a, int b)
		{
			_debugOrigin.x = GridOrigin.position.x + a * GridUnitSize + GridUnitSize / 2f;            
			_debugOrigin.y = GridOrigin.position.y + b * GridUnitSize + GridUnitSize / 2f;
			_debugOrigin.z = GridOrigin.position.z;
			Gizmos.DrawCube(_debugOrigin, GridUnitSize * new Vector3(1f, 1f, 0f));
		}

        /// <summary>
        /// 绘制一个3D调试单元格
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        protected virtual void DrawCell3D(int a, int b)
		{
			_debugOrigin.x = GridOrigin.position.x + a * GridUnitSize + GridUnitSize / 2f;
			_debugOrigin.y = GridOrigin.position.y;
			_debugOrigin.z = GridOrigin.position.z + b * GridUnitSize + GridUnitSize / 2f;
			Gizmos.DrawCube(_debugOrigin, GridUnitSize * new Vector3(1f, 0f, 1f));
		}
	}
}