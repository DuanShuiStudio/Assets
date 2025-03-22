using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到包含多个武器的对象中，它将把它变成组合武器，允许您从所有不同的武器中连锁攻击
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Combo Weapon")]
	public class ComboWeapon : TopDownMonoBehaviour
	{
		public enum InputModes { SemiAuto, Auto }
		
		[Header("Combo组合")]
		/// whether or not the combo can be dropped if enough time passes between two consecutive attacks
		[Tooltip("如果两次连续攻击之间经过足够的时间，组合是否可以被中断")]
		public bool DroppableCombo = true;
		/// the delay after which the combo drops
		[Tooltip("组合中断的延迟时间")]
		public float DropComboDelay = 0.5f;
		/// the input mode for this combo weapon. In Auto mode, you'll want to make sure you've set ContinuousPress:true on your CharacterHandleWeapon ability
		[Tooltip("这个组合武器的输入模式。在自动模式下，你需要确保在你的 CharacterHandleWeapon 能力上设置了 ContinuousPress:true")]
		public InputModes InputMode = InputModes.SemiAuto;
		
		[Header("Animation动画")]

		/// the name of the animation parameter to update when a combo is in progress.
		[Tooltip("当组合进行中时，要更新的动画参数的名称.")]
		public string ComboInProgressAnimationParameter = "ComboInProgress";

		[Header("Debug调试")]
		/// the list of weapons, set automatically by the class
		[MMReadOnly]
		[Tooltip("武器列表，由类自动设置")]
		public Weapon[] Weapons;
		/// the reference to the weapon's Owner
		[MMReadOnly]
		[Tooltip("武器所有者的引用")]
		public CharacterHandleWeapon OwnerCharacterHandleWeapon;
		/// the time spent since the last weapon stopped
		[MMReadOnly]
		[Tooltip("自上次武器停止以来所花费的时间")]
		public float TimeSinceLastWeaponStopped;

        /// <summary>
        ///如果组合正在进行中，则为真；否则为假
        /// </summary>
        /// <returns></returns>
        public bool ComboInProgress
		{
			get
			{
				bool comboInProgress = false;
				foreach (Weapon weapon in Weapons)
				{
					if (weapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle)
					{
						comboInProgress = true;
					}
				}
				return comboInProgress;
			}
		}

		protected int _currentWeaponIndex = 0;
		protected WeaponAutoShoot _weaponAutoShoot;
		protected bool _countdownActive = false;

        /// <summary>
        /// 在启动时，我们初始化我们的组合武器
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 获取所有武器组件并初始化它们
        /// </summary>
        public virtual void Initialization()
		{
			Weapons = GetComponents<Weapon>();
			_weaponAutoShoot = this.gameObject.GetComponent<WeaponAutoShoot>();
			InitializeUnusedWeapons();
		}

        /// <summary>
        /// 在更新时，如果需要，我们重置我们的组合
        /// </summary>
        protected virtual void Update()
		{
			ResetCombo();
		}

        /// <summary>
        /// 如果自上次攻击以来已经过了足够的时间，重置组合
        /// </summary>
        public virtual void ResetCombo()
		{
			if (Weapons.Length > 1)
			{
				if (_countdownActive && DroppableCombo)
				{
					TimeSinceLastWeaponStopped += Time.deltaTime;
					if (TimeSinceLastWeaponStopped > DropComboDelay)
					{
						_countdownActive = false;
                        
						_currentWeaponIndex = 0;
						OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[_currentWeaponIndex];
						OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[_currentWeaponIndex], Weapons[_currentWeaponIndex].WeaponName, true);
						if (_weaponAutoShoot != null)
						{
							_weaponAutoShoot.SetCurrentWeapon(Weapons[_currentWeaponIndex]);
						}
					}
				}
			}
		}

        /// <summary>
        /// 当其中一个武器被使用时，我们关闭倒计时
        /// </summary>
        /// <param name="weaponThatStarted"></param>
        public virtual void WeaponStarted(Weapon weaponThatStarted)
		{
			_countdownActive = false;
		}

        /// <summary>
        /// 当其中一个武器完成攻击时，我们开始倒计时并切换到下一个武器
        /// </summary>
        /// <param name="weaponThatStopped"></param>
        public virtual void WeaponStopped(Weapon weaponThatStopped)
		{
			ProceedToNextWeapon();
		}

		public virtual void ProceedToNextWeapon()
		{
			OwnerCharacterHandleWeapon = Weapons[_currentWeaponIndex].CharacterHandleWeapon;
            
			int newIndex = 0;
			if (OwnerCharacterHandleWeapon != null)
			{
				if (Weapons.Length > 1)
				{
					if (_currentWeaponIndex < Weapons.Length-1)
					{
						newIndex = _currentWeaponIndex + 1;
					}
					else
					{
						newIndex = 0;
					}

					_countdownActive = true;
					TimeSinceLastWeaponStopped = 0f;

					_currentWeaponIndex = newIndex;
					OwnerCharacterHandleWeapon.CurrentWeapon = Weapons[newIndex];
					OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = false;
					OwnerCharacterHandleWeapon.ChangeWeapon(Weapons[newIndex], Weapons[newIndex].WeaponName, true);
					OwnerCharacterHandleWeapon.CurrentWeapon.WeaponCurrentlyActive = true;
					
					if (_weaponAutoShoot != null)
					{
						_weaponAutoShoot.SetCurrentWeapon(Weapons[newIndex]);
					}
				}
			}
		}

        /// <summary>
        /// 翻转所有未使用的武器，以确保它们保持正确的方向
        /// </summary>
        public virtual void FlipUnusedWeapons()
		{
			for (int i = 0; i < Weapons.Length; i++)
			{
				if (i != _currentWeaponIndex)
				{
					Weapons[i].Flipped = !Weapons[i].Flipped;
				}                
			}
		}

        /// <summary>
        /// 初始化所有未使用的武器
        /// </summary>
        protected virtual void InitializeUnusedWeapons()
		{
			for (int i = 0; i < Weapons.Length; i++)
			{
				if (i != _currentWeaponIndex)
				{
					Weapons[i].SetOwner(Weapons[_currentWeaponIndex].Owner, Weapons[_currentWeaponIndex].CharacterHandleWeapon);
					Weapons[i].Initialization();
					Weapons[i].WeaponCurrentlyActive = false;
				}
			}
		}
	}
}