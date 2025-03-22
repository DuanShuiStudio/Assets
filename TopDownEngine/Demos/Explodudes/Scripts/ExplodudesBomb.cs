using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在Explodudes演示场景中使用的一个类，用于处理爆炸的炸弹。
    /// </summary>
    public class ExplodudesBomb : TopDownMonoBehaviour
	{
		[Header("demo-Bindings绑定")]
		/// the model of the bomb
		[Tooltip("demo-炸弹的模型")]
		public Transform BombModel;
		/// the particle system used for the northbound explosion
		[Tooltip("demo-用于北向爆炸的粒子系统")]
		public ParticleSystem DirectedExplosionN;
		/// the particle system used for the southbound explosion
		[Tooltip("demo-用于南向爆炸的粒子系统")]
		public ParticleSystem DirectedExplosionS;
		/// the particle system used for the eastbound explosion
		[Tooltip("demo-用于东向爆炸的粒子系统")]
		public ParticleSystem DirectedExplosionE;
		/// the particle system used for the westbound explosion
		[Tooltip("demo-用于西向爆炸的粒子系统")]
		public ParticleSystem DirectedExplosionW;

		[Header("demo-Raycasts射线投射")]
		/// the offset to apply to the base of the obstacle detecting raycast
		[Tooltip("demo-应用于障碍物检测射线投射基础的偏移量")]
		public Vector3 RaycastOffset = Vector3.zero;
		/// the max distance of the raycast (should be bigger than the grid)
		[Tooltip("demo-射线投射的最大距离（应大于网格）")]
		public float MaximumRaycastDistance = 50f;
		/// the layers to consider as obstacles to the bomb's fire
		[Tooltip("demo-作为炸弹火焰障碍物的层")]
		public LayerMask ObstaclesMask = LayerManager.ObstaclesLayerMask;
		/// the layers to apply damage to
		[Tooltip("demo-要施加伤害的层")]
		public LayerMask DamageLayerMask;
		/// a small offset to apply to the raycasts
		[Tooltip("demo-要应用于射线投射的一个小偏移量")]
		public float SkinWidth = 0.01f;

		[Header("demo-Bomb炸弹")]
		/// the delay (in seconds) before the bomb's explosion
		[Tooltip("demo-炸弹爆炸前的延迟（以秒为单位）")]
		public float BombDelayBeforeExplosion = 3f;
		/// the duration (in seconds) for which the bomb is active
		[Tooltip("demo-炸弹保持活跃状态的持续时间（以秒为单位）")]
		public float BombExplosionActiveDuration = 0.5f;
		/// a delay after the bomb has exploded and before it gets destroyed(in seconds)
		[Tooltip("demo-炸弹爆炸后到被销毁前的延迟（以秒为单位）")]
		public float BombAdditionalDelayBeforeDestruction = 1.5f;
		/// the damage applied by the bomb to anything with a Health component
		[Tooltip("demo-炸弹对任何具有生命值组件的物体造成的伤害")]
		public int BombDamage = 10;
		/// the distance the bomb affects
		[Tooltip("demo-炸弹的影响范围（距离）")]
		public int BombDistanceInGridUnits = 3;

		[Header("demo-Feedbacks反馈信息")]
		/// the feedbacks to play when the bomb explodes
		[Tooltip("demo-炸弹爆炸时要播放的反馈信息")]
		public MMFeedbacks ExplosionFeedbacks;

		[Header("demo-Owner所有者")]
		/// the owner of the bomb
		[MMReadOnly]
		[Tooltip("demo-炸弹的所有者")]
		public GameObject Owner;
        
		protected BoxCollider _boxCollider;
		protected WaitForSeconds _bombDuration;
		protected WaitForSeconds _explosionDuration;
		protected WaitForSeconds _additionalDelayBeforeDestruction;

		protected RaycastHit _raycastNorth;
		protected RaycastHit _raycastSouth;
		protected RaycastHit _raycastEast;
		protected RaycastHit _raycastWest;

		protected float _obstacleNorthDistance = 0f;
		protected float _obstacleEastDistance = 0f;
		protected float _obstacleWestDistance = 0f;
		protected float _obstacleSouthDistance = 0f;

		protected DamageOnTouch _damageAreaEast;
		protected DamageOnTouch _damageAreaWest;
		protected DamageOnTouch _damageAreaNorth;
		protected DamageOnTouch _damageAreaSouth;
		protected DamageOnTouch _damageAreaCenter;

		protected Vector3 _damageAreaPosition;
		protected Vector3 _damageAreaSize;
        
		protected Coroutine _delayBeforeExplosionCoroutine;
		protected ExplodudesBomb _otherBomb;
		protected bool _exploded = false;


        /// <summary>
        /// 一开始我们初始化我们的炸弹
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 初始化炸弹
        /// </summary>
        protected virtual void Initialization()
		{
			_bombDuration = new WaitForSeconds(BombDelayBeforeExplosion);
			_explosionDuration = new WaitForSeconds(BombExplosionActiveDuration);
			_additionalDelayBeforeDestruction = new WaitForSeconds(BombAdditionalDelayBeforeDestruction);

            // 随机化模型旋转
            BombModel.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);

            // 我们获取碰撞器并使其成为触发器
            _boxCollider = this.gameObject.MMGetComponentNoAlloc<BoxCollider>();
			_boxCollider.isTrigger = true;

            // 我们创建伤害区域
            _damageAreaEast = CreateDamageArea("East");
			_damageAreaWest = CreateDamageArea("West");
			_damageAreaSouth = CreateDamageArea("South");
			_damageAreaNorth = CreateDamageArea("North");
			_damageAreaCenter = CreateDamageArea("Center");

            // 中心伤害区域设置
            _damageAreaSize.x = GridManager.Instance.GridUnitSize / 2f;
			_damageAreaSize.y = GridManager.Instance.GridUnitSize / 2f;
			_damageAreaSize.z = GridManager.Instance.GridUnitSize / 2f;

			_damageAreaPosition = this.transform.position                                
			                      + Vector3.up * GridManager.Instance.GridUnitSize / 2f;

			_damageAreaCenter.gameObject.transform.position = _damageAreaPosition;
			_damageAreaCenter.gameObject.MMFGetComponentNoAlloc<BoxCollider>().size = _damageAreaSize;

		}

        /// <summary>
        /// 重置炸弹（通常在从对象池中重新生成后进行）
        /// </summary>
        protected virtual void ResetBomb()
		{
			_exploded = false;
			_boxCollider.enabled = true;
			_boxCollider.isTrigger = true;
			BombModel.transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), 0f);
			BombModel.gameObject.SetActive(true);

            // 我们开始执行炸弹的协程
            _delayBeforeExplosionCoroutine = StartCoroutine(DelayBeforeExplosionCoroutine());
		}

        /// <summary>
        /// 创建一个定向伤害区域
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected virtual DamageOnTouch CreateDamageArea(string name)
		{
			GameObject damageAreaGameObject = new GameObject();
			damageAreaGameObject.SetActive(false);
			damageAreaGameObject.transform.SetParent(this.transform);
			damageAreaGameObject.name = "ExplodudesBombDamageArea" + name;
			damageAreaGameObject.layer = LayerMask.NameToLayer("Enemies");

			DamageOnTouch damageOnTouch = damageAreaGameObject.AddComponent<DamageOnTouch>();
			damageOnTouch.MinDamageCaused = BombDamage;
			damageOnTouch.TargetLayerMask = DamageLayerMask;
			damageOnTouch.DamageTakenEveryTime = 0;
			damageOnTouch.InvincibilityDuration = 0f;
			damageOnTouch.DamageTakenEveryTime = 10;

			BoxCollider colllider = damageAreaGameObject.AddComponent<BoxCollider>();
			colllider.isTrigger = true;
            
			return damageOnTouch;
		}

        /// <summary>
        /// 在炸弹爆炸前应用一个延迟
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DelayBeforeExplosionCoroutine()
		{
			yield return _bombDuration;

			Detonate();
		}

        /// <summary>
        /// 使炸弹爆炸
        /// </summary>
        public virtual void Detonate()
		{
			if (_exploded)
			{
				return;
			}
			StartCoroutine(DetonateCoroutine());
		}

        /// <summary>
        /// 使炸弹爆炸并实例化伤害区域
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DetonateCoroutine()
		{
			_exploded = true;
			_boxCollider.enabled = false;
			StopCoroutine(_delayBeforeExplosionCoroutine);

            // 我们向所有方向发射光线（射线）
            CastRays();

            // 我们添加伤害区域
            DirectedExplosion(_raycastEast, _damageAreaEast, DirectedExplosionE, 90f);
			DirectedExplosion(_raycastWest, _damageAreaWest, DirectedExplosionW, 90f);
			DirectedExplosion(_raycastNorth, _damageAreaNorth, DirectedExplosionN, 0f);
			DirectedExplosion(_raycastSouth, _damageAreaSouth, DirectedExplosionS, 0f);
			_damageAreaCenter.gameObject.SetActive(true);
			ExplosionFeedbacks?.PlayFeedbacks();
			BombModel.gameObject.SetActive(false);

			yield return _explosionDuration;
			_damageAreaEast.gameObject.SetActive(false);
			_damageAreaWest.gameObject.SetActive(false);
			_damageAreaNorth.gameObject.SetActive(false);
			_damageAreaSouth.gameObject.SetActive(false);
			_damageAreaCenter.gameObject.SetActive(false);

			yield return _additionalDelayBeforeDestruction;
			this.gameObject.SetActive(false);
		}

        /// <summary>
        /// 向四个方向之一爆炸
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="damageArea"></param>
        /// <param name="explosion"></param>
        /// <param name="angle"></param>
        protected virtual void DirectedExplosion(RaycastHit hit, DamageOnTouch damageArea, ParticleSystem explosion, float angle)
		{
			float hitDistance = hit.distance;

            // 如果我们发现的目标具有生命值组件，它将被摧毁并需要被爆炸覆盖
            if (hit.collider.gameObject.MMFGetComponentNoAlloc<Health>() != null)
			{
				hitDistance += GridManager.Instance.GridUnitSize;
			}

            // 如果我们发现的目标具有炸弹组件，它将被摧毁并需要被爆炸覆盖
            _otherBomb = hit.collider.gameObject.MMFGetComponentNoAlloc<ExplodudesBomb>();
			if ((_otherBomb != null) && (hitDistance <= BombDistanceInGridUnits))
			{
				hitDistance += GridManager.Instance.GridUnitSize;
                // 我们引爆另一个炸弹
                _otherBomb.Detonate();
			}

            // 如果我们与障碍物碰撞，我们就停止这次爆炸
            if (hitDistance <= GridManager.Instance.GridUnitSize / 2f)
			{
				return;   
			}

            // 否则我们就爆炸

            // 我们计算爆炸的规模（大小）
            float explosionLength;            
			float adjustedDistance = hitDistance - GridManager.Instance.GridUnitSize / 2f;
			float maxExplosionLength = BombDistanceInGridUnits * GridManager.Instance.GridUnitSize;
			explosionLength = Mathf.Min(adjustedDistance, maxExplosionLength);
			explosionLength -= GridManager.Instance.GridUnitSize / 2f;

            // 我们设置伤害的大小和位置
            _damageAreaSize.x = GridManager.Instance.GridUnitSize / 2f;
			_damageAreaSize.y = GridManager.Instance.GridUnitSize / 2f;
			_damageAreaSize.z = explosionLength;

			_damageAreaPosition = this.transform.position 
			                      + (hit.point - (this.transform.position + RaycastOffset)).normalized * (explosionLength / 2f + GridManager.Instance.GridUnitSize/2f)
			                      + Vector3.up * GridManager.Instance.GridUnitSize / 2f;

			damageArea.gameObject.transform.position = _damageAreaPosition;
			damageArea.gameObject.transform.LookAt(this.transform.position + Vector3.up * (GridManager.Instance.GridUnitSize / 2f));

            // 我们激活伤害区域
            damageArea.gameObject.SetActive(true);            
			damageArea.gameObject.MMFGetComponentNoAlloc<BoxCollider>().size = _damageAreaSize;

            // 我们激活视觉效果爆炸（VFX：视觉效果）
            explosion.gameObject.SetActive(true);
			explosion.transform.position = _damageAreaPosition;
			ParticleSystem.ShapeModule shape = explosion.shape;
			shape.scale = new Vector3(0.1f, 0.1f, explosionLength);
			shape.rotation = new Vector3(0f, angle, 0f);

            // 我们触发一个炸弹事件
            MMGameEvent.Trigger("Bomb");

		}

        /// <summary>
        /// 向四个基本方向（东西南北）发射光线以寻找障碍物和受害者
        /// </summary>
        protected virtual void CastRays()
		{
			float boxWidth = (_boxCollider.bounds.size.x / 2f) + SkinWidth;
			boxWidth = 0f;

			_raycastEast = MMDebug.Raycast3D(this.transform.position + Vector3.right * boxWidth +  RaycastOffset, Vector3.right, MaximumRaycastDistance, ObstaclesMask, Color.red, true);
			if (_raycastEast.collider != null) { _obstacleEastDistance = _raycastEast.distance; } else { _obstacleEastDistance = 0f; }
            
			_raycastNorth = MMDebug.Raycast3D(this.transform.position + Vector3.forward * boxWidth + RaycastOffset, Vector3.forward, MaximumRaycastDistance, ObstaclesMask, Color.red, true);
			if (_raycastNorth.collider != null) { _obstacleNorthDistance = _raycastNorth.distance; } else { _obstacleNorthDistance = 0f; }
            
			_raycastSouth = MMDebug.Raycast3D(this.transform.position + Vector3.back * boxWidth + RaycastOffset, Vector3.back, MaximumRaycastDistance, ObstaclesMask, Color.red, true);
			if (_raycastSouth.collider != null) { _obstacleSouthDistance = _raycastSouth.distance; } else { _obstacleSouthDistance = 0f; }

			_raycastWest = MMDebug.Raycast3D(this.transform.position + Vector3.left * boxWidth + RaycastOffset, Vector3.left, MaximumRaycastDistance, ObstaclesMask, Color.red, true);
			if (_raycastWest.collider != null) { _obstacleWestDistance = _raycastWest.distance; } else { _obstacleWestDistance = 0f; }

		}

        /// <summary>
        /// 当对象被生成时，我们初始化它
        /// </summary>
        public virtual void OnEnable()
		{
			ResetBomb();
		}

        /// <summary>
        /// 当炸弹的所有者离开它时，我们将其设为障碍物
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit(Collider collider)
		{
			if (collider.gameObject == Owner)
			{
				_boxCollider.isTrigger = false;
			}
		}
	}
}