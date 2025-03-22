using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此添加到角色中，它将在死亡时触发其布娃娃系统
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Ragdoll on Death")]
	public class CharacterRagdollOnDeath : TopDownMonoBehaviour
	{
		[Header("Binding绑定")]
		/// The MMRagdoller for this character
		[Tooltip("角色的布娃娃系统")]
		public MMRagdoller Ragdoller;
		/// A list of optional objects to disable on death
		[Tooltip("要在死亡时禁用的可选对象列表")]
		public List<GameObject> ObjectsToDisableOnDeath;
		/// A list of optional monos to disable on death
		[Tooltip("死亡时要禁用的可选单声道列表")]
		public List<MonoBehaviour> MonosToDisableOnDeath;

		[Header("Force力")]
		/// the force by which the impact will be multiplied
		[Tooltip("冲击将被乘以的力")]
		public float ForceMultiplier = 10000f;

		[Header("Test测试")]
		/// A test button to trigger the ragdoll from the inspector
		[MMInspectorButton("Ragdoll")]
		[Tooltip("一个测试按钮，从检查器中触发布娃娃")]
		public bool RagdollButton;
		/// A test button to reset the ragdoll from the inspector
		[MMInspectorButton("ResetRagdoll")]
		[Tooltip("一个测试按钮，从检查器中重置布娃娃")]
		public bool ResetRagdollButton;
        
		protected TopDownController _controller;
		protected Health _health;
		protected Transform _initialParent;
		protected Vector3 _initialPosition;
		protected Quaternion _initialRotation;
		protected Character _character;

        /// <summary>
        /// 在Awake上，我们初始化组件
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

		/// <summary>
		/// 抓住我们的健康和控制器
		/// </summary>
		protected virtual void Initialization()
		{
			if (_health == null)
			{
				GrabHealth();
			}
			_controller = this.gameObject.GetComponent<TopDownController>();
			_initialParent = Ragdoller.transform.parent;
			_initialPosition = Ragdoller.transform.localPosition;
			_initialRotation = Ragdoller.transform.localRotation;
		}

		protected virtual void GrabHealth()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_health = (_character != null) ? _character.CharacterHealth : this.gameObject.GetComponent<Health>();
			if (_health != null)
			{
				_health.OnDeath += OnDeath;
				_health.OnRevive += OnRevive;
			}
		}

        /// <summary>
        /// 当我们收到OnDeath事件时，我们会布娃娃
        /// </summary>
        protected virtual void OnDeath()
		{
			Ragdoll();
		}

		protected virtual void OnRevive()
		{
			this.transform.position = Ragdoller.GetPosition();
			ResetRagdoll();
		}

        /// <summary>
        /// 禁用指定的对象和单声道，并触发布娃娃
        /// </summary>
        protected virtual void Ragdoll()
		{
			foreach (GameObject go in ObjectsToDisableOnDeath)
			{
				go.SetActive(false);
			}
			foreach (MonoBehaviour mono in MonosToDisableOnDeath)
			{
				mono.enabled = false;
			}
			Ragdoller.Ragdolling = true;
			Ragdoller.transform.SetParent(null);
			Ragdoller.MainRigidbody.AddForce(_controller.AppliedImpact.normalized * ForceMultiplier, ForceMode.Acceleration);
		}

		public virtual void ResetRagdoll()
		{
			Ragdoller.AllowBlending = false;
			
			foreach (GameObject go in ObjectsToDisableOnDeath)
			{
				go.SetActive(true);
			}
			foreach (MonoBehaviour mono in MonosToDisableOnDeath)
			{
				mono.enabled = true;
			}
			
			Ragdoller.transform.SetParent(_initialParent);
			Ragdoller.Ragdolling = false;
			Ragdoller.transform.localPosition = _initialPosition;
			Ragdoller.transform.localRotation = _initialRotation;
		}

        /// <summary>
        /// OnDestroy停止监听OnDeath事件
        /// </summary>
        protected virtual void OnDestroy()
		{
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
				_health.OnRevive -= OnRevive;
			}
		}
	}
}