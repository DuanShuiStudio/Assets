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
    /// 一个示例类，展示了如何扩展 MultiplayerLevelManager 以实现你自己的特定规则。
    /// 这个示例类的规则如下：
    /// - 如果除了一个玩家外，其他所有玩家都死了，那么游戏停止，获得最多硬币的玩家获胜
    /// - 游戏结束时，会显示一个获胜者屏幕，点击任意地方的跳跃按钮可以重新启动游戏
    /// </summary>
    public class ExplodudesMultiplayerLevelManager : MultiplayerLevelManager
	{
		[Header("demo-Explodudes Settings导出设置")]
		/// the duration of the game, in seconds
		[Tooltip("demo-游戏持续时间（以秒为单位）")]
		public int GameDuration = 99;
        /// 获胜者的ID
        public virtual string WinnerID { get; set; }

		protected string _playerID;
		protected bool _gameOver = false;

        /// <summary>
        /// 在初始化时，我们初始化我们的分数和倒计时。
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			WinnerID = "";
		}

        /// <summary>
        /// 每当有玩家死亡时，我们检查是否只剩下一个玩家存活，如果是，我们就触发游戏结束的例行程序。
        /// </summary>
        /// <param name="playerCharacter"></param>
        protected override void OnPlayerDeath(Character playerCharacter)
		{
			base.OnPlayerDeath(playerCharacter);
			int aliveCharacters = 0;
			int i = 0;

			foreach (Character character in LevelManager.Instance.Players)
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
        /// 游戏结束时，时间冻结并显示游戏结束屏幕。
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
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.GameOver, null);
		}

        /// <summary>
        /// 在更新时，我们更新倒计时并检查输入（如果我们处于游戏结束状态）。
        /// </summary>
        public virtual void Update()
		{
			CheckForGameOver();
		}

        /// <summary>
        /// 如果我们处于游戏结束状态，检查输入并在需要时重新启动游戏。
        /// </summary>
        protected virtual void CheckForGameOver()
		{
			if (_gameOver)
			{
				if ((Input.GetButton("Player1_Jump"))
				    || (Input.GetButton("Player2_Jump"))
				    || (Input.GetButton("Player3_Jump"))
				    || (Input.GetButton("Player4_Jump")))
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, 1f, 0f, false, 0f, true);
					MMSceneLoadingManager.LoadScene(SceneManager.GetActiveScene().name);
				}
			}
		}

        /// <summary>
        /// 开始监听可拾取物品事件
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
		}

        /// <summary>
        /// 停止监听可拾取物品事件。
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable();
		}
	}
}