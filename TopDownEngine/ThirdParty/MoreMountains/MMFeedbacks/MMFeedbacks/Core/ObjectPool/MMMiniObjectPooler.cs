using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Feedbacks
{
	public class MMMiniObjectPooler : MonoBehaviour
	{
        /// 我们将实例化的游戏对象
        public GameObject GameObjectToPool;
        ///我们将添加到池中的对象数量
        public int PoolSize = 20;
        /// 如果为真，则池将根据需要自动向自身添加对象
        public bool PoolCanExpand = true;
        /// 如果此选项为真，则池在找到具有相同名称的池时，将尽量不创建新的等待池
        public bool MutualizeWaitingPools = false;
        /// 如果此选项为真，则所有等待中和激活的对象都将被重新分组到一个空的游戏对象下。否则，它们将仅位于层次结构中的顶层
        public bool NestWaitingPool = true;

        /// 此对象仅用于对池化对象进行分组
        protected GameObject _waitingPool = null;
		protected MMMiniObjectPool _objectPool;
		protected const int _initialPoolsListCapacity = 5;
        
		static List<MMMiniObjectPool> _pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);

        /// <summary>
        /// 如果需要的话，向静态列表中添加一个池化对象
        /// </summary>
        /// <param name="pool"></param>
        public static void AddPool(MMMiniObjectPool pool)
		{
			if (_pools == null)
			{
				_pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);    
			}
			if (!_pools.Contains(pool))
			{
				_pools.Add(pool);
			}
		}

        /// <summary>
        /// 从静态列表中移除一个池化对象
        /// </summary>
        /// <param name="pool"></param>
        public static void RemovePool(MMMiniObjectPool pool)
		{
			_pools?.Remove(pool);
		}

        /// <summary>
        /// 在Awake时，我们填充对象池
        /// </summary>
        protected virtual void Awake()
		{
			FillObjectPool();
		}

        /// <summary>
        /// 在Destroy时，我们从池化对象列表中移除自己
        /// </summary>
        private void OnDestroy()
		{
			if (_objectPool != null)
			{
				RemovePool(_objectPool);    
			}
		}

        /// <summary>
        /// 查找相同对象的现有池化对象，如果找到则返回它，否则返回null。
        /// </summary>
        /// <param name="objectToPool"></param>
        /// <returns></returns>
        public virtual MMMiniObjectPool ExistingPool(string poolName)
		{
			if (_pools == null)
			{
				_pools = new List<MMMiniObjectPool>(_initialPoolsListCapacity);    
			}
            
			if (_pools.Count == 0)
			{
				var pools = FindObjectsOfType<MMMiniObjectPool>();
				if (pools.Length > 0)
				{
					_pools.AddRange(pools);
				}
			}
			foreach (MMMiniObjectPool pool in _pools)
			{
				if ((pool != null) && (pool.name == poolName)/* && (pool.gameObject.scene == this.gameObject.scene)*/)
				{
					return pool;
				}
			}
			return null;
		}

        /// <summary>
        /// 创建等待池，或者如果已有可用的则尝试重用一个
        /// </summary>
        protected virtual void CreateWaitingPool()
		{
			if (!MutualizeWaitingPools)
			{
                // 我们创建一个容器，用于存放我们创建的所有实例。
                _objectPool = this.gameObject.AddComponent<MMMiniObjectPool>();
				_objectPool.PooledGameObjects = new List<GameObject>();
				return;
			}
			else
			{
				MMMiniObjectPool waitingPool = ExistingPool(DetermineObjectPoolName(GameObjectToPool));
                
				if (waitingPool != null)
				{
					_waitingPool = waitingPool.gameObject;
					_objectPool = waitingPool;
				}
				else
				{
					GameObject newPool = new GameObject();
					newPool.name = DetermineObjectPoolName(GameObjectToPool);
					SceneManager.MoveGameObjectToScene(newPool, this.gameObject.scene);
					_objectPool = newPool.AddComponent<MMMiniObjectPool>();
					_objectPool.PooledGameObjects = new List<GameObject>();
					AddPool(_objectPool);
				}
			}
		}

        /// <summary>
        /// 确定对象池的名称。
        /// </summary>
        /// <returns>The object pool name.</returns>
        public static string DetermineObjectPoolName(GameObject gameObjectToPool)
		{
			return (gameObjectToPool.name + "_pool");
		}

        /// <summary>
        /// 实现该方法以用对象填充池
        /// </summary>
        public virtual void FillObjectPool()
		{
			if (GameObjectToPool == null)
			{
				return;
			}

			CreateWaitingPool();

			int objectsToSpawn = PoolSize;

			if (_objectPool != null)
			{
				objectsToSpawn -= _objectPool.PooledGameObjects.Count;
			}

            // 我们向池中添加指定数量的对象
            for (int i = 0; i < objectsToSpawn; i++)
			{
				AddOneObjectToThePool();
			}
		}

        /// <summary>
        /// 实现这个方法以返回一个游戏对象
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public virtual GameObject GetPooledGameObject()
		{
            // 我们遍历该池，寻找一个非活动的对象
            for (int i = 0; i < _objectPool.PooledGameObjects.Count; i++)
			{
				if (!_objectPool.PooledGameObjects[i].gameObject.activeInHierarchy)
				{
                    // 如果我们找到一个非活动的对象，我们就返回它。
                    return _objectPool.PooledGameObjects[i];
				}
			}
            // 如果我们没有找到非活动的对象（即池为空），并且我们可以扩展它，我们就向池中添加一个新的对象，然后返回它。
            if (PoolCanExpand)
			{
				return AddOneObjectToThePool();
			}
            // 如果池为空且不能增长，我们就不返回任何东西
            return null;
		}

        /// <summary>
        /// 在检查器中添加一个指定类型的对象到池中。
        /// </summary>
        /// <returns>The one object to the pool.</returns>
        protected virtual GameObject AddOneObjectToThePool()
		{
			if (GameObjectToPool == null)
			{
				Debug.LogWarning("这个 " + gameObject.name + " ObjectPooler 没有定义任何 GameObjectToPool.", gameObject);
				return null;
			}
			GameObjectToPool.gameObject.SetActive(false);
			GameObject newGameObject = (GameObject)Instantiate(GameObjectToPool);
			SceneManager.MoveGameObjectToScene(newGameObject, this.gameObject.scene);
			if (NestWaitingPool)
			{
				newGameObject.transform.SetParent(_objectPool.transform);
			}
			newGameObject.name = GameObjectToPool.name + "-" + _objectPool.PooledGameObjects.Count;

			_objectPool.PooledGameObjects.Add(newGameObject);

			return newGameObject;
		}

        /// <summary>
        /// 销毁对象池
        /// </summary>
        public virtual void DestroyObjectPool()
		{
			if (_waitingPool != null)
			{
				Destroy(_waitingPool.gameObject);
			}
		}
	}


	public class MMMiniObjectPool : MonoBehaviour
	{
		[MMFReadOnly]
		public List<GameObject> PooledGameObjects;
	}
}