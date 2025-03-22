using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.TopDownEngine
{
	[Serializable]
	public struct WeaponModelBindings
	{
		public GameObject WeaponModel;
		public int WeaponAnimationID;
	}

    /// <summary>
    /// 这个类负责启用/禁用武器的视觉表示（如果它们与实际的武器对象是分离的）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Model Enabler")]
	public class WeaponModelEnabler : TopDownMonoBehaviour
	{
		/// a list of model bindings. A binding is made of a gameobject, already present on the character, that will act as the visual representation of the weapon, and a name, that has to match the WeaponAnimationID of the actual Weapon
		[Tooltip("一个模型绑定的列表。绑定由角色身上已经存在的、将作为武器视觉表示的游戏对象，以及必须与实际武器的WeaponAnimationID匹配的名称组成。")]
		public WeaponModelBindings[] Bindings;

		public CharacterHandleWeapon HandleWeapon;

        /// <summary>
        /// 在唤醒（On Awake）时，我们获取我们的CharacterHandleWeapon组件
        /// </summary>
        protected virtual void Awake()
		{
			if (HandleWeapon == null)
			{
				HandleWeapon = this.gameObject.GetComponent<CharacterHandleWeapon>();	
			}
		}

        /// <summary>
        /// 在更新（On Update）时，我们根据它们的名称启用/禁用绑定的游戏对象。
        /// </summary>
        protected virtual void Update()
		{
			if (Bindings.Length <= 0)
			{
				return;
			}

			if (HandleWeapon == null)
			{
				return;
			}

			if (HandleWeapon.CurrentWeapon == null)
			{
				foreach (WeaponModelBindings binding in Bindings)
				{
					binding.WeaponModel.SetActive(false);
				}
				return;
			}

			foreach (WeaponModelBindings binding in Bindings)
			{
				if (binding.WeaponAnimationID == HandleWeapon.CurrentWeapon.WeaponAnimationID)
				{
					binding.WeaponModel.SetActive(true);
				}
				else
				{
					binding.WeaponModel.SetActive(false);
				}
			}
		}			
	}
}