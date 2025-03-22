using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一种在拾取时实例化效果并播放声音的物品拾取器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Inventory Pickable Item")]
	public class InventoryPickableItem : ItemPicker 
	{
		/// The effect to instantiate when the coin is hit
		[Tooltip("硬币被击中时要实例化（产生、触发等，具体需根据上下文确定）的效果")]
		public GameObject Effect;
		/// The sound effect to play when the object gets picked
		[Tooltip("当该物体被拾取时播放的音效")]
		public AudioClip PickSfx;

		protected override void PickSuccess()
		{
			base.PickSuccess ();
			Effects ();
		}

        /// <summary>
        /// 触发各种拾取效果
        /// </summary>
        protected virtual void Effects()
		{
			if (!Application.isPlaying)
			{
				return;
			}				
			else
			{
				if (PickSfx!=null) 
				{	
					MMSoundManagerSoundPlayEvent.Trigger(PickSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}

				if (Effect != null)
				{
                    // 在硬币的位置添加该效果的一个实例
                    Instantiate(Effect, transform.position, transform.rotation);				
				}	
			}
		}
	}
}