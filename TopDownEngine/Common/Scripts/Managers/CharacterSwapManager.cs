using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到场景中的空组件，这样在按下交换按钮（默认为P键）时，就可以在场景中切换角色
    /// 场景中的每个角色都需要有一个CharacterSwap类，并且需要有对应的PlayerID
    /// 你可以在MinimalCharacterSwap演示场景中看到这种设置的示例
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Characte Swap Manager")]
	public class CharacterSwapManager : MMSingleton<CharacterSwapManager>, MMEventListener<TopDownEngineEvent>
	{
		[Header("Character Swap角色交换")]
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the button to use to go up
			public Key SwapKey = Key.P;
		#else
		/// the name of the axis to use to catch input and trigger a swap on press
		[Tooltip("用于捕捉输入并在按下时触发交换的轴的名称")]
		public string SwapButtonName = "Player1_SwapCharacter";
		#endif
		/// the PlayerID set on the Characters you want to swap between
		[Tooltip("设置在你想要交换的角色上的PlayerID")]
		public string PlayerID = "Player1";

		protected CharacterSwap[] _characterSwapArray;
		protected MMCircularList<CharacterSwap> _characterSwapList;
		protected TopDownEngineEvent _swapEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwap, null);

        /// <summary>
        /// 静态初始化以支持进入播放模式
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

        /// <summary>
        /// 抓取场景中所有装备了CharacterSwap的角色，并将它们按顺序存储在列表中
        /// </summary>
        public virtual void UpdateList()
		{
			_characterSwapArray = FindObjectsOfType<CharacterSwap>();
			_characterSwapList = new MMCircularList<CharacterSwap>();

            // 如果PlayerID匹配，则将数组存储到列表中
            for (int i = 0; i < _characterSwapArray.Length; i++)
			{
				if (_characterSwapArray[i].PlayerID == PlayerID)
				{
					_characterSwapList.Add(_characterSwapArray[i]);
				}
			}

			if (_characterSwapList.Count == 0)
			{
				return;
			}

            // 按顺序对列表进行排序
            _characterSwapList.Sort(SortSwapsByOrder);
		}

        /// <summary>
        /// 静态方法来比较两个CharacterSwaps
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static int SortSwapsByOrder(CharacterSwap a, CharacterSwap b)
		{
			return a.Order.CompareTo(b.Order);
		}

        /// <summary>
        /// 在更新时，我们监视输入
        /// </summary>
        protected virtual void Update()
		{
			HandleInput();
		}

        /// <summary>
        /// 如果用户按下交换按钮，我们就会交换角色
        /// </summary>
        protected virtual void HandleInput()
		{
			#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
			if (Input.GetButtonDown(SwapButtonName))
			{
				SwapCharacter();
			}
			#else
			if (Keyboard.current[SwapKey].wasPressedThisFrame)
			{
				SwapCharacter();
			}
			#endif
		}

        /// <summary>
        /// 将当前角色更改为下一位
        /// </summary>
        public virtual void SwapCharacter()
		{
			if (_characterSwapList.Count < 2)
			{
				return;
			}

			int currentIndex = GetCurrentIndex();
			_characterSwapList.CurrentIndex = currentIndex;
			_characterSwapList.IncrementCurrentIndex();
			int newIndex = currentIndex;

			int i = 0;
			while (i < _characterSwapList.Count)
			{
				if (_characterSwapList.Current.enabled)
				{
					newIndex = _characterSwapList.CurrentIndex;
					break;
				}

				_characterSwapList.IncrementCurrentIndex();
				i++;
			}
			
			_characterSwapList[currentIndex].ResetCharacterSwap();
			_characterSwapList[newIndex].SwapToThisCharacter();

			LevelManager.Instance.Players[0] = _characterSwapList[newIndex].gameObject.GetComponentInParent<Character>();
			MMEventManager.TriggerEvent(_swapEvent);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
		}

        /// <summary>
        /// 找到当前活跃的角色，并将其视为当前角色
        /// </summary>
        /// <returns></returns>
        public virtual int GetCurrentIndex()
		{
			int currentIndex = -1;
			for (int i=0; i<_characterSwapList.Count; i++)
			{
				if (_characterSwapList[i].Current())
				{
					return i;
				}
			}
			return currentIndex;
		}

        /// <summary>
        /// 在关卡开始时，我们初始化列表
        /// </summary>
        /// <param name="eventType"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					UpdateList();
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}