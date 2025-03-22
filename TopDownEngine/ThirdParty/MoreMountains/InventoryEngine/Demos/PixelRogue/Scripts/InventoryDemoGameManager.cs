using UnityEngine;
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 一个游戏管理器的示例，唯一的重要部分是我们在Start方法中触发加载所有物品栏。
    /// </summary>
    public class InventoryDemoGameManager : MMSingleton<InventoryDemoGameManager> 
	{
		public virtual InventoryDemoCharacter Player { get; protected set; }

        /// <summary>
        /// 静态初始化以支持进入播放模式
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

		protected override void Awake () 
		{
			base.Awake ();
			Player = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryDemoCharacter>()	;
		}

        /// <summary>
        /// 在开始时，我们触发加载事件，这将被物品栏捕获，它们会尝试加载已保存的内容
        /// </summary>
        protected virtual void Start()
		{
			MMGameEvent.Trigger("Load");
		}
	}
}