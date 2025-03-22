using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，它将能够激活/取消激活暂停
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Pause")]
	public class CharacterPause : CharacterAbility
	{
        /// 此方法仅用于在功能检查器的开头显示帮助框文本
        public override string HelpBoxText() { return "允许角色（以及控制角色的玩家）按暂停键暂停游戏。"; }
		
		[Header("Pause audio tracks暂停音频轨道")]
		/// whether or not to mute the sfx track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("是否在游戏暂停时静音音频轨道，并在游戏恢复时取消静音")]
		public bool MuteSfxTrackSounds = true;
		/// whether or not to mute the UI track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("是否在游戏暂停时静音UI轨道，并在游戏恢复时取消静音")]
		public bool MuteUITrackSounds = false;
		/// whether or not to mute the music track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("是否在游戏暂停时静音音乐轨道，并在游戏恢复时取消静音")]
		public bool MuteMusicTrackSounds = false;
		/// whether or not to mute the master track when the game pauses, and to unmute it when it unpauses 
		[Tooltip("是否在游戏暂停时静音主轨道，并在游戏恢复时取消静音")]
		public bool MuteMasterTrackSounds = false;

		[Header("Hooks钩子")] 
		/// a UnityEvent that will trigger when the game pauses 
		[Tooltip("当游戏暂停时将触发的UnityEvent")]
		public UnityEvent OnPause;
		/// a UnityEvent that will trigger when the game unpauses
		[Tooltip("当游戏取消暂停时触发的UnityEvent")]
		public UnityEvent OnUnpause;


        /// <summary>
        /// 每一帧，我们都会检查输入，看看是否需要暂停/取消游戏
        /// </summary>
        protected override void HandleInput()
		{
			if (_inputManager.PauseButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				TriggerPause();
			}
		}

        /// <summary>
        /// 如果暂停按钮被按下，我们改变暂停状态
        /// </summary>
        protected virtual void TriggerPause()
		{
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Dead)
			{
				return;
			}
			if (!AbilityAuthorized)
			{
				return;
			}
			PlayAbilityStartFeedbacks();
            // 我们为GameManager和其他可能正在监听它的类触发一个Pause事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.TogglePause, null);
		}

        /// <summary>
        /// 将字符置于暂停状态
        /// </summary>
        public virtual void PauseCharacter()
		{
			if (!this.enabled)
			{
				return;
			}
			_condition.ChangeState(CharacterStates.CharacterConditions.Paused);
			
			OnPause?.Invoke();

			if (MuteSfxTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx); }
			if (MuteUITrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.UI); }
			if (MuteMusicTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Music); }
			if (MuteMasterTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, MMSoundManager.MMSoundManagerTracks.Master); }
		}

        /// <summary>
        /// 将角色恢复到暂停前的状态。
        /// </summary>
        public virtual void UnPauseCharacter()
		{
			if (!this.enabled)
			{
				return;
			}
			_condition.RestorePreviousState();

			OnUnpause?.Invoke();

			if (MuteSfxTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Sfx); }
			if (MuteUITrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.UI); }
			if (MuteMusicTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Music); }
			if (MuteMasterTrackSounds) { MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, MMSoundManager.MMSoundManagerTracks.Master); }
		}
	}
}