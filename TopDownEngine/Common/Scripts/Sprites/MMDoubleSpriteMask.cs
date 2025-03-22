using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// 这个设置使用了两个精灵遮罩，在检查器中绑定，以启用一个然后禁用另一个来遮住关卡的特定部分
    /// </summary>
    public class MMDoubleSpriteMask : MonoBehaviour, MMEventListener<MMSpriteMaskEvent>
	{
		[Header("Masks遮罩")]

		/// the first sprite mask
		[Tooltip("第一个精灵遮罩")]
		public MMSpriteMask Mask1;
		/// the second sprite mask
		[Tooltip("第二个精灵遮罩")]
		public MMSpriteMask Mask2;

		protected MMSpriteMask _currentMask;
		protected MMSpriteMask _dormantMask;

        /// <summary>
        /// 在唤醒时，我们初始化遮罩
        /// </summary>
        protected virtual void Awake()
		{
			Mask1.gameObject.SetActive(true);
			Mask2.gameObject.SetActive(false);
			_currentMask = Mask1;
			_dormantMask = Mask2;
		}

        /// <summary>
        /// 为当前和休眠遮罩设置新值
        /// </summary>
        protected virtual void SwitchCurrentMask()
		{
			_currentMask = (_currentMask == Mask1) ? Mask2 : Mask1;
			_dormantMask = (_currentMask == Mask1) ? Mask2 : Mask1;
		}

        /// <summary>
        /// 一个协程，旨在激活后掩盖第一个遮罩，并将休眠的遮罩移动到新位置
        /// </summary>
        /// <param name="spriteMaskEvent"></param>
        /// <returns></returns>
        protected virtual IEnumerator DoubleMaskCo(MMSpriteMaskEvent spriteMaskEvent)
		{
			_dormantMask.transform.position = spriteMaskEvent.NewPosition;
			_dormantMask.transform.localScale = spriteMaskEvent.NewSize * _dormantMask.ScaleMultiplier;
			_dormantMask.gameObject.SetActive(true);
			yield return new WaitForSeconds(spriteMaskEvent.Duration);
			_currentMask.gameObject.SetActive(false);
			SwitchCurrentMask();
		}

        /// <summary>
        /// 当我们捕捉到一个双重遮罩事件时，我们进行处理
        /// </summary>
        /// <param name="spriteMaskEvent"></param>
        public virtual void OnMMEvent(MMSpriteMaskEvent spriteMaskEvent)
		{
			switch (spriteMaskEvent.EventType)
			{
				case MMSpriteMaskEvent.MMSpriteMaskEventTypes.DoubleMask:
					StartCoroutine(DoubleMaskCo(spriteMaskEvent));
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMSpriteMaskEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMSpriteMaskEvent>();
		}
	}
}