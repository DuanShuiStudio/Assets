using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如何扩展MultiplayerLevelManager以实现你自己特定规则的一个示例类。
    /// 这一个的规则如下：
    /// - 玩家可以捡硬币，一枚硬币等于一分
    /// - X秒后，游戏停止，谁获得的硬币最多谁就获胜
    /// - 如果除了一个玩家外，所有玩家都死了，游戏就会停止，获得硬币最多的玩家获胜
    /// - 游戏结束时，会显示一个“获胜者”屏幕，点击任意处的跳跃按钮可以重新启动游戏
    /// </summary>
    public class GrasslandsMultiplayerLevelManager : MultiplayerLevelManager, MMEventListener<PickableItemEvent>
	{
        /// <summary>
        /// 一种用于存储每个玩家得分的结构体。
        /// </summary>
        public struct GrasslandPoints
		{
			public string PlayerID;
			public int Points;
		}

		[Header("demo-Grasslands Bindings绑定")]
		/// An array to store each player's points
		[Tooltip("demo-一个用于存储每个玩家得分的数组")]
		public GrasslandPoints[] Points;
		/// the list of countdowns we need to update
		[Tooltip("demo-我们需要更新的倒计时列表")]
		public List<MMCountdown> Countdowns;

		[Header("demo-Grasslands Settings设置")]
		/// the duration of the game, in seconds
		[Tooltip("demo-游戏持续时间（以秒为单位）")]
		public int GameDuration = 99;
		/// 获胜者 ID
		public virtual string WinnerID { get; set; }

		protected string _playerID;
		protected bool _gameOver = false;

        /// <summary>
        /// 在初始化时，我们初始化我们的得分和倒计时
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			WinnerID = "";
			Points = new GrasslandPoints[Players.Count];
			int i = 0;
			foreach(Character player in Players)
			{
				Points[i].PlayerID = player.PlayerID;
				Points[i].Points = 0;
				i++;
			}
			foreach(MMCountdown countdown in Countdowns)
			{
				countdown.CountdownFrom = GameDuration;
				countdown.ResetCountdown();
			}
		}

        /// <summary>
        /// 每当有玩家死亡时，我们检查是否只剩下一个玩家还活着，如果是这样，我们就触发游戏结束程序
        /// </summary>
        /// <param name="playerCharacter"></param>
        protected override void OnPlayerDeath(Character playerCharacter)
		{
			base.OnPlayerDeath(playerCharacter);
			int aliveCharacters = 0;
			int i = 0;
            
			foreach(Character character in LevelManager.Instance.Players)
			{
				if (character.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead)
				{
					WinnerID = character.PlayerID;
					aliveCharacters++;
				}
				i++;
			}

			if (aliveCharacters <= 1)
			{
				StartCoroutine(GameOver());
			}
		}

        /// <summary>
        /// 游戏结束时，时间冻结并显示游戏结束屏幕
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator GameOver()
		{
			yield return new WaitForSeconds(2f);
			if (WinnerID == "")
			{
				WinnerID = "Player1";
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			_gameOver = true;
			MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
		}

        /// <summary>
        /// 在更新时，我们更新倒计时并检查是否处于游戏结束状态以及是否有输入
        /// </summary>
        public virtual void Update()
		{
			UpdateCountdown();
			CheckForGameOver();
		}

        /// <summary>
        /// 对于每个倒计时，更新剩余时间
        /// </summary>
        protected virtual void UpdateCountdown()
		{
			if (_gameOver)
			{
				return;
			}

			float remainingTime = GameDuration;
			foreach (MMCountdown countdown in Countdowns)
			{
				if (countdown.gameObject.activeInHierarchy)
				{
					remainingTime = countdown.CurrentTime;
				}
			}
			if (remainingTime <= 0f)
			{
				int maxPoints = 0;
				foreach (GrasslandPoints points in Points)
				{
					if (points.Points > maxPoints)
					{
						WinnerID = points.PlayerID;
						maxPoints = points.Points;
					}
				}
				StartCoroutine(GameOver());
			}
		}

        /// <summary>
        /// 如果我们处于游戏结束状态，检查输入并在需要时重新启动游戏。
        /// </summary>
        protected virtual void CheckForGameOver()
		{
			if (_gameOver)
			{
				if ( (Input.GetButton("Player1_Jump"))
				     || (Input.GetButton("Player2_Jump"))
				     || (Input.GetButton("Player3_Jump"))
				     || (Input.GetButton("Player4_Jump")) )
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
					MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
				}
			}
		}

        /// <summary>
        /// 当一枚硬币被捡起时，我们增加捡起它的角色的得分。
        /// </summary>
        /// <param name="pickEvent"></param>
        public virtual void OnMMEvent(PickableItemEvent pickEvent)
		{
			_playerID = pickEvent.Picker.MMGetComponentNoAlloc<Character>()?.PlayerID;
			for (int i = 0; i < Points.Length; i++)
			{
				if (Points[i].PlayerID == _playerID)
				{
					Points[i].Points++;
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.Repaint, null);
				}
			}
		}

        /// <summary>
        /// 开始监听可捡取物品的事件
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<PickableItemEvent>();
		}

        /// <summary>
        /// 停止监听可捡取物品的事件。
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable();
			this.MMEventStopListening<PickableItemEvent>();
		}
	}
}