using UnityEngine;
using System.Collections;
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
    /// <summary>
    /// 一个可拾取的星星，如果被拾取则触发事件，并且如果它已经被收集过则禁用自己
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Deadline Star")]
	public class DeadlineStar : Star
	{
        /// <summary>
        /// 在开始时，如果需要则禁用我们的星星
        /// </summary>
        protected override void Start()
		{
			base.Start ();
			DisableIfAlreadyCollected ();
		}

        /// <summary>
        /// 如果星星已经被收集过，则禁用它
        /// </summary>
        protected virtual void DisableIfAlreadyCollected ()
		{
			foreach (DeadlineScene scene in DeadlineProgressManager.Instance.Scenes)
			{
				if (scene.SceneName == SceneManager.GetActiveScene().name)
				{
					if (scene.CollectedStars.Length >= StarID)
					{
						if (scene.CollectedStars[StarID])
						{
							Disable ();
						}
					}
				}
			}
		}

        /// <summary>
        /// 禁用这颗星星
        /// </summary>
        protected virtual void Disable()
		{
			this.gameObject.SetActive (false);
		}
	}
}