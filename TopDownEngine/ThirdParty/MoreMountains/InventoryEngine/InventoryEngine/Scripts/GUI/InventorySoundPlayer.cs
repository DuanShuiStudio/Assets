using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(InventoryDisplay))]
    /// <summary>
    /// 一个在与 InventoryDisplay 配对时处理播放歌曲的组件
    /// </summary>
    public class InventorySoundPlayer : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		public enum Modes { Direct, Event }

		[Header("Settings设置")]
        /// 选择播放声音的模式。直接模式将播放音频源，事件模式将调用一个MMSfxEvent，旨在由MMSoundManager捕获 
        public Modes Mode = Modes.Direct;
		
		[Header("Sounds声音")]
		[MMInformation("在这里，你可以定义在操作这个库存时将播放的默认声音", MMInformationAttribute.InformationType.Info,false)]
        /// 打开库存时要播放的音频片段
        public AudioClip OpenFx;
        /// 关闭库存时要播放的音频片段
        public AudioClip CloseFx;
        /// 从一个槽位移动到另一个槽位时要播放的音频片段
        public AudioClip SelectionChangeFx;
        /// 从一个槽位移动到另一个槽位时要播放的音频片段
        public AudioClip ClickFX;
        /// 成功移动一个对象时要播放的音频片段
        public AudioClip MoveFX;
        /// 发生错误时（选择空槽位等）要播放的音频片段
        public AudioClip ErrorFx;
        /// 当使用某个物品时，如果没有为其定义其他声音，则播放的音频片段
        public AudioClip UseFx;
        /// 当丢弃某个物品时，如果没有为其定义其他声音，则播放的音频片段
        public AudioClip DropFx;
        /// 当装备某个物品时，如果没有为其定义其他声音，则播放的音频片段
        public AudioClip EquipFx;

		protected string _targetInventoryName;
		protected string _targetPlayerID;
		protected AudioSource _audioSource;

		/// <summary>
		/// 在启动时，我们设置播放器并获取一些引用以供将来使用
		/// </summary>
		protected virtual void Start()
		{
			SetupInventorySoundPlayer ();
			_audioSource = GetComponent<AudioSource> ();
			_targetInventoryName = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().TargetInventoryName;
			_targetPlayerID = this.gameObject.MMGetComponentNoAlloc<InventoryDisplay> ().PlayerID;
		}

        /// <summary>
        /// 设置库存声音播放器
        /// </summary>
        public virtual void SetupInventorySoundPlayer()
		{
			AddAudioSource ();			
		}

        /// <summary>
        /// 如果需要，添加一个音频源组件
        /// </summary>
        protected virtual void AddAudioSource()
		{
			if (GetComponent<AudioSource>() == null)
			{
				this.gameObject.AddComponent<AudioSource>();
			}
		}

        /// <summary>
        /// 播放参数字符串中指定的音频
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        public virtual void PlaySound(string soundFx)
		{
			if (soundFx==null || soundFx=="")
			{
				return;
			}

			AudioClip soundToPlay=null;
			float volume=1f;

			switch (soundFx)
			{
				case "error":
					soundToPlay=ErrorFx;
					volume=1f;
					break;
				case "select":
					soundToPlay=SelectionChangeFx;
					volume=0.5f;
					break;
				case "click":
					soundToPlay=ClickFX;
					volume=0.5f;
					break;
				case "open":
					soundToPlay=OpenFx;
					volume=1f;
					break;
				case "close":
					soundToPlay=CloseFx;
					volume=1f;
					break;
				case "move":
					soundToPlay=MoveFX;
					volume=1f;
					break;
				case "use":
					soundToPlay=UseFx;
					volume=1f;
					break;
				case "drop":
					soundToPlay=DropFx;
					volume=1f;
					break;
				case "equip":
					soundToPlay=EquipFx;
					volume=1f;
					break;
			}

			if (soundToPlay!=null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundToPlay,volume);	
				}
				else
				{
					MMSfxEvent.Trigger(soundToPlay, null, volume, 1);	
				}
			}
		}

        /// <summary>
        /// 以期望的音量播放参数中指定的音效
        /// </summary>
        /// <param name="soundFx">Sound fx.</param>
        /// <param name="volume">Volume.</param>
        public virtual void PlaySound(AudioClip soundFx,float volume)
		{
			if (soundFx != null)
			{
				if (Mode == Modes.Direct)
				{
					_audioSource.PlayOneShot(soundFx, volume);
				}
				else
				{
					MMSfxEvent.Trigger(soundFx, null, volume, 1);
				}
			}
		}

        /// <summary>
        /// 捕获MMInventoryEvents事件并对其采取行动，播放相应的声音
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
            // 如果此事件与我们库存显示无关，则不执行任何操作并退出
            if (inventoryEvent.TargetInventoryName != _targetInventoryName)
			{
				return;
			}

			if (inventoryEvent.PlayerID != _targetPlayerID)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					this.PlaySound("select");
					break;
				case MMInventoryEventType.Click:
					this.PlaySound("click");
					break;
				case MMInventoryEventType.InventoryOpens:
					this.PlaySound("open");
					break;
				case MMInventoryEventType.InventoryCloses:
					this.PlaySound("close");
					break;
				case MMInventoryEventType.Error:
					this.PlaySound("error");
					break;
				case MMInventoryEventType.Move:
					if (inventoryEvent.EventItem.MovedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("move"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.MovedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemEquipped:
					if (inventoryEvent.EventItem.EquippedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("equip"); }
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.EquippedSound, 1f);
					}
					break;
				case MMInventoryEventType.ItemUsed:
					if (inventoryEvent.EventItem.UsedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("use"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.UsedSound, 1f);
					}
					break;
				case MMInventoryEventType.Drop:
					if (inventoryEvent.EventItem.DroppedSound == null)
					{
						if (inventoryEvent.EventItem.UseDefaultSoundsIfNull) { this.PlaySound ("drop"); 	}
					} else
					{
						this.PlaySound (inventoryEvent.EventItem.DroppedSound, 1f);
					}
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}