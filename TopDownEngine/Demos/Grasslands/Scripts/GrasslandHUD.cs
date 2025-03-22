using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
	public class GrasslandHUD : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// The playerID associated to this HUD
		[Tooltip("demo-与这个HUD相关联的玩家ID")]
		public string PlayerID = "Player1";
		/// the progress bar to use to show the healthbar
		[Tooltip("demo-用于显示生命值的进度条")]
		public MMProgressBar HealthBar;
		/// the Text comp to use to display the player name
		[Tooltip("demo-用于显示玩家名称的文本组件")]
		public Text PlayerName;
		/// the radial progress bar to put around the avatar
		[Tooltip("demo-围绕头像的径向进度条")]
		public MMProgressBar AvatarBar;
		/// the counter used to display coin amounts
		[Tooltip("demo-用于显示硬币数量的计数器")]
		public Text CoinCounter;
		/// the mask to use when the target player dies
		[Tooltip("demo-当目标玩家死亡时要使用的遮罩")]
		public CanvasGroup DeadMask;
		/// the screen to display if the target player wins
		[Tooltip("demo-如果目标玩家获胜要显示的屏幕")]
		public CanvasGroup WinnerScreen;

		protected virtual void Start()
		{
			CoinCounter.text = "0";
			DeadMask.gameObject.SetActive(false);
			WinnerScreen.gameObject.SetActive(false);
		}

		public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			switch (tdEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					if (tdEvent.OriginCharacter.PlayerID == PlayerID)
					{
						DeadMask.gameObject.SetActive(true);
						DeadMask.alpha = 0f;
						StartCoroutine(MMFade.FadeCanvasGroup(DeadMask, 0.5f, 0.8f, true));
					}
					break;
				case TopDownEngineEventTypes.Repaint:
					foreach (GrasslandsMultiplayerLevelManager.GrasslandPoints points in (LevelManager.Instance as GrasslandsMultiplayerLevelManager).Points)
					{
						if (points.PlayerID == PlayerID)
						{
							CoinCounter.text = points.Points.ToString();
						}
					}
					break;
				case TopDownEngineEventTypes.GameOver:
					if (PlayerID == (LevelManager.Instance as GrasslandsMultiplayerLevelManager).WinnerID)
					{
						WinnerScreen.gameObject.SetActive(true);
						WinnerScreen.alpha = 0f;
						StartCoroutine(MMFade.FadeCanvasGroup(WinnerScreen, 0.5f, 0.8f, true));
					}
					break;
			}
            
		}

        /// <summary>
        /// 在禁用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件。
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}