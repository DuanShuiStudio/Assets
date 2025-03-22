using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类处理 Explodudes 演示场景中角色投掷炸弹的行为。
    /// </summary>
    public class ExplodudesWeapon : Weapon
	{
        /// <summary>
        /// 在网格上生成炸弹的可能方法有：
        /// - no grid : 在武器的世界位置上。
        /// - last cell : 武器所有者最后经过的格子
        /// - next cell : 武器所有者正在移动到的格子
        /// - closest : 将选择最接近当前移动的格子
        /// </summary>
        public enum GridSpawnMethods { NoGrid, LastCell, NextCell, Closest }

		[MMInspectorGroup("Explodudes Weapon", true, 23)]
		/// the spawn method for this weapon
		[Tooltip("demo-这个武器的生成方法")]
		public GridSpawnMethods GridSpawnMethod;
		/// the offset to apply on spawn
		[Tooltip("demo-生成时要应用的偏移量")]
		public Vector3 BombOffset;        
		/// the max amount of bombs a character can drop on screen at once
		[Tooltip("demo-一个角色在屏幕上一次能投掷的炸弹的最大数量")]
		public int MaximumAmountOfBombsAtOnce = 3;
		/// the delay before the bomb explodes
		[Tooltip("demo-炸弹爆炸前的延迟时间")]
		public float BombDelayBeforeExplosion = 3f;
		/// the amount of bombs remaining
		[MMReadOnly]
		[Tooltip("demo-剩余的炸弹数量")]
		public int RemainingBombs = 0;

		protected MMSimpleObjectPooler _objectPool;
		protected Vector3 _newSpawnWorldPosition;
		protected bool _alreadyBombed = false;
		protected Vector3 _lastBombPosition;
		protected ExplodudesBomb _bomb;
		protected WaitForSeconds _addOneRemainingBomb;

		protected Vector3 _closestLast;
		protected Vector3 _closestNext;
		protected Vector3Int _cellPosition;
		protected Vector3 _positionLastFrame;
		protected bool _hasntMoved = false;

        /// <summary>
        /// 在初始化时，我们获取我们的池并初始化我们的东西
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_objectPool = this.gameObject.GetComponent<MMSimpleObjectPooler>();
			RemainingBombs = MaximumAmountOfBombsAtOnce;
			_addOneRemainingBomb = new WaitForSeconds(BombDelayBeforeExplosion);
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 当使用武器时，我们生成一颗炸弹
        /// </summary>
        public override void ShootRequest()
		{
            // 我们故意不调用基类
            SpawnBomb();
		}

        /// <summary>
        /// 在更新时，我们存储我们的移动位置
        /// </summary>
        protected override void Update()
		{
			base.Update();
			if (_positionLastFrame != this.transform.position)
			{
				_hasntMoved = false;
			}
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 生成一颗炸弹。
        /// </summary>
        protected virtual void SpawnBomb()
		{
            // 我们决定把炸弹放在哪里
            DetermineBombSpawnPosition();

            // 如果那里已经有炸弹了，我们就退出
            if (_alreadyBombed)
			{
				if ( (_lastBombPosition == _newSpawnWorldPosition) && _hasntMoved)
				{
					return;
				}
			}

            // 如果我们没有剩余的炸弹了，我们就退出
            if (RemainingBombs <= 0)
			{
				return;
			}

            // 我们从池中获取一颗新的炸弹
            GameObject nextGameObject = _objectPool.GetPooledGameObject();
			if (nextGameObject == null)
			{
				return;
			}

            // 我们设置好炸弹并激活它
            nextGameObject.transform.position = _newSpawnWorldPosition;
			_bomb = nextGameObject.MMGetComponentNoAlloc<ExplodudesBomb>();
			_bomb.Owner = Owner.gameObject;
			_bomb.BombDelayBeforeExplosion = BombDelayBeforeExplosion;
			nextGameObject.gameObject.SetActive(true);

            // 我们失去一颗炸弹，并准备将其放回池中
            RemainingBombs--;
			StartCoroutine(AddOneRemainingBombCoroutine());

            // 我们改变我们的状态
            WeaponState.ChangeState(WeaponStates.WeaponUse);
			_alreadyBombed = true;
			_hasntMoved = true;
			_lastBombPosition = _newSpawnWorldPosition;
		}

        /// <summary>
        /// 根据检查器的设置确定炸弹应该在哪里生成
        /// </summary>
        protected virtual void DetermineBombSpawnPosition()
		{
			_newSpawnWorldPosition = this.transform.position;
			switch (GridSpawnMethod)
			{
				case GridSpawnMethods.NoGrid:
					_newSpawnWorldPosition = this.transform.position;
					break;
				case GridSpawnMethods.LastCell:
					if (GridManager.Instance.LastPositions.ContainsKey(Owner.gameObject))
					{
						_cellPosition = GridManager.Instance.LastPositions[Owner.gameObject];
						_newSpawnWorldPosition = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
					}
					break;
				case GridSpawnMethods.NextCell:
					if (GridManager.Instance.NextPositions.ContainsKey(Owner.gameObject))
					{
						_cellPosition = GridManager.Instance.NextPositions[Owner.gameObject];
						_newSpawnWorldPosition = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
					}
					break;
				case GridSpawnMethods.Closest:
					if (GridManager.Instance.LastPositions.ContainsKey(Owner.gameObject))
					{
						_cellPosition = GridManager.Instance.LastPositions[Owner.gameObject];
						_closestLast = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
					}
					if (GridManager.Instance.NextPositions.ContainsKey(Owner.gameObject))
					{
						_cellPosition = GridManager.Instance.NextPositions[Owner.gameObject];
						_closestNext = GridManager.Instance.CellToWorldCoordinates(_cellPosition);
					}

					if (Vector3.Distance(_closestLast, this.transform.position) < Vector3.Distance(_closestNext, this.transform.position))
					{
						_newSpawnWorldPosition = _closestLast;
					}
					else
					{
						_newSpawnWorldPosition = _closestNext;
					}
					break;
			}
			_newSpawnWorldPosition += BombOffset;
		}

        /// <summary>
        /// 在炸弹爆炸后，再添加另一颗炸弹以便使用
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator AddOneRemainingBombCoroutine()
		{
			yield return _addOneRemainingBomb;
			RemainingBombs++;
			RemainingBombs = Mathf.Min(RemainingBombs, MaximumAmountOfBombsAtOnce);
		}
	}
}