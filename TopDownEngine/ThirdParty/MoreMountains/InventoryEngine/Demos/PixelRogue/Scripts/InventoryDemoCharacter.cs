using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{	

	[RequireComponent(typeof(Rigidbody2D))]
    /// <summary>
    /// 演示角色控制器，非常基础的东西
    /// </summary>
    public class InventoryDemoCharacter : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[MMInformation(
            "demo-一个非常基础的演示角色控制器，它让角色在xy轴上移动。在这里你可以改变它的速度并绑定精灵和装备物品栏",
			MMInformationAttribute.InformationType.Info, false)]

		public string PlayerID = "Player1";
        /// 角色速度
        public float CharacterSpeed = 300f;
        /// 用于显示当前武器的精灵
        public SpriteRenderer WeaponSprite;
        /// 盔甲物品栏
        public Inventory ArmorInventory;
        /// 武器物品栏
        public Inventory WeaponInventory;

		protected int _currentArmor=0;
		protected int _currentWeapon=0;
		protected float _horizontalMove = 0f;
		protected float _verticalMove = 0f;
		protected Vector2 _movement;
		protected Animator _animator;
		protected Rigidbody2D _rigidBody2D;
		protected bool _isFacingRight = true;

        /// <summary>
        /// 在开始时，我们存储角色的动画器和刚体
        /// </summary>
        protected virtual void Start()
		{
			_animator = GetComponent<Animator>();
			_rigidBody2D = GetComponent<Rigidbody2D>();
		}

        /// <summary>
        /// 在固定更新时，我们移动角色并更新其动画器
        /// </summary>
        protected virtual void FixedUpdate()
		{
			Movement();
			UpdateAnimator();
		}

        /// <summary>
        /// 更新这一帧角色的移动值
        /// </summary>
        /// <param name="movementX">Movement x.</param>
        /// <param name="movementY">Movement y.</param>
        public virtual void SetMovement(float movementX, float movementY)
		{
			_horizontalMove = movementX;
			_verticalMove = movementY;
		}

        /// <summary>
        /// 设置水平移动值
        /// </summary>
        /// <param name="value">Value.</param>
        public virtual void SetHorizontalMove(float value)
		{
			_horizontalMove = value;
		}

        /// <summary>
        /// 设置垂直移动值
        /// </summary>
        /// <param name="value">Value.</param>
        public virtual void SetVerticalMove(float value)
		{
			_verticalMove = value;
		}

        /// <summary>
        /// 对刚体的速度进行操作，以基于其当前的水平和垂直值来移动角色
        /// </summary>
        protected virtual void Movement()
		{
			if (_horizontalMove > 0.1f)
			{
				if (!_isFacingRight)
					Flip();
			}
            // 如果它是负数，那么我们面向左边
            else if (_horizontalMove < -0.1f)
			{
				if (_isFacingRight)
					Flip();
			}
			_movement = new Vector2(_horizontalMove, _verticalMove);
			_movement *= CharacterSpeed * Time.deltaTime;
			_rigidBody2D.velocity = _movement;
		}

        /// <summary>
        /// 水平翻转角色及其依赖项。
        /// </summary>
        protected virtual void Flip()
		{
            // 水平翻转角色
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
			_isFacingRight = transform.localScale.x > 0;
		}

        /// <summary>
        /// 更新动画器的参数
        /// </summary>
        protected virtual void UpdateAnimator()
		{
			if (_animator != null)
			{
				_animator.SetFloat("Speed", _rigidBody2D.velocity.magnitude);
				_animator.SetInteger("Armor", _currentArmor);
			}
		}

        /// <summary>
        /// 设置当前盔甲
        /// </summary>
        /// <param name="index">Index.</param>
        public virtual void SetArmor(int index)
		{
			_currentArmor = index;
		}

        /// <summary>
        /// 设置当前武器精灵
        /// </summary>
        /// <param name="newSprite">New sprite.</param>
        /// <param name="item">Item.</param>
        public virtual void SetWeapon(Sprite newSprite, InventoryItem item)
		{
			WeaponSprite.sprite = newSprite;
		}

        /// <summary>
        /// 捕捉MMInventoryEvents，如果是“物品栏加载”事件，则装备相应物品栏中存储的第一件盔甲和武器
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == "RogueArmorInventory")
				{
					if (ArmorInventory != null)
					{
						if (!InventoryItem.IsNull(ArmorInventory.Content [0]))
						{
							ArmorInventory.Content [0].Equip (PlayerID);	
						}
					}
				}
				if (inventoryEvent.TargetInventoryName == "RogueWeaponInventory")
				{
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							WeaponInventory.Content [0].Equip (PlayerID);
						}
					}
				}
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}


        /// <summary>
        /// 在禁用时，我们停止监听MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}