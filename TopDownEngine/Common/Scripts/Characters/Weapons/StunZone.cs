using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 眩晕区域将会使任何进入其中且具有CharacterStun能力的角色眩晕
    /// </summary>
    public class StunZone : TopDownMonoBehaviour
	{
        /// 可能的眩晕模式：Forever（永久）：直到在CharacterStun组件上调用StunExit才会解除眩晕；ForDuration（持续一定时间）：眩晕持续一段时间后，角色会自动解除眩晕
        public enum StunModes { Forever, ForDuration }

		[Header("Stun Zone眩晕区域")]
		// the layers that will be stunned by this object
		[Tooltip("此对象将使哪些层眩晕")]
		public LayerMask TargetLayerMask;
		/// the chosen stun mode (Forever : stuns until StunExit is called on the CharacterStun component, ForDuration : stuns for a duration, and then the character will exit stun on its own)
		[Tooltip("选择的眩晕模式（Forever：永久眩晕，直到在CharacterStun组件上调用StunExit才会解除；ForDuration：持续一定时间的眩晕，之后角色将自动解除眩晕）")] 
		public StunModes StunMode = StunModes.ForDuration;
		/// if in ForDuration mode, the duration of the stun in seconds
		[Tooltip("如果处于ForDuration模式，眩晕持续的时间（秒）")]
		[MMEnumCondition("StunMode", (int)StunModes.ForDuration)]
		public float StunDuration = 2f;
		/// whether or not to disable the zone after the stun has happened
		[Tooltip("是否在眩晕发生后禁用该区域")]
		public bool DisableZoneOnStun = true;

		protected Character _character;
		protected CharacterStun _characterStun;

        /// <summary>
        /// 当与游戏对象碰撞时，我们确认它是否为目标，如果是，我们就使其眩晕
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{

				return;
			}

			_character = collider.GetComponent<Character>();
			if (_character != null) { _characterStun = _character.FindAbility<CharacterStun>(); }

			if (_characterStun == null)
			{
				return;
			}
            
			if (StunMode == StunModes.ForDuration)
			{
				_characterStun.StunFor(StunDuration);
			}
			else
			{
				_characterStun.Stun();
			}
            
			if (DisableZoneOnStun)
			{
				this.gameObject.SetActive(false);
			}
		}

        /// <summary>
        /// 当与玩家发生碰撞时，我们对玩家造成伤害并将其击退
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        // public virtual void OnTriggerStay2D(Collider2D collider)
        // {
        //     Colliding(collider.gameObject);
        // }

        /// <summary>
        /// 在2D触发器进入时，我们调用我们的碰撞端点
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发器停留时，我们调用我们的碰撞端点
        /// </summary>
        /// <param name="collider"></param>
        // public virtual void OnTriggerStay(Collider collider)
        // {
        //     Colliding(collider.gameObject);
        // }

        /// <summary>
        /// 在触发器进入时，我们调用我们的碰撞端点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}
	}    
}