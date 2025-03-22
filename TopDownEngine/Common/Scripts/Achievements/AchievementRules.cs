using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 本课程描述了如何触发自顶向下引擎演示成就。
    /// 它扩展了基类MMAchievementRules
    /// 它监听不同的事件类型
    /// </summary>
    public class AchievementRules : MMAchievementRules, 
		MMEventListener<MMGameEvent>, 
		MMEventListener<MMCharacterEvent>, 
		MMEventListener<TopDownEngineEvent>,
		MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>,
		MMEventListener<MMStateChangeEvent<CharacterStates.CharacterConditions>>,
		MMEventListener<PickableItemEvent>,
		MMEventListener<CheckPointEvent>,
		MMEventListener<MMInventoryEvent>
	{
        /// <summary>
        /// 当我们捕获一个MMGameEvent时，我们会根据它的名字做一些事情
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public override void OnMMEvent(MMGameEvent gameEvent)
		{
			base.OnMMEvent (gameEvent);
		}

        /// <summary>
        /// 当角色事件发生时，如果是跳跃事件，我们会在JumpAround成就中添加进度
        /// </summary>
        /// <param name="characterEvent"></param>
        public virtual void OnMMEvent(MMCharacterEvent characterEvent)
		{
			if (characterEvent.TargetCharacter.CharacterType == Character.CharacterTypes.Player)
			{
				switch (characterEvent.EventType)
				{
					case MMCharacterEventTypes.Jump:
						MMAchievementManager.AddProgress ("JumpAround", 1);
						break;
				}	
			}
		}

        /// <summary>
        /// 当我们抓取TopDownEngineEvent时，如果它是PlayerDeath事件，我们就解锁了我们的成就
        /// </summary>
        /// <param name="topDownEngineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
		{
			switch (topDownEngineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					MMAchievementManager.UnlockAchievement ("DeathIsOnlyTheBeginning");
					break;
			}
		}

        /// <summary>
        /// 抓取PickableItem事件
        /// </summary>
        /// <param name="pickableItemEvent"></param>
        public virtual void OnMMEvent(PickableItemEvent pickableItemEvent)
		{
			/*if (pickableItemEvent.PickedItem.GetComponent<InventoryEngineHealth>() != null)
			{
				MMAchievementManager.UnlockAchievement ("Medic");
			}*/
		}

        /// <summary>
        /// 抓住MMStateChangeEvents
        /// </summary>
        /// <param name="movementEvent"></param>
        public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> movementEvent)
		{
			/*switch (movementEvent.NewState)
			{

			}*/
		}

        /// <summary>
        /// 抓住MMStateChangeEvents
        /// </summary>
        /// <param name="conditionEvent"></param>
        public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.CharacterConditions> conditionEvent)
		{
			/*switch (conditionEvent.NewState)
			{

			}*/
		}

        /// <summary>
        /// 获取检查点事件。如果检查点的顺序是>0，我们就可以解锁我们的成就
        /// </summary>
        /// <param name="checkPointEvent"></param>
        public virtual void OnMMEvent(CheckPointEvent checkPointEvent)
		{
			if (checkPointEvent.Order > 0)
			{
				MMAchievementManager.UnlockAchievement("SteppingStone");
			}
		}

        /// <summary>
        /// 在库存事件中，我们会根据需要解锁或添加成就进程
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
			{
				if (inventoryEvent.EventItem.ItemID == "KoalaCoin")
				{
					MMAchievementManager.AddProgress("MoneyMoneyMoney", 1);
				}
				if (inventoryEvent.EventItem.ItemID == "KoalaHealth")
				{
					MMAchievementManager.UnlockAchievement("Medic");
				}
			}
		}

        /// <summary>
        /// 启用后，我们开始监听MMGameEvents。您可能希望将其扩展为侦听其他类型的事件。
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable ();
			this.MMEventStartListening<MMCharacterEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
			this.MMEventStartListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			this.MMEventStartListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
			this.MMEventStartListening<PickableItemEvent>();
			this.MMEventStartListening<CheckPointEvent>();
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMGameEvents。您可能希望扩展它以停止侦听其他类型的事件。
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMCharacterEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
			this.MMEventStopListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			this.MMEventStopListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
			this.MMEventStopListening<PickableItemEvent>();
			this.MMEventStopListening<CheckPointEvent>();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}