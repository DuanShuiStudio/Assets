using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 拾取物品时通常触发的事件，让监听者知道什么物品被拾取了
    /// </summary>
    public struct PickableItemEvent
	{
		public GameObject Picker;
		public PickableItem PickedItem;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.PickableItemEvent"/> struct.
		/// </summary>
		/// <param name="pickedItem">Picked item.</param>
		public PickableItemEvent(PickableItem pickedItem, GameObject picker) 
		{
			Picker = picker;
			PickedItem = pickedItem;
		}
		static PickableItemEvent e;
		public static void Trigger(PickableItem pickedItem, GameObject picker)
		{
			e.Picker = picker;
			e.PickedItem = pickedItem;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 一个简单类，旨在扩展，将处理所有可选物品的机制：反馈、碰撞、拾取后果等
    /// </summary>
    public class PickableItem : TopDownMonoBehaviour
	{
		[Header("Pickable Item可拾取物品")]
		/// A feedback to play when the object gets picked
		[Tooltip("对象被拾取时要播放的反馈")]
		public MMFeedbacks PickedMMFeedbacks;
		/// if this is true, the picker's collider will be disabled on pick
		[Tooltip("如果这是真的，拾取者的碰撞器在拾取时将被禁用")]
		public bool DisableColliderOnPick = false;
		/// if this is set to true, the object will be disabled when picked
		[Tooltip("如果将其设置为true，对象在被拾取时将被禁用")]
		public bool DisableObjectOnPick = true;
		/// the duration (in seconds) after which to disable the object, instant if 0
		[MMCondition("DisableObjectOnPick", true)]
		[Tooltip("禁用对象的时间（以秒为单位），如果为0则立即禁用")]
		public float DisableDelay = 0f;
		/// if this is set to true, the object will be disabled when picked
		[Tooltip("如果将其设置为true，模型在被拾取时将被禁用")]
		public bool DisableModelOnPick = false;
		/// if this is set to true, the target object will be disabled when picked
		[Tooltip("如果将其设置为true，目标对象在被拾取时将被禁用")]
		public bool DisableTargetObjectOnPick = false;
		/// the object to disable on pick if DisableTargetObjectOnPick is true 
		[Tooltip("如果DisableTargetObjectOnPick为true，则在拾取时禁用的对象")]
		[MMCondition("DisableTargetObjectOnPick", true)]
		public GameObject TargetObjectToDisable;
		/// the time in seconds before disabling the target if DisableTargetObjectOnPick is true 
		[Tooltip("如果DisableTargetObjectOnPick为true，禁用目标之前的时间（以秒为单位）")]
		[MMCondition("DisableTargetObjectOnPick", true)]
		public float TargetObjectDisableDelay = 1f;
		/// the visual representation of this picker
		[MMCondition("DisableModelOnPick", true)]
		[Tooltip("这个拾取器的视觉表示")]
		public GameObject Model;

		[Header("Pick Conditions拾取条件")]
		/// if this is true, this pickable item will only be pickable by objects with a Character component 
		[Tooltip("如果这是真的，这个可选物品只能由带有角色组件的对象拾取")]
		public bool RequireCharacterComponent = true;
		/// if this is true, this pickable item will only be pickable by objects with a Character component of type player
		[Tooltip("如果这是真的，这个可选物品只能由带有玩家角色组件的对象拾取")]
		public bool RequirePlayerType = true;

		protected Collider _collider;
		protected Collider2D _collider2D;
		protected GameObject _collidingObject;
		protected Character _character = null;
		protected bool _pickable = false;
		protected ItemPicker _itemPicker = null;
		protected WaitForSeconds _disableDelay;

		protected virtual void Start()
		{
			_disableDelay = new WaitForSeconds(DisableDelay);
			_collider = gameObject.GetComponent<Collider>();
			_collider2D = gameObject.GetComponent<Collider2D>();
			_itemPicker = gameObject.GetComponent<ItemPicker> ();
			PickedMMFeedbacks?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 当有东西与硬币碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter (Collider collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

        /// <summary>
        /// 当有东西与硬币碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			_collidingObject = collider.gameObject;
			PickItem (collider.gameObject);
		}

        /// <summary>
        /// 检查物品是否可选，如果可选，则触发效果并禁用该对象
        /// </summary>
        public virtual void PickItem(GameObject picker)
		{
			if (CheckIfPickable ())
			{
				Effects ();
				PickableItemEvent.Trigger(this, picker);
				Pick (picker);
				if (DisableColliderOnPick)
				{
					if (_collider != null)
					{
						_collider.enabled = false;
					}
					if (_collider2D != null)
					{
						_collider2D.enabled = false;
					}
				}
				if (DisableModelOnPick && (Model != null))
				{
					Model.gameObject.SetActive(false);
				}
				
				if (DisableObjectOnPick)
				{
                    // 我们禁用该游戏对象
                    if (DisableDelay == 0f)
					{
						this.gameObject.SetActive(false);
					}
					else
					{
						StartCoroutine(DisablePickerCoroutine());
					}
				}
				
				if (DisableTargetObjectOnPick && (TargetObjectToDisable != null))
				{
					if (TargetObjectDisableDelay == 0f)
					{
						TargetObjectToDisable.SetActive(false);
					}
					else
					{
						StartCoroutine(DisableTargetObjectCoroutine());
					}
				}			
			} 
		}

		protected virtual IEnumerator DisableTargetObjectCoroutine()
		{
			yield return MMCoroutine.WaitFor(TargetObjectDisableDelay);
			TargetObjectToDisable.SetActive(false);
		}

		protected virtual IEnumerator DisablePickerCoroutine()
		{
			yield return _disableDelay;
			this.gameObject.SetActive(false);
		}

        /// <summary>
        /// 检查对象是否可选
        /// </summary>
        /// <returns><c>true</c>, if if pickable was checked, <c>false</c> otherwise.</returns>
        protected virtual bool CheckIfPickable()
		{
            // 如果与硬币碰撞的不是角色行为，我们就不采取任何行动并退出
            _character = _collidingObject.GetComponent<Character>();
			if (RequireCharacterComponent)
			{
				if (_character == null)
				{
					return false;
				}
				
				if (RequirePlayerType && (_character.CharacterType != Character.CharacterTypes.Player))
				{
					return false;
				}
			}
			if (_itemPicker != null)
			{
				if  (!_itemPicker.Pickable())
				{
					return false;	
				}
			}

			return true;
		}

        /// <summary>
        /// 触发各种拾取效果
        /// </summary>
        protected virtual void Effects()
		{
			PickedMMFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 重写这个方法以描述当对象被拾取时会发生什么
        /// </summary>
        protected virtual void Pick(GameObject picker)
		{
			
		}
	}
}