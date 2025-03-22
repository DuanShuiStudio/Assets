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
    /// <summary>
    /// 这个类处理 Explodudes 演示场景中的获胜者屏幕显示。
    /// </summary>
    public class ExplodudesWinnerScreen : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// the ID of the player we want this screen to appear for
		[Tooltip("demo-我们希望这个屏幕出现的玩家的ID")]
		public string PlayerID = "Player1";
		/// the canvas group containing the winner screen
		[Tooltip("demo-包含获胜者屏幕的画布组")]
		public CanvasGroup WinnerScreen;

        /// <summary>
        /// 在开始时，我们确保屏幕被禁用
        /// </summary>
        protected virtual void Start()
		{
			WinnerScreen.gameObject.SetActive(false);
		}

        /// <summary>
        /// 在游戏结束时，如果需要，我们显示获胜者屏幕
        /// </summary>
        /// <param name="tdEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			switch (tdEvent.EventType)
			{
				case TopDownEngineEventTypes.GameOver:
					if (PlayerID == (LevelManager.Instance as ExplodudesMultiplayerLevelManager).WinnerID)
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