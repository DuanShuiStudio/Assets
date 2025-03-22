using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 用于强制模型瞄准武器目标的类
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Model")]
	public class WeaponModel : TopDownMonoBehaviour
	{
		[Header("Model模型")]
		/// a unique ID that will be used to hide / show this model when the corresponding weapon gets equipped
		[Tooltip("a unique ID that will be used to hide / show this model when the corresponding weapon gets equipped")]
		public string WeaponID = "WeaponID";
		/// a GameObject to show/hide for this model, usually nested right below the logic level of the WeaponModel
		[Tooltip("用于显示/隐藏该模型的游戏对象，通常嵌套在WeaponModel的逻辑层级正下方")]
		public GameObject TargetModel;

		[Header("Aim瞄准")]
		/// if this is true, the model will aim at the parent weapon's target
		[Tooltip("如果此选项为真，则模型将瞄准父武器的目标")]
		public bool AimWeaponModelAtTarget = true;
		/// if this is true, the model's aim will be vertically locked (no up/down aiming)
		[Tooltip("如果此选项为真，则模型的瞄准将垂直锁定（无上下瞄准）")]
		public bool LockVerticalRotation = true;

		[Header("Animator动画器")]
		/// whether or not to add the target animator to the real weapon's animator list
		[Tooltip("是否将目标动画器添加到真实武器的动画器列表中")]
		public bool AddAnimator = false;
		/// the animator to send weapon animation parameters to
		[Tooltip("发送武器动画参数的动画器")]
		public Animator TargetAnimator;

		[Header("SpawnTransform生成变换")]
		/// whether or not to override the weapon use transform
		[Tooltip("是否覆盖武器使用的变换")]
		public bool OverrideWeaponUseTransform = false;
		/// a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)
		[Tooltip("用作武器使用生成点的变换（如果为空，则只考虑偏移，否则为无偏移的变换）")]
		public Transform WeaponUseTransform;

		[Header("IK逆运动学")]
		/// whether or not to use IK with this model
		[Tooltip("此模型是否使用IK")]
		public bool UseIK = false;
		/// the transform to which the character's left hand should be attached to
		[Tooltip("角色左手应附加到的变换")]
		public Transform LeftHandHandle;
		/// the transform to which the character's right hand should be attached to
		[Tooltip("角色右手应附加到的变换")]
		public Transform RightHandHandle;

		[Header("Feedbacks反馈")]
		/// if this is true, the model's feedbacks will replace the original weapon's feedbacks
		[Tooltip("如果此选项为真，则模型的反馈将替换原始武器的反馈")]
		public bool BindFeedbacks = true;
		/// the feedback to play when the weapon starts being used
		[Tooltip("武器开始使用时播放的反馈")]
		public MMFeedbacks WeaponStartMMFeedback;
		/// the feedback to play while the weapon is in use
		[Tooltip("武器使用中播放的反馈")]
		public MMFeedbacks WeaponUsedMMFeedback;
		/// the feedback to play when the weapon stops being used
		[Tooltip("武器停止使用时播放的反馈")]
		public MMFeedbacks WeaponStopMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("武器重新装填时播放的反馈")]
		public MMFeedbacks WeaponReloadMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("武器重新装填时播放的反馈")]
		public MMFeedbacks WeaponReloadNeededMMFeedback;

		public virtual CharacterHandleWeapon Owner { get; set; }
		
		protected List<CharacterHandleWeapon> _handleWeapons;
		protected WeaponAim _weaponAim;
		protected Vector3 _rotationDirection;

		protected virtual void Awake()
		{
			Hide();
		}

        /// <summary>
        /// 在开始时，我们获取我们的CharacterHandleWeapon组件。
        /// </summary>
        protected virtual void Start()
		{
			_handleWeapons = this.GetComponentInParent<Character>()?.FindAbilities<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 使武器模型瞄准目标。
        /// </summary>
        protected virtual void Update()
		{
			if (!AimWeaponModelAtTarget)
			{
				return;
			}

			if (_weaponAim == null)
			{
				foreach (CharacterHandleWeapon handleWeapon in _handleWeapons)
				{
					if (handleWeapon.CurrentWeapon != null)
					{
						_weaponAim = handleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
					}
				}               
			}
			else
			{
				this.transform.rotation = _weaponAim.transform.rotation;
			}
		}

		public virtual void Show(CharacterHandleWeapon handleWeapon)
		{
			Owner = handleWeapon;
			TargetModel.SetActive(true);
		}

		public virtual void Hide()
		{
			TargetModel.SetActive(false);
		}
	}
}