using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;


namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 生成玩家，处理检查点和重生
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Level Manager")]
	public class LevelManager : MMSingleton<LevelManager>, MMEventListener<TopDownEngineEvent>
	{
        /// 您想要用于玩家的预制件
        [Header("Instantiate Characters实例化角色")]
		[MMInformation("关卡管理器负责处理生成/重生、检查点管理和关卡边界。在这里，您可以为您的关卡定义一个或多个可玩角色", MMInformationAttribute.InformationType.Info,false)]
		/// should the player IDs be auto attributed (usually yes)
		[Tooltip("玩家ID是否应该自动分配（通常是肯定的）")]
		public bool AutoAttributePlayerIDs = true;
		/// the list of player prefabs to instantiate
		[Tooltip("这个关卡管理器将在开始时实例化的玩家预制件列表")]
		public Character[] PlayerPrefabs ;

		[Header("Characters already in the scene已经在场景中的角色")]
		[MMInformation("建议由关卡管理器来实例化你的角色，但如果你更愿意让它们已经出现在场景中，只需将它们绑定在下面的列表中", MMInformationAttribute.InformationType.Info, false)]
		/// a list of Characters already present in the scene before runtime. If this list is filled, PlayerPrefabs will be ignored
		[Tooltip("在运行时之前已经存在于场景中的角色列表。如果此列表已满，将忽略PlayerPrefabs（玩家预制件）")]
		public List<Character> SceneCharacters;

		[Header("Checkpoints检查点")]
		/// the checkpoint to use as initial spawn point if no point of entry is specified
		[Tooltip("如果没有指定入口点，则使用该检查点作为初始生成点")]
		public CheckPoint InitialSpawnPoint;
		/// the currently active checkpoint (the last checkpoint passed by the player)
		[Tooltip("当前活动的检查点（玩家最后通过的检查点）")]
		public CheckPoint CurrentCheckpoint;

		[Header("Points of Entry入口点")]
		/// A list of this level's points of entry, which can be used from other levels as initial targets
		[Tooltip("这个关卡的入口点列表，可以从其他关卡用作初始目标")]
		public Transform[] PointsOfEntry;
        				
		[Space(10)]
		[Header("Intro and Outro durations介绍（Intro）和结束（Outro）的持续时间")]
		[MMInformation("在这里，您可以指定关卡开始和结束时淡入和淡出的长度。您还可以确定重生之前的延迟", MMInformationAttribute.InformationType.Info,false)]
		/// duration of the initial fade in (in seconds)
		[Tooltip("初始淡入的持续时间（以秒为单位）")]
		public float IntroFadeDuration=1f;

		public float SpawnDelay = 0f;
		/// duration of the fade to black at the end of the level (in seconds)
		[Tooltip("关卡结束时淡出到黑色的持续时间（以秒为单位）")]
		public float OutroFadeDuration=1f;
		/// the ID to use when triggering the event (should match the ID on the fader you want to use)
		[Tooltip("触发事件时要使用的ID（应与您想要使用的淡入淡出效果的ID相匹配）")]
		public int FaderID = 0;
		/// the curve to use for in and out fades
		[Tooltip("用于淡入和淡出的曲线")]
		public MMTweenType FadeCurve = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);
		/// duration between a death of the main character and its respawn
		[Tooltip("主角死亡到重生之间的持续时间")]
		public float RespawnDelay = 2f;

		[Header("Respawn Loop重生循环")]
		/// the delay, in seconds, before displaying the death screen once the player is dead
		[Tooltip("在玩家死亡后显示死亡屏幕之前的延迟时间（以秒为单位）")]
		public float DelayBeforeDeathScreen = 1f;

		[Header("Bounds边界")]
		/// if this is true, this level will use the level bounds defined on this LevelManager. Set it to false when using the Rooms system.
		[Tooltip("如果设置为true，这个关卡将使用这个LevelManager上定义的关卡边界。当使用Rooms系统时，将其设置为false")]
		public bool UseLevelBounds = true;
        
		[Header("Scene Loading场景加载")]
		/// the method to use to load the destination level
		[Tooltip("用于加载目标关卡的方法")]
		public MMLoadScene.LoadingSceneModes LoadingSceneMode = MMLoadScene.LoadingSceneModes.MMSceneLoadingManager;
		/// the name of the MMSceneLoadingManager scene you want to use
		[Tooltip("您想要使用的MMSceneLoadingManager场景的名称")]
		[MMEnumCondition("LoadingSceneMode", (int) MMLoadScene.LoadingSceneModes.MMSceneLoadingManager)]
		public string LoadingSceneName = "LoadingScreen";
		/// the settings to use when loading the scene in additive mode
		[Tooltip("在累加模式下加载场景时要使用的设置")]
		[MMEnumCondition("LoadingSceneMode", (int)MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager)]
		public MMAdditiveSceneLoadingManagerSettings AdditiveLoadingSettings; 
		
		[Header("Feedbacks反馈")] 
		/// if this is true, an event will be triggered on player instantiation to set the range target of all feedbacks to it
		[Tooltip("如果设置为true，在玩家实例化时将触发一个事件，以将所有反馈的范围目标设置为它（玩家实例）")]
		public bool SetPlayerAsFeedbackRangeCenter = false;

        /// 关卡限制，相机和玩家不会超过这个点
        public virtual Bounds LevelBounds {  get { return (_collider==null)? new Bounds(): _collider.bounds; } }
		public virtual Collider BoundsCollider { get; protected set; }
		public virtual Collider2D BoundsCollider2D { get; protected set; }

        /// 自关卡开始以来经过的时间
        public virtual TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}

        // 私有物品
        public virtual List<CheckPoint> Checkpoints { get; protected set; }
		public virtual List<Character> Players { get; protected set; }

		protected DateTime _started;
		protected int _savedPoints;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected Vector3 _initialSpawnPointPosition;

        /// <summary>
        /// 静态初始化以支持进入播放模式
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

        /// <summary>
        /// 在唤醒时实例化玩家
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			_collider = this.GetComponent<Collider>();
			_collider2D = this.GetComponent<Collider2D>();
		}

        /// <summary>
        /// 在开始时，我们获取依赖项并初始化生成
        /// </summary>
        protected virtual void Start()
		{
			StartCoroutine(InitializationCoroutine());
		}

		protected virtual IEnumerator InitializationCoroutine()
		{
			if (SpawnDelay > 0f)
			{
				yield return MMCoroutine.WaitFor(SpawnDelay);    
			}

			BoundsCollider = _collider;
			BoundsCollider2D = _collider2D;
			InstantiatePlayableCharacters();

			if (UseLevelBounds)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.SetConfiner, null, BoundsCollider, BoundsCollider2D);
			}            
            
			if (Players == null || Players.Count == 0) { yield break; }

			Initialization();

			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnCharacterStarts, null);

            // 我们处理角色的生成
            if (Players.Count == 1)
			{
				SpawnSingleCharacter();
			}
			else
			{
				SpawnMultipleCharacters ();
			}

			CheckpointAssignment();

            // 我们触发一个淡入/淡出效果
            MMFadeOutEvent.Trigger(IntroFadeDuration, FadeCurve, FaderID);

            // 我们触发一个关卡开始事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelStart, null);
			MMGameEvent.Trigger("Load");

			if (SetPlayerAsFeedbackRangeCenter)
			{
				MMSetFeedbackRangeCenterEvent.Trigger(Players[0].transform);
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.SetTargetCharacter, Players[0]);
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);
			MMGameEvent.Trigger("CameraBound");
		}

        /// <summary>
        /// 每个多玩家关卡管理器应覆盖的方法，用于描述如何生成角色
        /// </summary>
        protected virtual void SpawnMultipleCharacters()
		{

		}

        /// <summary>
        /// 基于关卡管理器检查器中指定的PlayerPrefabs列表中的预制件实例化可玩角色
        /// </summary>
        protected virtual void InstantiatePlayableCharacters()
		{
			_initialSpawnPointPosition = (InitialSpawnPoint == null) ? Vector3.zero : InitialSpawnPoint.transform.position;
			
			Players = new List<Character> ();

			if (GameManager.Instance.PersistentCharacter != null)
			{
				Players.Add(GameManager.Instance.PersistentCharacter);
				return;
			}

            // 我们检查游戏管理器中是否有应该实例化的角色
            if (GameManager.Instance.StoredCharacter != null)
			{
				Character newPlayer = Instantiate(GameManager.Instance.StoredCharacter, _initialSpawnPointPosition, Quaternion.identity);
				newPlayer.name = GameManager.Instance.StoredCharacter.name;
				Players.Add(newPlayer);
				return;
			}

			if ((SceneCharacters != null) && (SceneCharacters.Count > 0))
			{
				foreach (Character character in SceneCharacters)
				{
					Players.Add(character);
				}
				return;
			}

			if (PlayerPrefabs == null) { return; }

            // 玩家实例化
            if (PlayerPrefabs.Length != 0)
			{ 
				foreach (Character playerPrefab in PlayerPrefabs)
				{
					Character newPlayer = Instantiate (playerPrefab, _initialSpawnPointPosition, Quaternion.identity);
					newPlayer.name = playerPrefab.name;
					Players.Add(newPlayer);

					if (playerPrefab.CharacterType != Character.CharacterTypes.Player)
					{
						Debug.LogWarning ("LevelManager : The Character you've set in the LevelManager isn't a Player, which means it's probably not going to move. You can change that in the Character component of your prefab.");
					}
				}
			}
		}

        /// <summary>
        /// 将场景中所有可重生的对象标记到它们的检查点
        /// </summary>
        protected virtual void CheckpointAssignment()
		{
            // 我们获取场景中所有可重生的对象，并将它们归因到相应的检查点
            IEnumerable<Respawnable> listeners = FindObjectsOfType<MonoBehaviour>(true).OfType<Respawnable>();
			AutoRespawn autoRespawn;
			foreach (Respawnable listener in listeners)
			{
				for (int i = Checkpoints.Count - 1; i >= 0; i--)
				{
					autoRespawn = (listener as MonoBehaviour).GetComponent<AutoRespawn>();
					if (autoRespawn == null)
					{
						Checkpoints[i].AssignObjectToCheckPoint(listener);
						continue;
					}
					else
					{
						if (autoRespawn.IgnoreCheckpointsAlwaysRespawn)
						{
							Checkpoints[i].AssignObjectToCheckPoint(listener);
							continue;
						}
						else
						{
							if (autoRespawn.AssociatedCheckpoints.Contains(Checkpoints[i]))
							{
								Checkpoints[i].AssignObjectToCheckPoint(listener);
								continue;
							}
							continue;
						}
					}
				}
			}
		}


        /// <summary>
        /// 获取当前相机、关卡编号、开始时间等
        /// </summary>
        protected virtual void Initialization()
		{
			Checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(o => o.CheckPointOrder).ToList();
			_savedPoints =GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

        /// <summary>
        /// 将一个可玩角色生成到场景中
        /// </summary>
        protected virtual void SpawnSingleCharacter()
		{
			PointsOfEntryStorage point = GameManager.Instance.GetPointsOfEntry(SceneManager.GetActiveScene().name);
			if ((point != null) && (PointsOfEntry.Length >= (point.PointOfEntryIndex + 1)))
			{
				Players[0].RespawnAt(PointsOfEntry[point.PointOfEntryIndex], point.FacingDirection);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

			if (InitialSpawnPoint != null)
			{
				InitialSpawnPoint.SpawnPlayer(Players[0]);
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, Players[0]);
				return;
			}

		}

        /// <summary>
        /// 将玩家带到指定的关卡
        /// </summary>
        /// <param name="levelName">Level name.</param>
        public virtual void GotoLevel(string levelName)
		{
			TriggerEndLevelEvents();
			StartCoroutine(GotoLevelCo(levelName));
		}

        /// <summary>
        ///触发关卡结束事件
        /// </summary>
        public virtual void TriggerEndLevelEvents()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelEnd, null);
			MMGameEvent.Trigger("Save");
		}

        /// <summary>
        /// 等待一小段时间，然后加载指定的关卡
        /// </summary>
        /// <returns>The level co.</returns>
        /// <param name="levelName">Level name.</param>
        protected virtual IEnumerator GotoLevelCo(string levelName)
		{
			if (Players != null && Players.Count > 0)
			{ 
				foreach (Character player in Players)
				{
					player.Disable ();	
				}	    		
			}

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID);
            
			if (Time.timeScale > 0.0f)
			{ 
				yield return new WaitForSeconds(OutroFadeDuration);
			}
            //我们为GameManager（以及其他可能的类）触发一个暂停事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LoadNextScene, null);

			string destinationScene = (string.IsNullOrEmpty(levelName)) ? "StartScreen" : levelName;

			switch (LoadingSceneMode)
			{
				case MMLoadScene.LoadingSceneModes.UnityNative:
					SceneManager.LoadScene(destinationScene);			        
					break;
				case MMLoadScene.LoadingSceneModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(destinationScene, LoadingSceneName);
					break;
				case MMLoadScene.LoadingSceneModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(levelName, AdditiveLoadingSettings);
					break;
			}
		}

        /// <summary>
        /// 杀死玩家
        /// </summary>
        public virtual void PlayerDead(Character playerCharacter)
		{
			if (Players.Count < 2)
			{
				StartCoroutine (PlayerDeadCo ());
			}
		}

        /// <summary>
        /// 在短暂延迟后触发死亡屏幕显示
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayerDeadCo()
		{
			yield return new WaitForSeconds(DelayBeforeDeathScreen);

			GUIManager.Instance.SetDeathScreen(true);
		}

        /// <summary>
        /// 初始化重生
        /// </summary>
        protected virtual void Respawn()
		{
			if (Players.Count < 2)
			{
				StartCoroutine(SoloModeRestart());
			}
		}

        /// <summary>
        /// 协程，用于杀死玩家、停止相机、重置分数
        /// </summary>
        /// <returns>The player co.</returns>
        protected virtual IEnumerator SoloModeRestart()
		{
			if ((PlayerPrefabs.Length <= 0) && (SceneCharacters.Count <= 0))
			{
				yield break;
			}

            // 如果我们已经设置了游戏管理器来使用生命值（意味着我们的最大生命值大于零）
            if (GameManager.Instance.MaximumLives > 0)
			{
                // 我们会失去一条生命
                GameManager.Instance.LoseLife();
                // 如果我们没有生命值了，我们检查是否有一个出口场景，并移动到那里
                if (GameManager.Instance.CurrentLives <= 0)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
					if ((GameManager.Instance.GameOverScene != null) && (GameManager.Instance.GameOverScene != ""))
					{
						MMSceneLoadingManager.LoadScene(GameManager.Instance.GameOverScene);
					}
				}
			}

			MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);

			MMFadeInEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);
			yield return new WaitForSeconds(OutroFadeDuration);

			yield return new WaitForSeconds(RespawnDelay);
			GUIManager.Instance.SetPauseScreen(false);
			GUIManager.Instance.SetDeathScreen(false);
			MMFadeOutEvent.Trigger(OutroFadeDuration, FadeCurve, FaderID, true, Players[0].transform.position);

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = InitialSpawnPoint;
			}

			if (Players[0] == null)
			{
				InstantiatePlayableCharacters();
			}

			if (CurrentCheckpoint != null)
			{
				CurrentCheckpoint.SpawnPlayer(Players[0]);
			}
			else
			{
				Debug.LogWarning("LevelManager : no checkpoint or initial spawn point has been defined, can't respawn the Player.");
			}

			_started = DateTime.UtcNow;
			
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

            // 我们为GameManager发送一个新的分数事件（以及其他可能监听此事件的类）
            TopDownEnginePointEvent.Trigger(PointsMethods.Set, 0);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnComplete, Players[0]);
			yield break;
		}


        /// <summary>
        /// 角色暂停
        /// </summary>
        public virtual void ToggleCharacterPause()
		{
			foreach (Character player in Players)
			{
				CharacterPause characterPause = player.FindAbility<CharacterPause>();
				if (characterPause == null)
				{
					break;
				}

				if (GameManager.Instance.Paused)
				{
					characterPause.PauseCharacter();
				}
				else
				{
					characterPause.UnPauseCharacter();
				}
			}
		}

        /// <summary>
        /// 释放角色
        /// </summary>
        public virtual void FreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.Freeze();
			}
		}

        /// <summary>
        /// 取消释放角色
        /// </summary>
        public virtual void UnFreezeCharacters()
		{
			foreach (Character player in Players)
			{
				player.UnFreeze();
			}
		}

        /// <summary>
        /// 将当前检查点设置为参数中指定的检查点。此检查点将被保存，并在玩家死亡时使用
        /// </summary>
        /// <param name="newCheckPoint"></param>
        public virtual void SetCurrentCheckpoint(CheckPoint newCheckPoint)
		{
			if (newCheckPoint.ForceAssignation)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}

			if (CurrentCheckpoint == null)
			{
				CurrentCheckpoint = newCheckPoint;
				return;
			}
			if (newCheckPoint.CheckPointOrder >= CurrentCheckpoint.CheckPointOrder)
			{
				CurrentCheckpoint = newCheckPoint;
			}
		}

        /// <summary>
        /// 捕捉TopDownEngineEvents事件并对其采取行动，播放相应的声音
        /// </summary>
        /// <param name="engineEvent">TopDownEngineEvent event.</param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					PlayerDead(engineEvent.OriginCharacter);
					break;
				case TopDownEngineEventTypes.RespawnStarted:
					Respawn();
					break;
			}
		}

        /// <summary>
        /// 在OnDisable中，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在OnDisable中，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}