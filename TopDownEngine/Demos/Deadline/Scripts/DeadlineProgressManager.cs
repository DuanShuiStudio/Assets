using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{	
	[System.Serializable]
    /// <summary>
    /// 一个可序列化的实体，用于存储Deadline演示场景、它们是否已完成、是否已解锁以及可以收集和已经收集的星星数量
    /// </summary>
    public class DeadlineScene
	{
		public string SceneName;
		public bool LevelComplete = false;
		public bool LevelUnlocked = false;
		public int MaxStars;
		public bool[] CollectedStars;
	}

	[System.Serializable]
    /// <summary>
    /// 一个用于存储进度的可序列化实体：一个包含其内部状态（见上文）的场景列表、剩余的生命数量以及我们拥有多少生命
    /// </summary>
    public class DeadlineProgress
	{
		public string StoredCharacterName;
		public int InitialMaximumLives = 0;
		public int InitialCurrentLives = 0;
		public int MaximumLives = 0;
		public int CurrentLives = 0;
		public DeadlineScene[] Scenes;
		public string[] Collectibles;
	}

    /// <summary>
    /// DeadlineProgressManager类展示了如何在您的游戏中实现进度管理的一个示例。
    /// 引擎中没有通用的类来实现这一点，原因很简单，没有两个游戏想要保存完全相同的内容
    /// 但这应该展示如何实现它，然后你可以复制并粘贴到你自己类中（或者根据你的喜好扩展这个类）
    /// </summary>
    public class DeadlineProgressManager : MMSingleton<DeadlineProgressManager>, MMEventListener<TopDownEngineEvent>, MMEventListener<TopDownEngineStarEvent>
	{
		public virtual int InitialMaximumLives { get; set; }
		public virtual int InitialCurrentLives { get; set; }

		[Header("demo-Characters角色")] 
		public Character Naomi;
		public Character Jules;

		/// the list of scenes that we'll want to consider for our game
		[Tooltip("demo-我们想要为游戏考虑的场景列表")]
		public DeadlineScene[] Scenes;

		[MMInspectorButton("CreateSaveGame")]
        /// 一个用于测试创建保存文件的测试按钮
        public bool CreateSaveGameBtn;

        /// 当前收集到的星星数量
        public virtual int CurrentStars { get; protected set; }
		
		public virtual List<string> FoundCollectibles { get; protected set; }

		protected const string _saveFolderName = "DeadlineProgressData";
		protected const string _saveFileName = "Progress.data";

        /// <summary>
        /// 静态初始化以支持进入游戏模式
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

        /// <summary>
        /// 在唤醒时，我们加载进度并初始化星星计数器
        /// </summary>
        protected override void Awake()
		{
			base.Awake ();
			LoadSavedProgress ();
			InitializeStars ();
			if (FoundCollectibles == null)
			{
				FoundCollectibles = new List<string> ();
			}
		}

        /// <summary>
        /// 当关卡完成时，我们更新进度
        /// </summary>
        protected virtual void LevelComplete()
		{
			for (int i = 0; i < Scenes.Length; i++)
			{
				if (Scenes[i].SceneName == SceneManager.GetActiveScene().name)
				{
					Scenes[i].LevelComplete = true;
					Scenes[i].LevelUnlocked = true;
					if (i < Scenes.Length - 1)
					{
						Scenes [i + 1].LevelUnlocked = true;
					}
				}
			}
		}

        /// <summary>
        /// 遍历进度列表中的所有场景，并更新已收集星星计数器
        /// </summary>
        protected virtual void InitializeStars()
		{
			foreach (DeadlineScene scene in Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					int stars = 0;
					foreach (bool star in scene.CollectedStars)
					{
						if (star) { stars++; }
					}
					CurrentStars = stars;
				}
			}
		}

        /// <summary>
        /// 将进度保存到文件中
        /// </summary>
        protected virtual void SaveProgress()
		{
			DeadlineProgress progress = new DeadlineProgress ();
			progress.StoredCharacterName = GameManager.Instance.StoredCharacter.name;
			progress.MaximumLives = GameManager.Instance.MaximumLives;
			progress.CurrentLives = GameManager.Instance.CurrentLives;
			progress.InitialMaximumLives = InitialMaximumLives;
			progress.InitialCurrentLives = InitialCurrentLives;
			progress.Scenes = Scenes;
			if (FoundCollectibles != null)
			{
				progress.Collectibles = FoundCollectibles.ToArray();	
			}

			MMSaveLoadManager.Save(progress, _saveFileName, _saveFolderName);
		}

        /// <summary>
        /// 从检查器中创建一个测试保存文件的测试方法
        /// </summary>
        protected virtual void CreateSaveGame()
		{
			SaveProgress();
		}

        /// <summary>
        /// 将保存的进度加载到内存中
        /// </summary>
        protected virtual void LoadSavedProgress()
		{
			DeadlineProgress progress = (DeadlineProgress)MMSaveLoadManager.Load(typeof(DeadlineProgress), _saveFileName, _saveFolderName);
			if (progress != null)
			{
				GameManager.Instance.StoredCharacter = (progress.StoredCharacterName == Jules.name) ? Jules : Naomi;
				GameManager.Instance.MaximumLives = progress.MaximumLives;
				GameManager.Instance.CurrentLives = progress.CurrentLives;
				InitialMaximumLives = progress.InitialMaximumLives;
				InitialCurrentLives = progress.InitialCurrentLives;
				Scenes = progress.Scenes;
				if (progress.Collectibles != null)
				{
					FoundCollectibles = new List<string>(progress.Collectibles);	
				}
			}
			else
			{
				InitialMaximumLives = GameManager.Instance.MaximumLives;
				InitialCurrentLives = GameManager.Instance.CurrentLives;
			}
		}

		public virtual void FindCollectible(string collectibleName)
		{
			FoundCollectibles.Add(collectibleName);
		}

        /// <summary>
        /// 当我们获取到一个星星事件时，我们相应地更新场景状态
        /// </summary>
        /// <param name="deadlineStarEvent">Deadline star event.</param>
        public virtual void OnMMEvent(TopDownEngineStarEvent deadlineStarEvent)
		{
			foreach (DeadlineScene scene in Scenes)
			{
				if (scene.SceneName == deadlineStarEvent.SceneName)
				{
					scene.CollectedStars [deadlineStarEvent.StarID] = true;
					CurrentStars++;
				}
			}
		}

        /// <summary>
        /// 当我们获取到一个关卡完成事件时，我们更新状态，并将进度保存到文件中
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public virtual void OnMMEvent(TopDownEngineEvent gameEvent)
		{
			switch (gameEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelComplete:
					LevelComplete ();
					SaveProgress ();
					break;
				case TopDownEngineEventTypes.GameOver:
					GameOver ();
					break;
			}
		}

        /// <summary>
        /// 这个方法描述了玩家失去所有生命时会发生什么。在这种情况下，我们重置进度和所有生命
        /// </summary>
        protected virtual void GameOver()
		{
			ResetProgress ();
			ResetLives ();
		}

        /// <summary>
        /// 将生命数量重置为其初始值
        /// </summary>
        protected virtual void ResetLives()
		{
			GameManager.Instance.MaximumLives = InitialMaximumLives;
			GameManager.Instance.CurrentLives = InitialCurrentLives;
		}

        /// <summary>
        /// 一个用于移除与进度相关的所有保存文件的方法
        /// </summary>
        public virtual void ResetProgress()
		{
			MMSaveLoadManager.DeleteSaveFolder (_saveFolderName);			
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineStarEvent> ();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineStarEvent> ();
			this.MMEventStopListening<TopDownEngineEvent>();
		}		
	}
}