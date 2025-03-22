using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 你可以将这个类添加到WeaponIK组件的旁边（通常在角色的动画器上），它将让你在某些动画期间禁用IK，并可选择地重新设置WeaponAttachment的父子关系 
    /// </summary>
    public class WeaponIKDisabler : TopDownMonoBehaviour
	{
		[Header("Animation Parameter Names动画参数名称")] 
		/// a list of animation parameter names which, if true, should cause IK to be disabled
		[Tooltip("如果为真，则应导致IK被禁用的动画参数名称列表")]
		public List<string> AnimationParametersPreventingIK;
        
		[Header("Attachments附件")]
		/// the WeaponAttachment transform to reparent
		[Tooltip("要设置为父对象的WeaponAttachment变换")]
		public Transform WeaponAttachment;
		/// the transform the WeaponAttachment will be reparented to when certain animation parameters are true
		[Tooltip("当某些动画参数为真时，WeaponAttachment将被设置为其父对象的变换")]
		public Transform WeaponAttachmentParentNoIK;

		[Header("Settings设置")]
		/// whether or not to match parent position when disabling IK
		[Tooltip("在禁用IK时，是否匹配父对象的位置")]
		public bool FollowParentPosition = false;
		/// whether or not to disable weapon aim when disabling IK
		[Tooltip("在禁用IK时，是否禁用武器瞄准")]
		public bool ControlWeaponAim = false;

		protected Transform _initialParent;
		protected Vector3 _initialLocalPosition;
		protected Vector3 _initialLocalScale;
		protected Quaternion _initialRotation;
		protected WeaponIK _weaponIK;
		protected WeaponAim _weaponAim;
		protected Animator _animator;
		protected List<int> _animationParametersHashes;
		protected bool _shouldSetIKLast = true;

        /// <summary>
        /// 在开始时，我们初始化我们的组件
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 获取动画器、WeaponIK，对动画参数名称进行哈希处理，并存储初始位置
        /// </summary>
        protected virtual void Initialization()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			_weaponIK = this.gameObject.GetComponent<WeaponIK>();
			_animator = this.gameObject.GetComponent<Animator>();
			_animationParametersHashes = new List<int>();
            
			foreach (string _animationParameterName in AnimationParametersPreventingIK)
			{
				int newHash = Animator.StringToHash(_animationParameterName);
				_animationParametersHashes.Add(newHash);
			}

			if (WeaponAttachment != null)
			{
				_initialParent = WeaponAttachment.parent;
				_initialLocalPosition = WeaponAttachment.transform.localPosition;
				_initialLocalScale = WeaponAttachment.transform.localScale;
				_initialRotation = WeaponAttachment.transform.localRotation;
			}
		}

        /// <summary>
        /// 在动画器IK上，如果需要，我们打开或关闭IK
        /// </summary>
        /// <param name="layerIndex"></param>
        protected virtual void OnAnimatorIK(int layerIndex)
		{
			if ((_animator == null) || (_weaponIK == null) || (WeaponAttachment == null))
			{
				return;
			}

			if (_animationParametersHashes.Count <= 0)
			{
				return;
			}

			bool shouldPreventIK = false;
			foreach (int hash in _animationParametersHashes)
			{
				if (_animator.GetBool(hash))
				{
					shouldPreventIK = true;
				}
			}

			if (shouldPreventIK != _shouldSetIKLast)
			{
				PreventIK(shouldPreventIK);
			}

			_shouldSetIKLast = shouldPreventIK;
		}

        /// <summary>
        /// 启用或禁用IK
        /// </summary>
        /// <param name="status"></param>
        protected virtual void PreventIK(bool status)
		{
			if (status)
			{
				_weaponIK.AttachLeftHand = false;
				_weaponIK.AttachRightHand = false;
				WeaponAttachment.transform.SetParent(WeaponAttachmentParentNoIK);

				if (FollowParentPosition)
				{
					WeaponAttachment.transform.localPosition = Vector3.zero;
					WeaponAttachment.transform.localScale = WeaponAttachmentParentNoIK.transform.localScale;
					WeaponAttachment.transform.localRotation = Quaternion.identity;	
				}
				
				EnableWeaponAim(false);
			}
			else
			{
				_weaponIK.AttachLeftHand = true;
				_weaponIK.AttachRightHand = true;
				WeaponAttachment.transform.SetParent(_initialParent);
                
				WeaponAttachment.transform.localPosition = _initialLocalPosition;
				WeaponAttachment.transform.localScale = _initialLocalScale;
				WeaponAttachment.transform.localRotation = _initialRotation;

				EnableWeaponAim(true);
			}
		}

        /// <summary>
        /// 根据指定状态启用或禁用武器瞄准
        /// </summary>
        /// <param name="status"></param>
        protected virtual void EnableWeaponAim(bool status)
		{
			if (!ControlWeaponAim)
			{
				return;
			}

			_weaponAim.enabled = status;
		}
	}
}