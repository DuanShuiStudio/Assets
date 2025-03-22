using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，当转换到新场景时，它将保持其确切的当前状态。
    /// 它将被自动传递到新场景的LevelManager中，作为这个场景的主角。
    /// 它会保持所有组件完成关卡时的状态。
    /// 它的生命值，激活的能力，组件值，装备的武器，你可能添加的新组件等等，都将在新场景中保留一次。
    /// 动画器参数：无
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Persistence")]
	public class CharacterPersistence : CharacterAbility, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
	{
		public virtual bool Initialized { get; set; }

        /// <summary>
        /// 在Start（）中，我们防止角色在需要时被摧毁
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();

			if (AbilityAuthorized)
			{
				DontDestroyOnLoad(this.gameObject);
			}

			Initialized = true;
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			Initialized = false;
		}

        /// <summary>
        /// 当我们收到保存请求时，我们将角色存储在游戏管理器中以备将来使用
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == "Save")
			{
				SaveCharacter();
			}
		}

        /// <summary>
        /// 当我们得到一个自顶向下引擎事件时，我们对它进行操作
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			if (!AbilityAuthorized)
			{
				return;
			}

			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LoadNextScene:
					this.gameObject.SetActive(false);
					break;
				case TopDownEngineEventTypes.SpawnCharacterStarts:
					this.transform.position = LevelManager.Instance.InitialSpawnPoint.transform.position;
					this.gameObject.SetActive(true);
					Character character = this.gameObject.GetComponentInParent<Character>(); 
					character.enabled = true;
					character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
					character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
					character.SetInputManager();
					break;
				case TopDownEngineEventTypes.LevelStart:
					if (_health != null)
					{
						_health.StoreInitialPosition();    
					}
					break;
				case TopDownEngineEventTypes.RespawnComplete:
					Initialized = true;
					break;
			}
		}

        /// <summary>
        /// 保存到游戏管理器的参考我们的角色
        /// </summary>
        protected virtual void SaveCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			GameManager.Instance.PersistentCharacter = _character;
		}

        /// <summary>
        /// 清除任何可能已经存储在GameManager中的保存角色
        /// </summary>
        public virtual void ClearSavedCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			GameManager.Instance.PersistentCharacter = null;
		}

        /// <summary>
        /// 启用后，我们开始监听事件
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在disable选项中，我们停止侦听事件
        /// </summary>
        protected virtual void OnDestroy()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}