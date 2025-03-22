using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此区域添加到触发碰撞体，当3D角色进入时它会自动触发碰撞声
    /// </summary>
    [RequireComponent(typeof(Collider))]
	[AddComponentMenu("TopDown Engine/Environment/Crouch Zone")]
	public class CrouchZone : TopDownMonoBehaviour
	{
		protected CharacterCrouch _characterCrouch;

        /// <summary>
        /// 在开始时，我们确保我们的碰撞体被设置为触发
        /// </summary>
        protected virtual void Start()
		{
			this.gameObject.MMGetComponentNoAlloc<Collider>().isTrigger = true;
		}

        /// <summary>
        /// 进入时，如果我们能的话，我们强制触发碰撞声
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider collider)
		{
			_characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
			if (_characterCrouch != null)
			{
				_characterCrouch.StartForcedCrouch();
			}
		}

        /// <summary>
        /// 退出时，我们停止强制触发碰撞声
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit(Collider collider)
		{
			_characterCrouch = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterCrouch>();
			if (_characterCrouch != null)
			{
				_characterCrouch.StopForcedCrouch();
			}
		}
	}
}