using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到触发器 Collider2D，它将让您知道何时有角色进入它
    /// 并让您因此触发操作
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Character Detector")]
	[RequireComponent(typeof(Collider2D))]
	public class CharacterDetector : TopDownMonoBehaviour
	{
		/// It this is true, the character will have to be tagged Player for this to work
		[Tooltip("如果这是真的，角色就必须被标记为Player，这样这才能起作用")]
		public bool RequiresPlayer = true;
		/// if this is true, a character (and possibly a player based on the setting above) is in the area
		[MMReadOnly]
		[Tooltip("如果这是真的，一个角色（可能基于上面的设置是一个玩家）在区域内")]
		public bool CharacterInArea = false;
		/// a UnityEvent to fire when the targeted character enters the area
		[Tooltip("当目标角色进入区域时触发一个UnityEvent")]
		public UnityEvent OnEnter;
		/// a UnityEvent to fire while the targeted character stays in the area
		[Tooltip("当目标角色停留在区域内时触发的一个UnityEvent")]
		public UnityEvent OnStay;
		/// a UnityEvent to fire when the targeted character exits the area
		[Tooltip("当目标角色离开区域时触发的一个UnityEvent")]
		public UnityEvent OnExit;

		protected Collider2D _collider2D;
		protected Collider _collider;
		protected Character _character;

        /// <summary>
        /// 在开始时，我们获取collider2D并设置它为触发，以防我们再次忘记设置它为触发
        /// </summary>
        protected virtual void Start()
		{
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			_collider = this.gameObject.GetComponent<Collider>();
			if (_collider2D != null) { _collider2D.isTrigger = true; }
			if (_collider != null) { _collider.isTrigger = true; }
		}

        /// <summary>
        /// 当一个角色进入时，我们将状态转为true
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2D(Collider2D collider) { OnTriggerEnterProxy(collider.gameObject); }
		protected void OnTriggerEnter(Collider collider) { OnTriggerEnterProxy(collider.gameObject); }

		protected virtual void OnTriggerEnterProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}

			CharacterInArea = true;

			if (OnEnter != null)
			{
				OnEnter.Invoke();
			}
		}

        /// <summary>
        /// 当一个角色停留时，我们保持布尔值为true
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerStay2D(Collider2D collider) { OnTriggerStayProxy(collider.gameObject); }
		protected void OnTriggerStay(Collider collider) { OnTriggerStayProxy(collider.gameObject); }

		protected virtual void OnTriggerStayProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}
            
			CharacterInArea = true;

			if (OnStay != null)
			{
				OnStay.Invoke();
			}
		}

        /// <summary>
        /// 当一个角色退出时，我们重置布尔值
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit2D(Collider2D collider) { OnTriggerExitProxy(collider.gameObject); }
		protected void OnTriggerExit(Collider collider) { OnTriggerExitProxy(collider.gameObject); }

		protected virtual void OnTriggerExitProxy(GameObject collider)
		{
			if (!TargetFound(collider))
			{
				return;
			}
            
			CharacterInArea = false;

			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

        /// <summary>
        /// 如果参数中设置的碰撞体是目标类型，则返回true，否则返回false
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool TargetFound(GameObject collider)
		{
			_character = collider.gameObject.MMGetComponentNoAlloc<Character>();
            
			if (_character == null)
			{
				return false;
			}

			if (RequiresPlayer && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return false;
			}

			return true;
		}
	}
}