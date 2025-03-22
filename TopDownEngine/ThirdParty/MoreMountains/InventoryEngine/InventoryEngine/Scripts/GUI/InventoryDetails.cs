using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 用于在图形用户界面（GUI）中显示物品详情的类
    /// </summary>
    public class InventoryDetails : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        /// 我们将从中展示物品详情的参考库存
        [MMInformation("在此指定要在该详细信息面板中展示其内容详情的库存的名称。你也可以决定将其设置为全局。如果你这样做，它将展示所有物品的详情，而不管它们在哪个库存中", MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName;
		public string PlayerID = "Player1";
        /// 如果你将这个面板设置为全局，它将忽略
        public bool Global = false;
        /// 无论细节当前是否隐藏
        public virtual bool Hidden { get; protected set; }

		[Header("Default默认")]
		[MMInformation("通过勾选HideOnEmptySlot（在空槽位时隐藏），如果你选择一个空槽位，详细信息面板将不会显示。", MMInformationAttribute.InformationType.Info,false)]
        /// 当当前选择的槽位为空时，详细信息面板是否应该隐藏
        public bool HideOnEmptySlot=true;
		[MMInformation("在这里你可以为详细信息面板的所有字段设置默认值。当没有选择任何物品时（并且如果你选择在这种情况下不隐藏面板），这些值将会被显示", MMInformationAttribute.InformationType.Info,false)]
        /// 当没有提供标题时显示的标题
        public string DefaultTitle;
        /// 当没有提供简短描述时显示的简短描述
        public string DefaultShortDescription;
        /// 当没有提供描述时显示的描述
        public string DefaultDescription;
        /// 当没有提供数量时显示的数量
        public string DefaultQuantity;
        /// 当没有提供图标时显示的图标
        public Sprite DefaultIcon;

		[Header("Behaviour行为")]
		[MMInformation("在这里你可以决定是否在开始时隐藏详细信息面板", MMInformationAttribute.InformationType.Info,false)]
        /// 是否在开始时隐藏详细信息面板
        public bool HideOnStart = true;

		[Header("Components组件")]
		[MMInformation("在这里你需要绑定面板组件", MMInformationAttribute.InformationType.Info,false)]
        /// 图标容器对象
        public Image Icon;
        /// 标题容器对象
        public Text Title;
        /// 简短描述容器对象
        public Text ShortDescription;
        /// 描述容器对象
        public Text Description;
        /// 数量容器对象
        public Text Quantity;

		protected float _fadeDelay=0.2f;
		protected CanvasGroup _canvasGroup;

        /// <summary>
        /// 在开始时，我们获取并存储画布组，并确定我们当前的隐藏状态
        /// </summary>
        protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			if (HideOnStart)
			{
				_canvasGroup.alpha = 0;
			}

			if (_canvasGroup.alpha == 0)
			{
				Hidden = true;
			}
			else
			{
				Hidden = false;
			}
		}

        /// <summary>
        /// 根据当前槽位是否为空，启动显示协程或面板的淡入淡出效果
        /// </summary>
        /// <param name="item">Item.</param>
        public virtual void DisplayDetails(InventoryItem item)
		{
			if (InventoryItem.IsNull(item))
			{
				if (HideOnEmptySlot && !Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
					Hidden=true;
				}
				if (!HideOnEmptySlot)
				{
					StartCoroutine(FillDetailFieldsWithDefaults(0));
				}
			}
			else
			{
				StartCoroutine(FillDetailFields(item,0f));

				if (HideOnEmptySlot && Hidden)
				{
					StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,1f));
					Hidden=false;
				}
			}
		}

        /// <summary>
        /// 用物品的元数据填充各种详细信息字段
        /// </summary>
        /// <returns>The detail fields.</returns>
        /// <param name="item">Item.</param>
        /// <param name="initialDelay">Initial delay.</param>
        protected virtual IEnumerator FillDetailFields(InventoryItem item, float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = item.ItemName ; }
			if (ShortDescription!=null) { ShortDescription.text = item.ShortDescription;}
			if (Description!=null) { Description.text = item.Description;}
			if (Quantity!=null) { Quantity.text = item.Quantity.ToString();}
			if (Icon!=null) { Icon.sprite = item.Icon;}
			
			if (HideOnEmptySlot && !Hidden && (item.Quantity == 0))
			{
				StartCoroutine(MMFade.FadeCanvasGroup(_canvasGroup,_fadeDelay,0f));
				Hidden=true;
			}
		}

        /// <summary>
        /// 用默认值填充详细信息字段
        /// </summary>
        /// <returns>The detail fields with defaults.</returns>
        /// <param name="initialDelay">Initial delay.</param>
        protected virtual IEnumerator FillDetailFieldsWithDefaults(float initialDelay)
		{
			yield return new WaitForSeconds(initialDelay);
			if (Title!=null) { Title.text = DefaultTitle ;}
			if (ShortDescription!=null) { ShortDescription.text = DefaultShortDescription;}
			if (Description!=null) { Description.text = DefaultDescription;}
			if (Quantity!=null) { Quantity.text = DefaultQuantity;}
			if (Icon!=null) { Icon.sprite = DefaultIcon;}
		}

        /// <summary>
        /// 捕获MMInventoryEvents并在需要时显示详细信息
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
            // 如果该事件与我们库存显示无关，我们什么也不做并退出
            if (!Global && (inventoryEvent.TargetInventoryName != this.TargetInventoryName))
			{
				return;
			}

			if (inventoryEvent.PlayerID != PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.UseRequest:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.InventoryOpens:
					DisplayDetails (inventoryEvent.EventItem);
					break;
				case MMInventoryEventType.Drop:
					DisplayDetails (null);
					break;
				case MMInventoryEventType.EquipRequest:
					DisplayDetails (null);
					break;
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