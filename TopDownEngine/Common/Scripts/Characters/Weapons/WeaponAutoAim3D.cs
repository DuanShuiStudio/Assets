using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// WeaponAutoAim的3D版本，旨在用于配备WeaponAim3D的物体上
    /// 它会在定义的半径内检测目标，选择最近的一个，并在找到目标时强制WeaponAim组件瞄准它。
    /// </summary>
    [RequireComponent(typeof(WeaponAim3D))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim 3D")]
	public class WeaponAutoAim3D : WeaponAutoAim
	{
		[Header("Overlap Detection重叠检测")]
		/// the maximum amount of targets the overlap detection can acquire
		[Tooltip("重叠检测可以获得的最大目标数量")]
		public int OverlapMaximum = 10;
        
		protected Vector3 _aimDirection;
		protected Collider[] _hits;
		protected Vector3 _raycastDirection;
		protected Collider _potentialHit;
		protected TopDownController3D _topDownController3D;
		protected Vector3 _origin;
		protected List<Transform> _potentialTargets;
        
		public Vector3 Origin
		{
			get
			{
				_origin = this.transform.position;
				if (_topDownController3D != null)
				{
					_origin += Quaternion.FromToRotation(Vector3.forward, _topDownController3D.CurrentDirection.normalized) * DetectionOriginOffset;
				}
				return _origin;
			}
		}

        /// <summary>
        /// 在初始化时，我们获取方向，以便能够检测朝向方向
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_potentialTargets = new List<Transform>();
			_hits = new Collider[10];
			if (_weapon.Owner != null)
			{
				_topDownController3D = _weapon.Owner.GetComponent<TopDownController3D>();
			}
		}

        /// <summary>
        /// 通过执行重叠检测来扫描目标，然后使用箱体投射验证火力线。
        /// </summary>
        /// <returns></returns>
        protected override bool ScanForTargets()
		{
			Target = null;
            
			int numberOfHits = Physics.OverlapSphereNonAlloc(Origin, ScanRadius, _hits, TargetsMask);
            
			if (numberOfHits == 0)
			{
				return false;
			}
            
			_potentialTargets.Clear();

            // 我们遍历找到的每个碰撞器。
            int min = Mathf.Min(OverlapMaximum, numberOfHits);
			for (int i = 0; i < min; i++)
			{
				if (_hits[i] == null)
				{
					continue;
				}
				if ((_hits[i].gameObject == this.gameObject) || (_hits[i].transform.IsChildOf(this.transform)))
				{
					continue;
				}  
                
				_potentialTargets.Add(_hits[i].gameObject.transform);
			}

            // 我们按距离对目标进行排序
            _potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector3.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
			});

            // 我们返回第一个未被遮挡的目标
            foreach (Transform t in _potentialTargets)
			{
				_raycastDirection = t.position - _raycastOrigin;
				RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, _raycastDirection.magnitude, ObstacleMask.value, Color.yellow, true);
				if ((hit.collider == null) && CanAcquireNewTargets())
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
        /// 确定射线投射原点
        /// </summary>
        protected override void DetermineRaycastOrigin()
		{
			_raycastOrigin = Origin;
		}
        
		protected override void OnDrawGizmos()
		{
			if (DrawDebugRadius)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(Origin, ScanRadius);
			}
		}
	}
}