using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在角色周围投射一个视锥的能力。
    /// </summary>
    [RequireComponent(typeof(MMConeOfVision))]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Cone of Vision")]
	public class CharacterConeOfVision : TopDownMonoBehaviour
	{
		protected MMConeOfVision _coneOfVision;
		protected CharacterOrientation3D _characterOrientation;

        /// <summary>
        /// 唤醒时，我们抓住我们的组件
        /// </summary>
        protected virtual void Awake()
		{
			_characterOrientation = this.gameObject.GetComponentInParent<CharacterOrientation3D>();
			_coneOfVision = this.gameObject.GetComponent<MMConeOfVision>();
		}

        /// <summary>
        /// 更新时，我们更新我们的视锥
        /// </summary>
        protected virtual void Update()
		{
			UpdateDirection();   
		}

        /// <summary>
        /// 将角色方向的角度发送到视锥
        /// </summary>
        protected virtual void UpdateDirection()
		{
			if (_characterOrientation == null)
			{
				_coneOfVision.SetDirectionAndAngles(this.transform.forward, this.transform.eulerAngles);              
			}
			else
			{
				_coneOfVision.SetDirectionAndAngles(_characterOrientation.ModelDirection, _characterOrientation.ModelAngles);              
			}
		}
	}
}