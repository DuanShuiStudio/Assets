using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// WeaponAutoAim的2D版本，旨在用于配备WeaponAim2D的物体上。
    /// 它将在定义的半径内检测目标，选择最近的一个，并在找到目标时强制WeaponAim组件瞄准它。
    /// </summary>
    [RequireComponent(typeof(WeaponAim2D))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim 2D")]
	public class WeaponAutoAim2D : WeaponAutoAim
	{
		/// the maximum amount of targets the overlap detection can acquire
		[Tooltip("重叠检测可以获得的最大目标数量")]
		public int OverlapMaximum = 10;
        
		protected CharacterOrientation2D _orientation2D;
		protected Vector2 _facingDirection;
		protected Vector3 _boxcastDirection;
		protected Vector3 _aimDirection;
		protected bool _initialized = false;
		protected List<Transform> _potentialTargets;
		protected Collider2D[] _results;
		protected RaycastHit2D _hit;

        /// <summary>
        /// 在初始化时，我们获取方向，以便能够检测朝向方向
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_orientation2D = _weapon.Owner.GetComponent<Character>()?.FindAbility<CharacterOrientation2D>();
			_initialized = true;
			_results = new Collider2D[OverlapMaximum];
			_potentialTargets = new List<Transform>();
		}

        /// <summary>
        /// 通过执行重叠检测来扫描目标，然后使用箱体投射验证火力线。
        /// </summary>
        /// <returns></returns>
        protected override bool ScanForTargets()
		{
			if (!_initialized)
			{
				Initialization();
			}

			Target = null;

			int numberOfResults = Physics2D.OverlapCircleNonAlloc(_raycastOrigin, ScanRadius, _results, TargetsMask);
            // 如果周围没有目标，我们退出
            if (numberOfResults == 0)
			{
				return false;
			}
			_potentialTargets.Clear();

            // 我们遍历找到的每个碰撞器

            int min = Mathf.Min(OverlapMaximum, numberOfResults);
			for (int i = 0; i < min; i++)
			{
				if (_results[i] == null)
				{
					continue;
				}
                
				_potentialTargets.Add(_results[i].gameObject.transform);
			}


            // 我们按距离对目标进行排序
            _potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector2.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector2.Distance(this.transform.position,b.transform.position) );
			});

            // 我们返回第一个未被遮挡的目标
            foreach (Transform t in _potentialTargets)
			{
				_boxcastDirection = (Vector2)(t.gameObject.MMGetComponentNoAlloc<Collider2D>().bounds.center - _raycastOrigin);
                
				_hit = Physics2D.BoxCast(_raycastOrigin, LineOfFireBoxcastSize, 0f, _boxcastDirection.normalized, _boxcastDirection.magnitude, ObstacleMask); 
                
				if (!_hit && CanAcquireNewTargets())
				{
					Target = t;
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// 将瞄准设置为目标的相对方向
        /// </summary>
        protected override void SetAim()
		{
			_aimDirection = (Target.transform.position - _raycastOrigin).normalized;
			_weaponAim.SetCurrentAim(_aimDirection, ApplyAutoAimAsLastDirection);
		}

        /// <summary>
        /// 为了确定我们的射线投射原点，我们应用一个偏移量
        /// </summary>
        protected override void DetermineRaycastOrigin()
		{
			if (_orientation2D != null)
			{
				_facingDirection = _orientation2D.IsFacingRight ? Vector2.right : Vector2.left;
				_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
				_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;
			}
			else
			{
				_raycastOrigin = transform.position + DetectionOriginOffset;
			}
		}
	}
}