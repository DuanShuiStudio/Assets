using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{	
	[SelectionBase]
    /// <summary>
    /// 一个处理库存视觉表示的组件，允许用户与其交互
    /// </summary>
    public class InventoryDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Binding绑定")]
        /// 要显示的库存的名称
        [MMInformation("库存显示是一个组件，它将处理库存中包含的数据的可视化。首先指定要显示的库存的名称", MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName = "MainInventory";
		public string PlayerID = "Player1";

		protected Inventory _targetInventory = null;

        /// <summary>
        /// 根据其名称抓取目标库存
        /// </summary>
        /// <value>The target inventory.</value>
        public Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if ((inventory.name == TargetInventoryName) && (inventory.PlayerID == PlayerID))
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}
		
		
		public struct ItemQuantity
		{
			public string ItemID;
			public int Quantity;

			public ItemQuantity(string itemID, int quantity)
			{
				ItemID = itemID;
				Quantity = quantity;
			}
		}

		[Header("Inventory Size库存大小")]
        /// 要显示的行数
        [MMInformation("库存显示以网格形式将库存数据展示在各个槽位中，每个槽位包含一个物品。在这里你可以设置想要的槽位行数和列数。一旦你对设置感到满意，可以按下这个检查器底部的“自动设置”按钮来查看你的更改", MMInformationAttribute.InformationType.Info,false)]
		public int NumberOfRows = 3;
        /// 要显示的列数
        public int NumberOfColumns = 2;

        /// 这个库存中的槽位总数
        public virtual int InventorySize { get { return NumberOfRows * NumberOfColumns; } set {} }		

		[Header("Equipment设备")]
		[MMInformation("如果这显示的是设备库存的内容，你应该在这里绑定一个选择库存。选择库存是你将为设备挑选物品的库存。通常，选择库存是主库存。同样，如果是设备库存，你可以指定想要授权的物品类别。", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay TargetChoiceInventory;
		public ItemClasses ItemClass;

		[Header("Behaviour行为")]
        /// 如果这是真的，即使槽位中不包含对象，我们也会绘制槽位；否则我们不会绘制它们
        [MMInformation("如果将其设置为真，则即使槽位为空也会被绘制出来；否则它们将对玩家隐藏", MMInformationAttribute.InformationType.Info,false)]
		public bool DrawEmptySlots=true;
        /// 如果这是真的，玩家将被允许使用移动按钮将对象从另一个库存移动到这个库存中
        [MMInformation("如果这是真的，玩家将被允许使用移动按钮将对象从另一个库存移动到这个库存中", MMInformationAttribute.InformationType.Info,false)]
		public bool AllowMovingObjectsToThisInventory = false;

		[Header("Inventory Padding库存填充")]
		[MMInformation("在这里你可以定义库存面板边界和槽位之间的填充", MMInformationAttribute.InformationType.Info,false)]
        /// 库存面板顶部和第一个槽位之间的内部边距
        public int PaddingTop = 20;
        /// 库存面板右侧和最后一个槽位之间的内部边距
        public int PaddingRight = 20;
        /// 库存面板底部和最后一个槽位之间的内部边距
        public int PaddingBottom = 20;
        /// 库存面板左侧和第一个槽位之间的内部边距
        public int PaddingLeft = 20;

		[Header("Slots槽")]
		[MMInformation(
            "当你按下这个库存底部的“自动设置”按钮时，库存显示将用准备好的槽位填充自己，以显示你的库存内容。在这里你可以定义槽位的大小、边距，并定义当槽位为空或已满时使用的图像等",
			MMInformationAttribute.InformationType.Info, false)]
        /// 要使用的槽位的游戏对象。如果为空，将在运行时自动创建一个
        public InventorySlot SlotPrefab;
        /// 槽位的水平尺寸和垂直尺寸
        public Vector2 SlotSize = new Vector2(50,50);
        /// 每个槽位中图标的尺寸
        public Vector2 IconSize = new Vector2(30,30);
        /// 要在槽位行和列之间应用的水平边距和垂直边距
        public Vector2 SlotMargin = new Vector2(5,5);
        /// 当槽位为空时，要设置为每个槽位背景的图像
        public Sprite EmptySlotImage;
        /// 当槽位不为空时，要设置为每个槽位背景的图像
        public Sprite FilledSlotImage;
        /// 将设置为每个插槽在高亮显示时的槽背景图片
        public Sprite HighlightedSlotImage;
        /// 将设置为每个插槽在被按下时的背景图片
        public Sprite PressedSlotImage;
        /// 将设置为每个插槽在被禁用时的背景图片
        public Sprite DisabledSlotImage;
        /// 将设置为每个插槽中的物品在被移动时的背景图片
        public Sprite MovedSlotImage;
        /// 图像的类型（切片的、普通的、平铺的……）
        public Image.Type SlotImageType;
		
		[Header("Navigation导航")]
		[MMInformation("在这里，您可以决定是否使用内置的导航系统（允许玩家使用键盘箭头或操纵杆在插槽之间移动），以及当场景开始时，这个库存显示面板是否应该获得焦点。通常，您会希望主库存获得焦点。", MMInformationAttribute.InformationType.Info,false)]
        /// 如果为真，引擎将自动创建绑定，以便使用键盘或游戏手柄在不同的插槽之间进行导航
        public bool EnableNavigation = true;
        /// 如果这个值为真，那么在开始的时候，这个库存显示将会获得焦点
        public bool GetFocusOnStart = false;

		[Header("Title Text标题文本")]
		[MMInformation("在这里，您可以决定是否在库存显示面板旁边显示一个标题。对于它，您可以指定标题、字体、字体大小、颜色等", MMInformationAttribute.InformationType.Info,false)]
        /// 如果为真，将显示面板的标题。
        public bool DisplayTitle=true;
        /// 要显示的库存的标题
        public string Title;
        /// 用于显示数量的字体
        public Font TitleFont;
        /// 要使用的字体大小
        public int TitleFontSize=20;
        /// 用于显示数量的颜色
        public Color TitleColor = Color.black;
        /// 填充（与插槽边缘的距离）
        public Vector3 TitleOffset=Vector3.zero;
        /// 应该显示数量的位置
        public TextAnchor TitleAlignment = TextAnchor.LowerRight;

		[Header("Quantity Text数量文本")]
		[MMInformation("如果您的库存包含堆叠的物品（即单个插槽中某种物品的数量超过一个，例如硬币或药水），您可能需要在该物品的图标旁边显示数量。为此，您可以在这里指定要使用的字体、颜色和数量文本的位置", MMInformationAttribute.InformationType.Info,false)]
        /// 用于显示数量的字体
        public Font QtyFont;
        /// 要使用的字体大小
        public int QtyFontSize=12;
        /// 用于显示数量的颜色
        public Color QtyColor = Color.black;
        /// 填充（与插槽边缘的距离）
        public float QtyPadding=10f;
        /// 应该显示数量的位置
        public TextAnchor QtyAlignment = TextAnchor.LowerRight;

		[Header("Extra Inventory Navigation额外的库存导航")]
		[MMInformation("库存输入管理器带有控件，允许您从一个库存面板转到下一个。在这里，您可以定义当玩家按下上一个或下一个库存按钮时，应该从这个面板转到哪个库存", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay PreviousInventory;
		public InventoryDisplay NextInventory;

        /// 用于以行和列显示库存的网格布局
        public virtual GridLayoutGroup InventoryGrid { get; protected set; }
        /// 用于显示库存名称的游戏对象
        public virtual InventoryDisplayTitle InventoryTitle { get; protected set; }
        /// 主面板
        public virtual RectTransform InventoryRectTransform { get { return GetComponent<RectTransform>(); }}
        /// 插槽的内部列表
        public virtual List<InventorySlot> SlotContainer { get; protected set; }
        /// 在操作后焦点应返回的库存
        public virtual InventoryDisplay ReturnInventory { get; protected set; }
        /// 这个库存显示是打开的还是关闭的
        public virtual bool IsOpen { get; protected set; }
		
		public virtual bool InEquipSelection { get; set; }

        /// 当前正在移动的物品

        public static InventoryDisplay CurrentlyBeingMovedFromInventoryDisplay;
		public static int CurrentlyBeingMovedItemIndex = -1;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			CurrentlyBeingMovedFromInventoryDisplay = null;
			CurrentlyBeingMovedItemIndex = -1;
		}

		protected List<ItemQuantity> _contentLastUpdate;	
		protected List<int> _comparison;	
		protected SpriteState _spriteState = new SpriteState();
		protected InventorySlot _currentlySelectedSlot;
		protected InventorySlot _slotPrefab = null;

        /// <summary>
        /// 创建并设置库存显示（通常通过检查器的专用按钮调用）
        /// </summary>
        public virtual void SetupInventoryDisplay()
		{
			if (TargetInventoryName == "")
			{
				Debug.LogError("这个 " + this.name + " 库存显示没有设置TargetInventoryName。您需要从检查器中设置一个，匹配库存的名称。");
				return;
			}

			if (TargetInventory == null)
			{
				Debug.LogError("这个 " + this.name + " 库存显示找不到目标库存。您需要创建一个具有匹配库存名称的库存，或者在场景中的某个位置激活一个库存输入管理器 (" + TargetInventoryName + "), 或者将TargetInventoryName设置为已存在的库存.");
				return;
			}

            // 如果我们还拥有一个声音播放器组件，我们也将其设置好
            if (this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer> ().SetupInventorySoundPlayer ();
			}

			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryTitle();
			ResizeInventoryDisplay ();
			DrawInventoryContent();
		}

        /// <summary>
        /// 在唤醒时，初始化用于跟踪库存内容的各种列表。
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

        /// <summary>
        /// 初始化列表并重新绘制库存显示
        /// </summary>
        public virtual void Initialization(bool forceRedraw = false)
		{
			_contentLastUpdate = new List<ItemQuantity>();		
			SlotContainer = new List<InventorySlot>() ;		
			_comparison = new List<int>();
			if (!TargetInventory.Persistent || forceRedraw)
			{
				RedrawInventoryDisplay();
			}
		}

        /// <summary>
        /// 在需要时重新绘制库存显示的内容（通常在目标库存发生变化后）
        /// </summary>
        protected virtual void RedrawInventoryDisplay()
		{
			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryContent();		
			FillLastUpdateContent();	
		}

        /// <summary>
        /// 初始化精灵。
        /// </summary>
        protected virtual void InitializeSprites()
		{
            // 我们创建一个spriteState来指定我们的各种按钮状态
            _spriteState.disabledSprite = DisabledSlotImage;
			_spriteState.selectedSprite = HighlightedSlotImage;
			_spriteState.highlightedSprite = HighlightedSlotImage;
			_spriteState.pressedSprite = PressedSlotImage;
		}

        /// <summary>
        /// 添加并设置库存标题子对象
        /// </summary>
        protected virtual void DrawInventoryTitle()
		{
			if (!DisplayTitle)
			{
				return;
			}
			if (GetComponentInChildren<InventoryDisplayTitle>() != null)
			{
				if (!Application.isPlaying)
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						DestroyImmediate(title.gameObject);
					}
				}
				else
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						Destroy(title.gameObject);
					}
				}
			}
			GameObject inventoryTitle = new GameObject();
			InventoryTitle = inventoryTitle.AddComponent<InventoryDisplayTitle>();
			inventoryTitle.name="InventoryTitle";
			inventoryTitle.GetComponent<RectTransform>().SetParent(this.transform);
			inventoryTitle.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
			inventoryTitle.GetComponent<RectTransform>().localPosition = TitleOffset;
			inventoryTitle.GetComponent<RectTransform>().localScale = Vector3.one;
			InventoryTitle.text = Title;
			InventoryTitle.color = TitleColor;
			InventoryTitle.font = TitleFont;
			InventoryTitle.fontSize = TitleFontSize;
			InventoryTitle.alignment = TitleAlignment;
			InventoryTitle.raycastTarget = false;
		}

        /// <summary>
        /// 如果没有网格布局组则添加一个
        /// </summary>
        protected virtual void AddGridLayoutGroup()
		{
			if (GetComponentInChildren<InventoryDisplayGrid>() == null)
			{
				GameObject inventoryGrid=new GameObject("InventoryDisplayGrid");
				inventoryGrid.transform.parent=this.transform;
				inventoryGrid.transform.position=transform.position;
				inventoryGrid.transform.localScale=Vector3.one;
				inventoryGrid.AddComponent<InventoryDisplayGrid>();
				InventoryGrid = inventoryGrid.AddComponent<GridLayoutGroup>();
			}
			if (InventoryGrid == null)
			{
				InventoryGrid = GetComponentInChildren<GridLayoutGroup>();
			}
			InventoryGrid.padding.top = PaddingTop;
			InventoryGrid.padding.right = PaddingRight;
			InventoryGrid.padding.bottom = PaddingBottom;
			InventoryGrid.padding.left = PaddingLeft;
			InventoryGrid.cellSize = SlotSize;
			InventoryGrid.spacing = SlotMargin;
		}

        /// <summary>
        /// 调整库存面板的大小，考虑到行数/列数、填充和边距等因素
        /// </summary>
        protected virtual void ResizeInventoryDisplay()
		{

			float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns-1) + PaddingRight;
			float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows-1) + PaddingBottom;

			TargetInventory.ResizeArray(NumberOfRows * NumberOfColumns);	

			Vector2 newSize= new Vector2(newWidth,newHeight);
			InventoryRectTransform.sizeDelta = newSize;
			InventoryGrid.GetComponent<RectTransform>().sizeDelta = newSize;
		}

        /// <summary>
        /// 绘制库存的内容（插槽和图标）
        /// </summary>
        protected virtual void DrawInventoryContent ()             
		{            
			if (SlotContainer != null)
			{
				SlotContainer.Clear();
			}
			else
			{
				SlotContainer = new List<InventorySlot>();
			}
            // 我们初始化我们的精灵
            if (EmptySlotImage==null)
			{
				InitializeSprites();
			}
            // 我们移除所有现存的插槽
            foreach (InventorySlot slot in transform.GetComponentsInChildren<InventorySlot>())
			{	 			
				if (!Application.isPlaying)
				{
					DestroyImmediate (slot.gameObject);
				}
				else
				{
					Destroy(slot.gameObject);
				}				
			}
            // 对于每个插槽，我们创建该插槽及其内容
            for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{    
				DrawSlot(i);
			}

			if (_slotPrefab != null)
			{
				if (Application.isPlaying)
				{
					Destroy(_slotPrefab.gameObject);
					_slotPrefab = null;
				}
				else
				{
					DestroyImmediate(_slotPrefab.gameObject);
					_slotPrefab = null;
				}	
			}

			if (EnableNavigation)
			{
				SetupSlotNavigation();
			}
		}

        /// <summary>
        /// 如果内容已更改，我们再次绘制库存面板
        /// </summary>
        protected virtual void ContentHasChanged()
		{
			if (!(Application.isPlaying))
			{
				AddGridLayoutGroup();
				DrawInventoryContent();
				#if UNITY_EDITOR
				EditorUtility.SetDirty(gameObject);
				#endif
			}
			else
			{
				if (!DrawEmptySlots)
				{
					DrawInventoryContent();
				}
				else
				{
					UpdateInventoryContent();	
				}
			}
		}

        /// <summary>
        /// 填充更新的最后内容
        /// </summary>
        protected virtual void FillLastUpdateContent()		
		{		
			_contentLastUpdate.Clear();		
			_comparison.Clear();
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 		
			{  		
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					_contentLastUpdate.Add(new ItemQuantity(TargetInventory.Content[i].ItemID, TargetInventory.Content[i].Quantity));
				}
				else
				{
					_contentLastUpdate.Add(new ItemQuantity(null,0));	
				}	
			}	
		}

        /// <summary>
        /// 绘制库存的内容（插槽和图标）
        /// </summary>
        protected virtual void UpdateInventoryContent ()             
		{      
			if (_contentLastUpdate == null || _contentLastUpdate.Count == 0)
			{
				FillLastUpdateContent();
			}

            //我们将当前内容与存储中的内容进行比较，以查找变化
            for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{
				if ((TargetInventory.Content[i] == null) && (_contentLastUpdate[i].ItemID != null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID == null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i].ItemID != null))
				{
					if ((TargetInventory.Content[i].ItemID != _contentLastUpdate[i].ItemID) || (TargetInventory.Content[i].Quantity != _contentLastUpdate[i].Quantity))
					{
						_comparison.Add(i);
					}
				}
			}
			if (_comparison.Count>0)
			{
				foreach (int comparison in _comparison)
				{
					UpdateSlot(comparison);
				}
			} 	    
			FillLastUpdateContent();
		}

        /// <summary>
        /// 更新插槽的内容和外观
        /// </summary>
        /// <param name="i">The index.</param>
        protected virtual void UpdateSlot(int i)
		{
			
			if (SlotContainer.Count < i)
			{
				Debug.LogWarning ("It looks like your inventory display wasn't properly initialized. If you're not triggering any Load events, you may want to mark your inventory as non persistent in its inspector. Otherwise, you may want to reset and empty saved inventories and try again.");
			}

			if (SlotContainer.Count <= i)
			{
				return;
			}
			
			if (SlotContainer[i] == null)
			{
				return;
			}
            // 我们更新插槽的背景图像
            if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				SlotContainer[i].TargetImage.sprite = EmptySlotImage; 
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
                // 我们重新绘制图标
                SlotContainer[i].DrawIcon(TargetInventory.Content[i],i);
			}
			else
			{
				SlotContainer[i].DrawIcon(null,i);
			}
		}

        /// <summary>
        /// 创建用于所有插槽创建的插槽预制体
        /// </summary>
        protected virtual void InitializeSlotPrefab()
		{
			if (SlotPrefab != null)
			{
				_slotPrefab = Instantiate(SlotPrefab);
			}
			else
			{
				GameObject newSlot = new GameObject();
				newSlot.AddComponent<RectTransform>();

				newSlot.AddComponent<Image> ();
				newSlot.MMGetComponentNoAlloc<Image> ().raycastTarget = true;

				_slotPrefab = newSlot.AddComponent<InventorySlot> ();
				_slotPrefab.transition = Selectable.Transition.SpriteSwap;

				Navigation explicitNavigation = new Navigation ();
				explicitNavigation.mode = Navigation.Mode.Explicit;
				_slotPrefab.GetComponent<InventorySlot> ().navigation = explicitNavigation;

				_slotPrefab.interactable = true;

				newSlot.AddComponent<CanvasGroup> ();
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().alpha = 1;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().interactable = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().blocksRaycasts = true;
				newSlot.MMGetComponentNoAlloc<CanvasGroup> ().ignoreParentGroups = false;

                // 我们添加图标
                GameObject itemIcon = new GameObject("Slot Icon", typeof(RectTransform));
				itemIcon.transform.SetParent(newSlot.transform);
				UnityEngine.UI.Image itemIconImage = itemIcon.AddComponent<Image>();
				_slotPrefab.IconImage = itemIconImage;
				RectTransform itemRectTransform = itemIcon.GetComponent<RectTransform>();
				itemRectTransform.localPosition = Vector3.zero;
				itemRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(itemRectTransform, IconSize);

                // 我们添加数量占位符
                GameObject textObject = new GameObject("Slot Quantity", typeof(RectTransform));
				textObject.transform.SetParent(itemIcon.transform);
				Text textComponent = textObject.AddComponent<Text>();
				_slotPrefab.QuantityText = textComponent;
				textComponent.font = QtyFont;
				textComponent.fontSize = QtyFontSize;
				textComponent.color = QtyColor;
				textComponent.alignment = QtyAlignment;
				RectTransform textObjectRectTransform = textObject.GetComponent<RectTransform>();
				textObjectRectTransform.localPosition = Vector3.zero;
				textObjectRectTransform.localScale = Vector3.one;
				MMGUI.SetSize(textObjectRectTransform, (SlotSize - Vector2.one * QtyPadding)); 

				_slotPrefab.name = "SlotPrefab";
			}
		}

        /// <summary>
        /// 绘制插槽及其内容（图标、数量等）
        /// </summary>
        /// <param name="i">The index.</param>
        protected virtual void DrawSlot(int i)
		{
			if (!DrawEmptySlots)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					return;
				}
			}
			
			if ((_slotPrefab == null) || (!_slotPrefab.isActiveAndEnabled))
			{
				InitializeSlotPrefab ();
			}

			InventorySlot theSlot = Instantiate(_slotPrefab);

			theSlot.transform.SetParent(InventoryGrid.transform);
			theSlot.TargetRectTransform.localScale = Vector3.one;
			theSlot.transform.position = transform.position;
			theSlot.name = "Slot "+i;

            // 我们添加背景图像
            if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				theSlot.TargetImage.sprite = FilledSlotImage;   
			}
			else
			{
				theSlot.TargetImage.sprite = EmptySlotImage;      	
			}
			theSlot.TargetImage.type = SlotImageType; 
			theSlot.spriteState=_spriteState;
			theSlot.MovedSprite=MovedSlotImage;
			theSlot.ParentInventoryDisplay = this;
			theSlot.Index=i;

			SlotContainer.Add(theSlot);	

			theSlot.gameObject.SetActive(true)	;

			theSlot.DrawIcon(TargetInventory.Content[i],i);
		}

        /// <summary>
        /// 使用 Unity 的内置 GUI 系统设置插槽导航，以便用户可以使用左/右/上/下箭头进行移动
        /// </summary>
        protected virtual void SetupSlotNavigation()
		{
			if (!EnableNavigation)
			{
				return;
			}

			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (SlotContainer[i]==null)
				{
					return;
				}
				Navigation navigation = SlotContainer[i].navigation;
                // 我们确定向上移动时前往的位置
                if (i - NumberOfColumns >= 0) 
				{
					navigation.selectOnUp = SlotContainer[i-NumberOfColumns];
				}
				else
				{
					navigation.selectOnUp=null;
				}
                // 我们确定向下移动时前往的位置
                if (i+NumberOfColumns < SlotContainer.Count) 
				{
					navigation.selectOnDown = SlotContainer[i+NumberOfColumns];
				}
				else
				{
					navigation.selectOnDown=null;
				}
                // 我们确定向左移动时前往的位置
                if ((i%NumberOfColumns != 0) && (i>0))
				{
					navigation.selectOnLeft = SlotContainer[i-1];
				}
				else
				{
					navigation.selectOnLeft=null;
				}
                // 我们确定向右移动时前往的位置
                if (((i+1)%NumberOfColumns != 0)  && (i<SlotContainer.Count - 1))
				{
					navigation.selectOnRight = SlotContainer[i+1];
				}
				else
				{
					navigation.selectOnRight=null;
				}
				SlotContainer[i].navigation = navigation;
			}
		}

        /// <summary>		
        /// 将焦点设置在库存的第一个项目上		
        /// </summary>		
        public virtual void Focus()		
		{
			if (!EnableNavigation)
			{
				return;
			}
			
			if (SlotContainer.Count > 0)
			{
				SlotContainer[0].Select();
			}		

			if (EventSystem.current.currentSelectedGameObject == null)
			{
				InventorySlot newSlot = transform.GetComponentInChildren<InventorySlot>();
				if (newSlot != null)
				{
					EventSystem.current.SetSelectedGameObject (newSlot.gameObject);	
				}
			}			
		}

        /// <summary>
        /// 返回当前选中的库存插槽
        /// </summary>
        /// <returns>The selected inventory slot.</returns>
        public virtual InventorySlot CurrentlySelectedInventorySlot()
		{
			return _currentlySelectedSlot;
		}

        /// <summary>
        /// 设置当前选中的插槽
        /// </summary>
        /// <param name="slot">Slot.</param>
        public virtual void SetCurrentlySelectedSlot(InventorySlot slot)
		{
			_currentlySelectedSlot = slot;
		}

        /// <summary>
        /// 根据传入参数中的 int 方向（-1 为上一个，1 为下一个），前往上一个或下一个库存
        /// </summary>
        /// <param name="direction">Direction.</param>
        public virtual InventoryDisplay GoToInventory(int direction)
		{
			if (direction==-1)
			{
				if (PreviousInventory==null)
				{
					return null;
				}
				PreviousInventory.Focus();
				return PreviousInventory;
			}
			else
			{
				if (NextInventory==null)
				{
					return null;
				}
				NextInventory.Focus();	
				return NextInventory;			
			}
		}

        /// <summary>
        /// 设置返回库存显示
        /// </summary>
        /// <param name="inventoryDisplay">Inventory display.</param>
        public virtual void SetReturnInventory(InventoryDisplay inventoryDisplay)
		{
			ReturnInventory = inventoryDisplay;
		}

        /// <summary>
        /// 如果可能，将焦点返回到当前返回库存焦点（通常在拾取物品后）
        /// </summary>
        public virtual void ReturnInventoryFocus()
		{
			if (ReturnInventory == null)
			{
				return;
			}
			else
			{
				InEquipSelection = false;
				ResetDisabledStates();
				ReturnInventory.Focus();
				ReturnInventory = null;
			}
		}

        /// <summary>
        /// 禁用库存显示中的所有插槽，除了来自特定类别的那些
        /// </summary>
        /// <param name="itemClass">Item class.</param>
        public virtual void DisableAllBut(ItemClasses itemClass)
		{
			for (int i=0; i < SlotContainer.Count;i++)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					continue;
				}
				if (TargetInventory.Content[i].ItemClass!=itemClass)
				{
					SlotContainer[i].DisableSlot();
				}
			}
		}

        /// <summary>
        /// 启用所有插槽（通常在禁用了一些之后）
        /// </summary>
        public virtual void ResetDisabledStates()
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				SlotContainer[i].EnableSlot();
			}
		}

        /// <summary>
        /// 一种可用于将此显示的目标库存更改为新库存的公共方法
        /// </summary>
        /// <param name="newInventoryName"></param>
        public virtual void ChangeTargetInventory(string newInventoryName)
		{
			_targetInventory = null;
			TargetInventoryName = newInventoryName;
			Initialization(true);
		}

        /// <summary>
        /// 捕获 MMInventoryEvents 并对它们采取行动
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
            // 如果此事件与我们库存显示无关，我们则不执行任何操作并退出
            if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
			{
				return;
			}

			if (inventoryEvent.PlayerID != this.PlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Click:
					ReturnInventoryFocus ();
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Move:
					this.ReturnInventoryFocus();
					UpdateSlot(inventoryEvent.Index);

					break;

				case MMInventoryEventType.ItemUsed:
					this.ReturnInventoryFocus();
					break;
				
				case MMInventoryEventType.EquipRequest:
					if (this.TargetInventory.InventoryType == Inventory.InventoryTypes.Equipment)
					{
                        // 如果没有设置目标库存，我们则不执行任何操作并退出
                        if (TargetChoiceInventory == null)
						{
							Debug.LogWarning ("InventoryEngine Warning : " + this + " has no choice inventory associated to it.");
							return;
						}
                        // 我们禁用所有与正确类型不匹配的插槽
                        TargetChoiceInventory.DisableAllBut (this.ItemClass);
                        // 我们将焦点设置在目标库存上
                        TargetChoiceInventory.Focus ();
						TargetChoiceInventory.InEquipSelection = true;
                        // 我们将返回焦点库存设置
                        TargetChoiceInventory.SetReturnInventory (this);
					}
					break;
				
				case MMInventoryEventType.ItemEquipped:
					ReturnInventoryFocus();
					break;

				case MMInventoryEventType.Drop:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.ItemUnEquipped:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.InventoryOpens:
					Focus();
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					IsOpen = true;
					EventSystem.current.sendNavigationEvents = true;
					break;

				case MMInventoryEventType.InventoryCloses:
					InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					EventSystem.current.sendNavigationEvents = false;
					IsOpen = false;
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.ContentChanged:
					ContentHasChanged ();
					break;

				case MMInventoryEventType.Redraw:
					RedrawInventoryDisplay ();
					break;

				case MMInventoryEventType.InventoryLoaded:
					RedrawInventoryDisplay ();
					if (GetFocusOnStart)
					{
						Focus();
					}
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听 MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听 MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}