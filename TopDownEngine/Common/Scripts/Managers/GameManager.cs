using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 可能的TopDown Engine基础事件列表
    /// LevelStart : 当关卡开始时，由LevelManager触发
    ///	LevelComplete : 可在到达关卡末尾时触发
    /// LevelEnd : 同样的事情
    ///	Pause : 当暂停开始时触发
    ///	UnPause : 当暂停结束并恢复正常时触发
    ///	PlayerDeath : 当玩家角色死亡时触发
    ///	RespawnStarted : 当玩家角色重生队列开始时触发
    ///	RespawnComplete : 当玩家角色重生队列结束时触发
    ///	StarPicked : 当星星奖励被拾取时触发
    ///	GameOver : 当所有生命都丧失时，由LevelManager触发
    /// CharacterSwap : 当角色被切换时触发
    /// CharacterSwitch : 当角色被切换时触发
    /// Repaint : 触发以请求UI刷新
    /// TogglePause : 触发以请求暂停（或取消暂停）
    /// </summary>
    public enum TopDownEngineEventTypes
	{
		SpawnCharacterStarts,
		LevelStart,
		LevelComplete,
		LevelEnd,
		Pause,
		UnPause,
		PlayerDeath,
		SpawnComplete,
		RespawnStarted,
		RespawnComplete,
		StarPicked,
		GameOver,
		CharacterSwap,
		CharacterSwitch,
		Repaint,
		TogglePause,
		LoadNextScene,
		PauseNoMenu
	}

    /// <summary>
    /// 一种用于（目前）表示关卡开始和结束的信号事件类型
    /// </summary>
    public struct TopDownEngineEvent
	{
		public TopDownEngineEventTypes EventType;
		public Character OriginCharacter;

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEngineEvent"/> struct.
		/// </summary>
		/// <param name="eventType">Event type.</param>
		public TopDownEngineEvent(TopDownEngineEventTypes eventType, Character originCharacter)
		{
			EventType = eventType;
			OriginCharacter = originCharacter;
		}

		static TopDownEngineEvent e;
		public static void Trigger(TopDownEngineEventTypes eventType, Character originCharacter)
		{
			e.EventType = eventType;
			e.OriginCharacter = originCharacter;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 当前分数的修改方法列表
    /// </summary>
    public enum PointsMethods
	{
		Add,
		Set
	}

    /// <summary>
    /// 一种用于表示当前分数变化的事件类型
    /// </summary>
    public struct TopDownEnginePointEvent
	{
		public PointsMethods PointsMethod;
		public int Points;
		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.TopDownEnginePointEvent"/> struct.
		/// </summary>
		/// <param name="pointsMethod">Points method.</param>
		/// <param name="points">Points.</param>
		public TopDownEnginePointEvent(PointsMethods pointsMethod, int points)
		{
			PointsMethod = pointsMethod;
			Points = points;
		}

		static TopDownEnginePointEvent e;
		public static void Trigger(PointsMethods pointsMethod, int points)
		{
			e.PointsMethod = pointsMethod;
			e.Points = points;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 可能的暂停方法列表
    /// </summary>
    public enum PauseMethods
	{
		PauseMenu,
		NoPauseMenu
	}

    /// <summary>
    /// 一个用于存储每个关卡入口点的类，每个关卡一个
    /// </summary>
    public class PointsOfEntryStorage
	{
		public string LevelName;
		public int PointOfEntryIndex;
		public Character.FacingDirections FacingDirection;

		public PointsOfEntryStorage(string levelName, int pointOfEntryIndex, Character.FacingDirections facingDirection)
		{
			LevelName = levelName;
			FacingDirection = facingDirection;
			PointOfEntryIndex = pointOfEntryIndex;
		}
	}

    /// <summary>
    /// 游戏管理器是一个持久的单例，用于处理分数和时间。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Game Manager")]
	public class GameManager : 	MMPersistentSingleton<GameManager>, 
		MMEventListener<MMGameEvent>, 
		MMEventListener<TopDownEngineEvent>, 
		MMEventListener<TopDownEnginePointEvent>
	{
		/// the target frame rate for the game
		[Tooltip("游戏的目标帧率")]
		public int TargetFrameRate = 300;
		[Header("Lives生命值")]
		/// the maximum amount of lives the character can currently have
		[Tooltip("角色当前可以拥有的最大生命值")]
		public int MaximumLives = 0;
		/// the current number of lives 
		[Tooltip("当前的生命值")]
		public int CurrentLives = 0;

		[Header("Bindings绑定")]
		/// the name of the scene to redirect to when all lives are lost
		[Tooltip("当所有生命都丧失时要重定向到的场景名称")]
		public string GameOverScene;

		[Header("Points分数")]
		/// the current number of game points
		[MMReadOnly]
		[Tooltip("当前的游戏分数")]
		public int Points;

		[Header("Pause游戏暂停")]
		/// if this is true, the game will automatically pause when opening an inventory
		[Tooltip("如果这是真的，那么当打开物品栏时游戏将自动暂停")]
		public bool PauseGameWhenInventoryOpens = true;
        /// 如果游戏当前是暂停状态，则返回真
        public virtual bool Paused { get; set; }
        // 如果我们至少存储过一次地图位置，则返回真
        public virtual bool StoredLevelMapPosition{ get; set; }
        /// 当前的角色
        public virtual Vector2 LevelMapPosition { get; set; }
        /// 已存储的选定角色
        public virtual Character PersistentCharacter { get; set; }
		/// the list of points of entry and exit
		[Tooltip("进入和退出的点列表")]
		public List<PointsOfEntryStorage> PointsOfEntry;
        /// 已存储的选定角色
        public virtual Character StoredCharacter { get; set; }

		// storage
		protected bool _inventoryOpen = false;
		protected bool _pauseMenuOpen = false;
		protected InventoryInputManager _inventoryInputManager;
		protected int _initialMaximumLives;
		protected int _initialCurrentLives;

        /// <summary>
        /// 在Awake中，我们初始化进入点列表
        /// </summary>
        protected override void Awake()
		{
			base.Awake ();
			PointsOfEntry = new List<PointsOfEntryStorage> ();
		}

        /// <summary>
        /// 在Start()函数中，将目标帧率设置为指定的值
        /// </summary>
        protected virtual void Start()
		{
			Application.targetFrameRate = TargetFrameRate;
			_initialCurrentLives = CurrentLives;
			_initialMaximumLives = MaximumLives;
		}

        /// <summary>
        /// 这个方法重置整个游戏管理器。
        /// </summary>
        public virtual void Reset()
		{
			Points = 0;
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
			Paused = false;
		}
        /// <summary>
        /// 使用此方法来减少当前的生命值。
        /// </summary>
        public virtual void LoseLife()
		{
			CurrentLives--;
		}

        /// <summary>
        /// 当获得一个（或更多）生命时，使用此方法
        /// </summary>
        /// <param name="lives">Lives.</param>
        public virtual void GainLives(int lives)
		{
			CurrentLives += lives;
			if (CurrentLives > MaximumLives)
			{
				CurrentLives = MaximumLives;
			}
		}

        /// <summary>
        /// 使用此方法来增加最大生命值的数量，并且可以选择性地同时增加当前的生命值
        /// </summary>
        /// <param name="lives">Lives.</param>
        /// <param name="increaseCurrent">If set to <c>true</c> increase current.</param>
        public virtual void AddLives(int lives, bool increaseCurrent)
		{
			MaximumLives += lives;
			if (increaseCurrent)
			{
				CurrentLives += lives;
			}
		}

        /// <summary>
        /// 将生命值的数量重置为初始值
        /// </summary>
        public virtual void ResetLives()
		{
			CurrentLives = _initialCurrentLives;
			MaximumLives = _initialMaximumLives;
		}

        /// <summary>
        /// 将参数中的点数添加到当前游戏点数中
        /// </summary>
        /// <param name="pointsToAdd">Points to add.</param>
        public virtual void AddPoints(int pointsToAdd)
		{
			Points += pointsToAdd;
			GUIManager.Instance.RefreshPoints();
		}

        /// <summary>
        /// 使用此方法将当前点数设置为您作为参数传递的点数
        /// </summary>
        /// <param name="points">Points.</param>
        public virtual void SetPoints(int points)
		{
			Points = points;
			GUIManager.Instance.RefreshPoints();
		}

        /// <summary>
        /// 如果找到，则启用物品栏输入管理器
        /// </summary>
        /// <param name="status"></param>
        protected virtual void SetActiveInventoryInputManager(bool status)
		{
			_inventoryInputManager = GameObject.FindObjectOfType<InventoryInputManager> ();
			if (_inventoryInputManager != null)
			{
				_inventoryInputManager.enabled = status;
			}
		}

        /// <summary>
        /// 根据当前状态暂停或取消暂停游戏
        /// </summary>
        public virtual void Pause(PauseMethods pauseMethod = PauseMethods.PauseMenu, bool unpauseIfPaused = true)
		{	
			if ((pauseMethod == PauseMethods.PauseMenu) && _inventoryOpen)
			{
				return;
			}

            // 如果时间还没有停止的话	
            if (Time.timeScale>0.0f)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
				Instance.Paused=true;
				if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
				{
					GUIManager.Instance.SetPauseScreen(true);	
					_pauseMenuOpen = true;
					SetActiveInventoryInputManager (false);
				}
				if (pauseMethod == PauseMethods.NoPauseMenu)
				{
					_inventoryOpen = true;
				}
			}
			else
			{
				if (unpauseIfPaused)
				{
					UnPause(pauseMethod);	
				}
			}		
			LevelManager.Instance.ToggleCharacterPause();
		}

        /// <summary>
        ///取消暂停游戏
        /// </summary>
        public virtual void UnPause(PauseMethods pauseMethod = PauseMethods.PauseMenu)
		{
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			Instance.Paused = false;
			if ((GUIManager.HasInstance) && (pauseMethod == PauseMethods.PauseMenu))
			{ 
				GUIManager.Instance.SetPauseScreen(false);
				_pauseMenuOpen = false;
				SetActiveInventoryInputManager (true);
			}
			if (_inventoryOpen)
			{
				_inventoryOpen = false;
			}
			LevelManager.Instance.ToggleCharacterPause();
		}

        /// <summary>
        /// Stores the points of entry for the level whose name you pass as a parameter.
        /// 存储您作为参数传递的关卡名称的进入点
        /// </summary>
        /// <param name="levelName">Level name.</param>
        /// <param name="entryIndex">Entry index.</param>
        /// <param name="exitIndex">Exit index.</param>
        public virtual void StorePointsOfEntry(string levelName, int entryIndex, Character.FacingDirections facingDirection)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						point.PointOfEntryIndex = entryIndex;
						return;
					}
				}	
			}

			PointsOfEntry.Add (new PointsOfEntryStorage (levelName, entryIndex, facingDirection));
		}

        /// <summary>
        /// Gets point of entry info for the level whose scene name you pass as a parameter
        /// 获取您作为参数传递的关卡场景名称的进入点信息
        /// </summary>
        /// <returns>The points of entry.</returns>
        /// <param name="levelName">Level name.</param>
        public virtual PointsOfEntryStorage GetPointsOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						return point;
					}
				}
			}
			return null;
		}

        /// <summary>
        /// Clears the stored point of entry infos for the level whose name you pass as a parameter
        /// 清除您作为参数传递的关卡名称的存储进入点信息
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public virtual void ClearPointOfEntry(string levelName)
		{
			if (PointsOfEntry.Count > 0)
			{
				foreach (PointsOfEntryStorage point in PointsOfEntry)
				{
					if (point.LevelName == levelName)
					{
						PointsOfEntry.Remove (point);
					}
				}
			}
		}

        /// <summary>
        /// Clears all points of entry.
        /// 清除所有进入点
        /// </summary>
        public virtual void ClearAllPointsOfEntry()
		{
			PointsOfEntry.Clear ();
		}

        /// <summary>
        /// 删除所有保存文件
        /// </summary>
        public virtual void ResetAllSaves()
		{
			MMSaveLoadManager.DeleteSaveFolder("InventoryEngine");
			MMSaveLoadManager.DeleteSaveFolder("TopDownEngine");
			MMSaveLoadManager.DeleteSaveFolder("MMAchievements");
		}

        /// <summary>
        /// 为即将到来的关卡存储选定的角色
        /// </summary>
        /// <param name="selectedCharacter">Selected character.</param>
        public virtual void StoreSelectedCharacter(Character selectedCharacter)
		{
			StoredCharacter = selectedCharacter;
		}

        /// <summary>
        /// 清除选定的角色
        /// </summary>
        public virtual void ClearSelectedCharacter()
		{
			StoredCharacter = null;
		}

        /// <summary>
        /// 设置一个新的持久角色
        /// </summary>
        /// <param name="newCharacter"></param>
        public virtual void SetPersistentCharacter(Character newCharacter)
		{
			PersistentCharacter = newCharacter;
		}

        /// <summary>
        /// 如果存在，则销毁一个持久角色。
        /// </summary>
        public virtual void DestroyPersistentCharacter()
		{
			if (PersistentCharacter != null)
			{
				Destroy(PersistentCharacter.gameObject);
				SetPersistentCharacter(null);
			}
			

			if (LevelManager.Instance.Players[0] != null)
			{
				if (LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<CharacterPersistence>() != null)
				{
					Destroy(LevelManager.Instance.Players[0].gameObject);	
				}
			}
		}

        /// <summary>
        /// 捕获MMGameEvents并对其采取行动，播放相应的声音。
        /// </summary>
        /// <param name="gameEvent">MMGameEvent event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "inventoryOpens":
					if (PauseGameWhenInventoryOpens)
					{
						Pause(PauseMethods.NoPauseMenu, false);
					}					
					break;

				case "inventoryCloses":
					if (PauseGameWhenInventoryOpens)
					{
						UnPause(PauseMethods.NoPauseMenu);
					}
					break;
			}
		}

        /// <summary>
        /// 捕获TopDownEngineEvents并对其采取行动，播放相应的声音
        /// </summary>
        /// <param name="engineEvent">TopDownEngineEvent event.</param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.TogglePause:
					if (Paused)
					{
						TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
					}
					else
					{
						TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Pause, null);
					}
					break;
				case TopDownEngineEventTypes.Pause:
					Pause ();
					break;
				case TopDownEngineEventTypes.UnPause:
					UnPause ();
					break;
				case TopDownEngineEventTypes.PauseNoMenu:
					Pause(PauseMethods.NoPauseMenu, false);
					break;
			}
		}

        /// <summary>
        /// 捕获TopDownEnginePointsEvents并对其采取行动，播放相应的声音
        /// </summary>
        /// <param name="pointEvent">TopDownEnginePointEvent event.</param>
        public virtual void OnMMEvent(TopDownEnginePointEvent pointEvent)
		{
			switch (pointEvent.PointsMethod)
			{
				case PointsMethods.Set:
					SetPoints(pointEvent.Points);
					break;

				case PointsMethods.Add:
					AddPoints(pointEvent.Points);
					break;
			}
		}

        /// <summary>
        /// 在启用状态下，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent> ();
			this.MMEventStartListening<TopDownEngineEvent> ();
			this.MMEventStartListening<TopDownEnginePointEvent> ();
		}

        /// <summary>
        /// 在禁用状态下，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent> ();
			this.MMEventStopListening<TopDownEngineEvent> ();
			this.MMEventStopListening<TopDownEnginePointEvent> ();
		}
	}
}