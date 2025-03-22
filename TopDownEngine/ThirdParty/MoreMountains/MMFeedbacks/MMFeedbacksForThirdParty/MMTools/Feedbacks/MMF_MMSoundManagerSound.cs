using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{    
	/// <summary>
	/// This feedback will let you play a sound via the MMSoundManager. You will need a game object in your scene with a MMSoundManager object on it for this to work.
	/// </summary>
	[ExecuteAlways]
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Sound")]
	[FeedbackHelp("此反馈将使你能够通过MM声音管理器播放声音。要使此功能生效，你的场景中需要有一个游戏对象，并且该游戏对象上需带有一个MM声音管理器对象。 ")]
	public class MMF_MMSoundManagerSound : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		public override bool EvaluateRequiresSetup()
		{
			bool requiresSetup = false;
			if (Sfx == null)
			{
				requiresSetup = true;
			}
			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				requiresSetup = false;
				foreach (AudioClip clip in RandomSfx)
				{
					if (clip == null)
					{
						requiresSetup = true;
					}
				}    
			}
			if (SoundDataSO != null)
			{
				requiresSetup = false;
			}
			return requiresSetup;
		}
		public override string RequiredTargetText { get { return Sfx != null ? Sfx.name + " - ID:" + ID : "";  } }

		public override string RequiresSetupText { get { return "此反馈要求你在下方的音效（Sfx）插槽中设置一个音频剪辑，或者在随机音效（Random Sfx）数组中设置一个或多个音频剪辑。 "; } }
		#endif

		/// 此反馈的持续时间就是正在播放的音频剪辑的时长。 
		public override float FeedbackDuration { get { return GetDuration(); } }
		public override bool HasRandomness => true;
        
		[MMFInspectorGroup("Sound", true, 14, true)]
		/// the sound clip to play
		[Tooltip("要播放的声音片段")]
		public AudioClip Sfx;

		[MMFInspectorGroup("Random Sound", true, 34, true)]
        
		/// an array to pick a random sfx from
		[Tooltip("一个用于从中随机选取音效（Sfx）的数组 ")]
		public AudioClip[] RandomSfx;
		/// if this is true, random sfx audio clips will be played in sequential order instead of at random
		[Tooltip("如果这为真，随机音效音频片段将按顺序依次播放，而不是随机播放。 ")]
		public bool SequentialOrder = false;
		/// if we're in sequential order, determines whether or not to hold at the last index, until either a cooldown is met, or the ResetSequentialIndex method is called
		[Tooltip("如果我们处于按顺序播放的状态，该参数用于确定是否停留在最后一个索引位置，直到达到冷却时间，或者调用了“ResetSequentialIndex（重置顺序索引）”方法为止。 ")]
		[MMFCondition("SequentialOrder", true)]
		public bool SequentialOrderHoldLast = false;
		/// if we're in sequential order hold last mode, index will reset to 0 automatically after this duration, unless it's 0, in which case it'll be ignored
		[Tooltip("如果我们处于按顺序播放且保持在最后一个的模式下，在此时间段过后，索引将自动重置为0，除非索引已经是0，在这种情况下，该重置操作将被忽略。 ")]
		[MMFCondition("SequentialOrderHoldLast", true)]
		public float SequentialOrderHoldCooldownDuration = 2f;
		/// if this is true, sfx will be picked at random until all have been played. once this happens, the list is shuffled again, and it starts over
		[Tooltip("如果这为真，音效将被随机选取，直到所有音效都播放过为止。一旦出现这种情况，列表将再次被打乱顺序，然后重新开始。 ")]
		public bool RandomUnique = false;
		
		[MMFInspectorGroup("Scriptable Object", true, 14, true)]
		/// a scriptable object (created via the Create/MoreMountains/Audio/MMF_SoundData menu) to define settings that will override all other settings on this feedback
		[Tooltip("一个可编写脚本的对象（通过“Create/MoreMountains/Audio/MMF_SoundData”菜单创建），用于定义一些设置，这些设置将覆盖此反馈上的所有其他设置。 ")]
		public MMF_MMSoundManagerSoundData SoundDataSO;

		[MMFInspectorGroup("Sound Properties", true, 24)]
		[Header("Volume音量")]
		/// the minimum volume to play the sound at
		[Tooltip("播放声音时的最小音量")]
		[Range(0f,2f)]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("播放声音时的最大音量")]
		[Range(0f,2f)]
		public float MaxVolume = 1f;

		[Header("Pitch音调")]
		/// the minimum pitch to play the sound at
		[Tooltip("播放声音时的最小音调")]
		[Range(-3f,3f)]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("播放声音时的最大音调")]
		[Range(-3f,3f)]
		public float MaxPitch = 1f;
		
		[MMFInspectorGroup("SoundManager Options", true, 28)]
		/// the track on which to play the sound. Pick the one that matches the nature of your sound
		[Tooltip("播放声音的音轨。选择与你的声音特性相匹配的那一个音轨。 ")]
		public MMSoundManager.MMSoundManagerTracks MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
		/// the ID of the sound. This is useful if you plan on using sound control feedbacks on it afterwards. 
		[Tooltip("声音的标识符（ID）。如果你之后打算对该声音使用声音控制反馈，这个ID会很有用。 ")]
		public int ID = 0;
		/// the AudioGroup on which to play the sound. If you're already targeting a preset track, you can leave it blank, otherwise the group you specify here will override it.
		[Tooltip("用于播放声音的音频组。如果你已经指定了一个预设音轨，那么此处可以留空；否则，你在此处指定的音频组将覆盖预设的音轨设置。 ")]
		public AudioMixerGroup AudioGroup = null;
		/// if (for some reason) you've already got an audiosource and wouldn't like to use the built-in pool system, you can specify it here 
		[Tooltip("如果（由于某些原因）你已经有了一个音频源，并且不想使用内置的池系统，那么你可以在此处指定该音频源。 ")]
		public AudioSource RecycleAudioSource = null;
		/// whether or not this sound should loop
		[Tooltip("这个声音是否应该循环播放")]
		public bool Loop = false;
		/// whether or not this sound should continue playing when transitioning to another scene
		[Tooltip("当切换到另一个场景时，这个声音是否应该继续播放 ")]
		public bool Persistent = false;
		/// whether or not this sound should play if the same sound clip is already playing
		[Tooltip("如果相同的声音片段已经在播放，这个声音是否也应该播放 ")]
		public bool DoNotPlayIfClipAlreadyPlaying = false;
		/// if this is true, this sound will stop playing when stopping the feedback
		[Tooltip("如果这为真，那么在停止反馈时，这个声音将会停止播放。")]
		public bool StopSoundOnFeedbackStop = false;
        
		[MMFInspectorGroup("Fade", true, 30)]
		/// whether or not to fade this sound in when playing it
		[Tooltip("播放此声音时是否要让其淡入。 ")]
		public bool Fade = false;
		/// if fading, the volume at which to start the fade
		[Tooltip("如果要进行淡入处理，这是开始淡入时的音量。")]
		[MMCondition("Fade", true)]
		public float FadeInitialVolume = 0f;
		/// if fading, the duration of the fade, in seconds
		[Tooltip("如果要进行淡入处理，这是淡入的持续时间，单位为秒。 ")]
		[MMCondition("Fade", true)]
		public float FadeDuration = 1f;
		/// if fading, the tween over which to fade the sound 
		[Tooltip("如果进行淡入处理，这是用于使声音淡入的缓动效果。")]
		[MMCondition("Fade", true)]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		[MMFInspectorGroup("Solo", true, 32)]
		/// whether or not this sound should play in solo mode over its destination track. If yes, all other sounds on that track will be muted when this sound starts playing
		[Tooltip("该声音在其目标音轨上处于独奏模式时是否应该播放。如果是，那么当这个声音开始播放时，该音轨上的所有其他声音都将被静音。")]
		public bool SoloSingleTrack = false;
		/// whether or not this sound should play in solo mode over all other tracks. If yes, all other tracks will be muted when this sound starts playing
		[Tooltip("这个声音是否应该以独奏模式播放，且覆盖所有其他音轨。如果是，那么当这个声音开始播放时，所有其他音轨都将被静音。")]
		public bool SoloAllTracks = false;
		/// if in any of the above solo modes, AutoUnSoloOnEnd will unmute the track(s) automatically once that sound stops playing
		[Tooltip("如果处于上述任何一种独奏模式下，“自动结束时取消独奏（AutoUnSoloOnEnd）”功能会在该声音停止播放后，自动取消对相关音轨的静音设置。")]
		public bool AutoUnSoloOnEnd = false;

		[MMFInspectorGroup("Spatial Settings", true, 33)]
		/// Pans a playing sound in a stereo way (left or right). This only applies to sounds that are Mono or Stereo.
		[Tooltip("以立体声的方式（向左或向右）平移正在播放的声音。这仅适用于单声道或立体声的声音。 ")]
		[Range(-1f,1f)]
		public float PanStereo;
		/// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
		[Tooltip("设置此音频源受 3D 空间化计算（衰减、多普勒效应等）的影响程度。数值为 0.0 时，声音完全是 2D 效果，数值为 1.0 时，声音完全是 3D 效果。")]
		[Range(0f,1f)]
		public float SpatialBlend;
		/// a Transform this sound can 'attach' to and follow it along as it plays
		[Tooltip("一个可以让这个声音“附着”其上的变换（Transform）组件，并且在声音播放时声音会跟随其移动。")]
		public Transform AttachToTransform;
		
		[MMFInspectorGroup("Effects", true, 36)]
		/// Bypass effects (Applied from filter components or global listener filters).
		[Tooltip("绕过效果（由滤波器组件或全局监听滤波器应用的效果）。")]
		public bool BypassEffects = false;
		/// When set global effects on the AudioListener will not be applied to the audio signal generated by the AudioSource. Does not apply if the AudioSource is playing into a mixer group.
		[Tooltip("当进行设置后，应用于音频监听器（AudioListener）的全局效果将不会应用到由音频源（AudioSource）生成的音频信号上。但如果音频源正在向混音器组播放音频，则此设置不适用。")]
		public bool BypassListenerEffects = false;
		/// When set doesn't route the signal from an AudioSource into the global reverb associated with reverb zones.
		[Tooltip("当进行设置后，不会将来自音频源的信号路由到与混响区域相关联的全局混响中。")]
		public bool BypassReverbZones = false;
		/// Sets the priority of the AudioSource.
		[Tooltip("设置音频源的优先级。")]
		[Range(0, 256)]
		public int Priority = 128;
		/// The amount by which the signal from the AudioSource will be mixed into the global reverb associated with the Reverb Zones.
		[Tooltip("来自音频源的信号混入与混响区域相关联的全局混响中的混合量。")]
		[Range(0f,1.1f)]
		public float ReverbZoneMix = 1f;

		[MMFInspectorGroup("Time Options", true, 15)]
		/// a timestamp (in seconds, randomized between the defined min and max) at which the sound will start playing, equivalent to the Audiosource API's Time) 
		[Tooltip("一个时间戳（以秒为单位，在定义的最小值和最大值之间随机取值），声音将在该时间戳开始播放，相当于音频源 API 中的“时间（Time）”。")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackTime = new Vector2(0f, 0f);
		/// a duration (in seconds, randomized between the defined min and max) for which the sound will play before stopping. Ignored if min and max are zero.
		[Tooltip("声音在停止前播放的时长（以秒为单位，在定义的最小值和最大值之间随机取值）。如果最小值和最大值都为零，则此设置将被忽略。 ")]
		[MMVector("Min", "Max")]
		public Vector2 PlaybackDuration = new Vector2(0f, 0f);
        
		[MMFInspectorGroup("3D Sound Settings", true, 37)]
		/// Sets the Doppler scale for this AudioSource.
		[Tooltip("设置此音频源的多普勒缩放比例。")]
		[Range(0f,5f)]
		public float DopplerLevel = 1f;
		/// Sets the spread angle (in degrees) of a 3d stereo or multichannel sound in speaker space.
		[Tooltip("设置三维立体声或多声道声音在扬声器空间中的扩散角度（单位：度）。 ")]
		[Range(0,360)]
		public int Spread = 0;
		/// Sets/Gets how the AudioSource attenuates over distance.
		[Tooltip("设置/获取音频源如何随距离衰减。 ")]
		public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
		/// Within the Min distance the AudioSource will cease to grow louder in volume.
		[Tooltip("在最小距离范围内，音频源的音量将不再增大。 ")]
		public float MinDistance = 1f;
		/// (Logarithmic rolloff) MaxDistance is the distance a sound stops attenuating at.
		[Tooltip("（对数衰减模式）最大距离是指声音停止衰减时所处的距离。 ")]
		public float MaxDistance = 500f;
		/// whether or not to use a custom curve for custom volume rolloff
		[Tooltip("是否使用自定义曲线来实现自定义的音量衰减 ")]
		public bool UseCustomRolloffCurve = false;
		/// the curve to use for custom volume rolloff if UseCustomRolloffCurve is true
		[Tooltip("如果“使用自定义衰减曲线（UseCustomRolloffCurve）”为真时，用于自定义音量衰减的曲线。 ")]
		[MMFCondition("UseCustomRolloffCurve", true)]
		public AnimationCurve CustomRolloffCurve;
		/// whether or not to use a custom curve for spatial blend
		[Tooltip("是否针对空间混合使用自定义曲线 ")]
		public bool UseSpatialBlendCurve = false;
		/// the curve to use for custom spatial blend if UseSpatialBlendCurve is true
		[Tooltip("如果“使用空间混合曲线（UseSpatialBlendCurve）”为真时，用于自定义空间混合的曲线。 ")]
		[MMFCondition("UseSpatialBlendCurve", true)]
		public AnimationCurve SpatialBlendCurve;
		/// whether or not to use a custom curve for reverb zone mix
		[Tooltip("是否针对混响区域混合使用自定义曲线 ")]
		public bool UseReverbZoneMixCurve = false;
		/// the curve to use for custom reverb zone mix if UseReverbZoneMixCurve is true
		[Tooltip("	如果“使用混响区域混合曲线（UseReverbZoneMixCurve）”为真时，用于自定义混响区域混合的曲线。 ")]
		[MMFCondition("UseReverbZoneMixCurve", true)]
		public AnimationCurve ReverbZoneMixCurve;
		/// whether or not to use a custom curve for spread
		[Tooltip("是否针对扩散使用自定义曲线 ")]
		public bool UseSpreadCurve = false;
		/// the curve to use for custom spread if UseSpreadCurve is true
		[Tooltip("如果“使用扩散曲线（UseSpreadCurve）”为真时，用于自定义扩散的曲线。 ")]
		[MMFCondition("UseSpreadCurve", true)]
		public AnimationCurve SpreadCurve;
        
		[MMFInspectorGroup("Debug", true, 31)]
		/// whether or not to draw sound falloff gizmos when this MMF Player is selected
		[Tooltip("当选中此多媒体融合（MMF）播放器时，是否绘制声音衰减小工具 。")]
		public bool DrawGizmos = false;
		/// an object to use as the center of the gizmos. If left empty, this MMF Player's position will be used.
		[Tooltip("用作小工具中心的对象。如果留空，则将使用此多媒体融合（MMF）播放器的位置。 ")]
		[MMFCondition("DrawGizmos", true)]
		public Transform GizmosCenter;
		/// the color to use to draw the min distance sphere of the sound falloff gizmos
		[Tooltip("用于绘制声音衰减小工具的最小距离球体的颜色。 ")]
		[MMFCondition("DrawGizmos", true)]
		public Color MinDistanceColor = MMColors.CadetBlue;
		/// the color to use to draw the max distance sphere of the sound falloff gizmos
		[Tooltip("用于绘制声音衰减小工具的最大距离球体的颜色。 ")]
		[MMFCondition("DrawGizmos", true)]
		public Color MaxDistanceColor = MMColors.Orangered;
		/// 一个用于在检视面板中播放声音的测试按钮。 
		public MMF_Button TestPlayButton;
		/// 一个用于在检视面板中停止声音的测试按钮。 
		public MMF_Button TestStopButton;
		/// 一个用于在检视面板中重启声音的测试按钮。 
		public MMF_Button ResetSequentialIndexButton;
        
		protected AudioClip _randomClip;
		protected AudioSource _editorAudioSource;
		protected MMSoundManagerPlayOptions _options;
		protected AudioSource _playedAudioSource;
		protected float _randomPlaybackTime;
		protected float _randomPlaybackDuration;
		protected int _currentIndex = 0;
		protected Vector3 _gizmoCenter;
		protected MMShufflebag<int> _randomUniqueShuffleBag;
		protected AudioClip _lastPlayedClip;
		
		protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			HandleSO();

			_lastPlayedClip = null;
			
			if (RandomUnique)
			{
				_randomUniqueShuffleBag = new MMShufflebag<int>(RandomSfx.Length);
				for (int i = 0; i < RandomSfx.Length; i++)
				{
					_randomUniqueShuffleBag.Add(i,1);
				}
			}
		}

		/// <summary>
		/// 初始化调试按钮
		/// </summary>
		public override void InitializeCustomAttributes()
		{
			base.InitializeCustomAttributes();
			TestPlayButton = new MMF_Button("Debug Play Sound", TestPlaySound);
			TestStopButton = new MMF_Button("Debug Stop Sound", TestStopSound);
			ResetSequentialIndexButton = new MMF_Button("Reset Sequential Index", ResetSequentialIndex);
		}
        
		/// <summary>
		/// 播放随机音效或指定的音效（sfx，即sound effect的缩写）。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
            

			if (RandomSfx.Length > 0)
			{
				_randomClip = PickRandomClip();

				if (_randomClip != null)
				{
					PlaySound(_randomClip, position, intensityMultiplier);
					return;
				}
			}
			
			if (Sfx != null)
			{
				PlaySound(Sfx, position, intensityMultiplier);
				return;
			}
		}

		/// <summary>
		/// 在停止（操作）时，如果有需要，我们会停止播放声音。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (StopSoundOnFeedbackStop && (_playedAudioSource != null))
			{
				_playedAudioSource.Stop();
				MMSoundManager.Instance.FreeSound(_playedAudioSource);
			}
		}

		/// <summary>
		/// 如果指定了一个可编写脚本的对象，我们就获取它的值。 
		/// </summary>
		protected virtual void HandleSO()
		{
			if (SoundDataSO == null)
			{
				return;
			}
			
			Sfx = SoundDataSO.Sfx;
			RandomSfx = SoundDataSO.RandomSfx;
			SequentialOrder = SoundDataSO.SequentialOrder;
			SequentialOrderHoldLast = SoundDataSO.SequentialOrderHoldLast;
			SequentialOrderHoldCooldownDuration = SoundDataSO.SequentialOrderHoldCooldownDuration;
			RandomUnique = SoundDataSO.RandomUnique;
	        MinVolume = SoundDataSO.MinVolume;
			MaxVolume = SoundDataSO.MaxVolume;
			MinPitch = SoundDataSO.MinPitch;
			MaxPitch = SoundDataSO.MaxPitch;
			PlaybackTime = SoundDataSO.PlaybackTime;
			PlaybackDuration = SoundDataSO.PlaybackDuration;
			MmSoundManagerTrack = SoundDataSO.MmSoundManagerTrack;
			ID = SoundDataSO.ID;
			AudioGroup = SoundDataSO.AudioGroup;
			RecycleAudioSource = SoundDataSO.RecycleAudioSource;
			Loop = SoundDataSO.Loop;
			Persistent = SoundDataSO.Persistent;
			DoNotPlayIfClipAlreadyPlaying = SoundDataSO.DoNotPlayIfClipAlreadyPlaying;
			StopSoundOnFeedbackStop = SoundDataSO.StopSoundOnFeedbackStop;
			Fade = SoundDataSO.Fade;
			FadeInitialVolume = SoundDataSO.FadeInitialVolume;
			FadeDuration = SoundDataSO.FadeDuration;
			FadeTween = SoundDataSO.FadeTween;
			SoloSingleTrack = SoundDataSO.SoloSingleTrack;
			SoloAllTracks = SoundDataSO.SoloAllTracks;
			AutoUnSoloOnEnd = SoundDataSO.AutoUnSoloOnEnd;
			PanStereo = SoundDataSO.PanStereo;
			SpatialBlend = SoundDataSO.SpatialBlend;
			AttachToTransform = SoundDataSO.AttachToTransform;
			BypassEffects = SoundDataSO.BypassEffects;
			BypassListenerEffects = SoundDataSO.BypassListenerEffects;
			BypassReverbZones = SoundDataSO.BypassReverbZones;
			Priority = SoundDataSO.Priority;
			ReverbZoneMix = SoundDataSO.ReverbZoneMix;
			DopplerLevel = SoundDataSO.DopplerLevel;
			Spread = SoundDataSO.Spread;
			RolloffMode = SoundDataSO.RolloffMode;
			MinDistance = SoundDataSO.MinDistance;
			MaxDistance = SoundDataSO.MaxDistance;
			UseCustomRolloffCurve = SoundDataSO.UseCustomRolloffCurve;
			CustomRolloffCurve = SoundDataSO.CustomRolloffCurve;
			UseSpatialBlendCurve = SoundDataSO.UseSpatialBlendCurve;
			SpatialBlendCurve = SoundDataSO.SpatialBlendCurve;
			UseReverbZoneMixCurve = SoundDataSO.UseReverbZoneMixCurve;
			ReverbZoneMixCurve = SoundDataSO.ReverbZoneMixCurve;
			UseSpreadCurve = SoundDataSO.UseSpreadCurve;
			SpreadCurve = SoundDataSO.SpreadCurve;
		}

		/// <summary>
		/// 随机化播放时间和播放时长。
		/// </summary>
		public virtual void RandomizeTimes()
		{
			_randomPlaybackTime = Random.Range(PlaybackTime.x, PlaybackTime.y);
			_randomPlaybackDuration = Random.Range(PlaybackDuration.x, PlaybackDuration.y);
			Owner.ComputeCachedTotalDuration();
		}

		/// <summary>
		/// 触发一个播放声音的事件。
		/// </summary>
		/// <param name="sfx"></param>
		/// <param name="position"></param>
		/// <param name="intensity"></param>
		protected virtual void PlaySound(AudioClip sfx, Vector3 position, float intensity)
		{
			if (DoNotPlayIfClipAlreadyPlaying) 
			{
				if ((MMSoundManager.Instance.FindByClip(sfx) != null) && (MMSoundManager.Instance.FindByClip(sfx).isPlaying))
				{
					return;
				}
			}
            
			float volume = Random.Range(MinVolume, MaxVolume);
            
			if (!Timing.ConstantIntensity)
			{
				volume = volume * intensity;
			}
            
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();

			int timeSamples = NormalPlayDirection ? 0 : sfx.samples - 1;
            
			_options.MmSoundManagerTrack = MmSoundManagerTrack;
			_options.Location = position;
			_options.Loop = Loop;
			_options.Volume = volume;
			_options.ID = ID;
			_options.Fade = Fade;
			_options.FadeInitialVolume = FadeInitialVolume;
			_options.FadeDuration = FadeDuration;
			_options.FadeTween = FadeTween;
			_options.Persistent = Persistent;
			_options.RecycleAudioSource = RecycleAudioSource;
			_options.AudioGroup = AudioGroup;
			_options.Pitch = pitch;
			_options.PlaybackTime = _randomPlaybackTime;
			_options.PlaybackDuration = _randomPlaybackDuration;
			_options.PanStereo = PanStereo;
			_options.SpatialBlend = SpatialBlend;
			_options.SoloSingleTrack = SoloSingleTrack;
			_options.SoloAllTracks = SoloAllTracks;
			_options.AutoUnSoloOnEnd = AutoUnSoloOnEnd;
			_options.BypassEffects = BypassEffects;
			_options.BypassListenerEffects = BypassListenerEffects;
			_options.BypassReverbZones = BypassReverbZones;
			_options.Priority = Priority;
			_options.ReverbZoneMix = ReverbZoneMix;
			_options.DopplerLevel = DopplerLevel;
			_options.Spread = Spread;
			_options.RolloffMode = RolloffMode;
			_options.MinDistance = MinDistance;
			_options.MaxDistance = MaxDistance;
			_options.AttachToTransform = AttachToTransform;
			_options.UseSpreadCurve = UseSpreadCurve;
			_options.SpreadCurve = SpreadCurve;
			_options.UseCustomRolloffCurve = UseCustomRolloffCurve;
			_options.CustomRolloffCurve = CustomRolloffCurve;
			_options.UseSpatialBlendCurve = UseSpatialBlendCurve;
			_options.SpatialBlendCurve = SpatialBlendCurve;
			_options.UseReverbZoneMixCurve = UseReverbZoneMixCurve;
			_options.ReverbZoneMixCurve = ReverbZoneMixCurve;
			_options.DoNotAutoRecycleIfNotDonePlaying = true;

			_playedAudioSource = MMSoundManagerSoundPlayEvent.Trigger(sfx, _options);

			_lastPlayTimestamp = FeedbackTime;
			_lastPlayedClip = sfx;
		}

		/// <summary>
		/// 返回声音的时长，或者如果是随机声音，则返回其中最长声音的时长。 
		/// </summary>
		/// <returns></returns>
		protected virtual float GetDuration()
		{
			if (SoundDataSO != null)
			{
				return ComputeDuration(SoundDataSO.Sfx, SoundDataSO.RandomSfx);
			}
			else
			{
				return ComputeDuration(Sfx, RandomSfx);
			}
		}

		protected virtual float ComputeDuration(AudioClip sfx, AudioClip[] randomSfx)
		{
			if (sfx != null)
			{
				return (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : sfx.length - _randomPlaybackTime;
			}

			float longest = 0f;
			if ((randomSfx != null) && (randomSfx.Length > 0))
			{
				if (_lastPlayedClip != null)
				{
					return _lastPlayedClip.length;	
				}
				
				foreach (AudioClip clip in randomSfx)
				{
					if ((clip != null) && (clip.length > longest))
					{
						longest = clip.length;
					}
				}

				return (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : longest - _randomPlaybackTime;
			}

			return 0f;
		}

		public override void OnDrawGizmosSelectedHandler()
		{
			if (!DrawGizmos)
			{
				return;
			}

			_gizmoCenter = GizmosCenter != null ? GizmosCenter.position : Owner.transform.position;
			Gizmos.color = MinDistanceColor;
			Gizmos.DrawWireSphere(_gizmoCenter, MinDistance);
			Gizmos.color = MaxDistanceColor;
			Gizmos.DrawWireSphere(_gizmoCenter, MaxDistance);
		}
		
		/// <summary>
		/// 如果场景中不存在多媒体声音管理器（MMSoundManager），则自动尝试向场景中添加一个。 
		/// </summary>
		public override void AutomaticShakerSetup()
		{
			MMSoundManager soundManager = (MMSoundManager)Object.FindObjectOfType(typeof(MMSoundManager));
			if (soundManager == null)
			{
				GameObject soundManagerGo = new GameObject("MMSoundManager");
				soundManagerGo.AddComponent<MMSoundManager>();
				MMDebug.DebugLogInfo( "已向场景中添加了一个多媒体声音管理器（MMSoundManager）。一切已准备就绪。");
			}
		}

		#region TestMethods

		/// <summary>
		/// 一种测试方法，该方法会创建一个音频源，播放该音频源，并且在播放完成后自行销毁。 
		/// </summary>
		protected virtual async void TestPlaySound()
		{
			AudioClip tmpAudioClip = null;

			if (Sfx != null)
			{
				tmpAudioClip = Sfx;
			}

			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				tmpAudioClip = PickRandomClip();
			}

			if (tmpAudioClip == null)
			{
				Debug.LogError(Owner.gameObject.name + "的" + Label + " 无法在编辑器模式下播放，你尚未设置其音效（Sfx）。");
				return;
			}

			float volume = Random.Range(MinVolume, MaxVolume);
			float pitch = Random.Range(MinPitch, MaxPitch);
			RandomizeTimes();
			GameObject temporaryAudioHost = new GameObject("EditorTestAS_WillAutoDestroy");
			SceneManager.MoveGameObjectToScene(temporaryAudioHost.gameObject, Owner.gameObject.scene);
			temporaryAudioHost.transform.position = Owner.transform.position;
			_editorAudioSource = temporaryAudioHost.AddComponent<AudioSource>() as AudioSource;
			PlayAudioSource(_editorAudioSource, tmpAudioClip, volume, pitch, _randomPlaybackTime, _randomPlaybackDuration);
			_lastPlayTimestamp = FeedbackTime;
			_lastPlayedClip = tmpAudioClip;
			float length = (_randomPlaybackDuration > 0) ? _randomPlaybackDuration : tmpAudioClip.length;
			length *= 1000;
			length = length / Mathf.Abs(pitch);
			await Task.Delay((int)length);
			Object.DestroyImmediate(temporaryAudioHost);
		}

		/// <summary>
		/// 一种用于停止测试声音的测试方法。
		/// </summary>
		protected virtual void TestStopSound()
		{
			if (_editorAudioSource != null)
			{
				_editorAudioSource.Stop();
			}            
		}

		/// <summary>
		/// 以指定的音量和音调播放音频源。
		/// </summary>
		/// <param name="audioSource"></param>
		/// <param name="sfx"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		protected virtual void PlayAudioSource(AudioSource audioSource, AudioClip sfx, float volume, float pitch, float time, float playbackDuration)
		{
			// 我们将那个音频源剪辑设置为参数中指定的那个。 
			audioSource.clip = sfx;
			audioSource.time = time;
			// 我们将音频源的音量设置为参数中所指定的音量。 
			audioSource.volume = volume;
			audioSource.pitch = pitch;
			// 我们设置了循环选项。 
			audioSource.loop = false;
			// 我们开始播放声音。
			audioSource.Play(); 
		}
        
		/// <summary>
		/// 在处理随机剪辑时，确定要播放的下一个索引。 
		/// </summary>
		/// <returns></returns>
		protected virtual AudioClip PickRandomClip()
		{
			int newIndex = 0;
	        
			if (!SequentialOrder)
			{
				if (RandomUnique)
				{
					newIndex = _randomUniqueShuffleBag.Pick();
				}
				else
				{
					newIndex = Random.Range(0, RandomSfx.Length);	
				}
			}
			else
			{
				newIndex = _currentIndex;
		        
				if (newIndex >= RandomSfx.Length)
				{
					if (SequentialOrderHoldLast)
					{
						newIndex--;
						if ((SequentialOrderHoldCooldownDuration > 0)
						    && (FeedbackTime - _lastPlayTimestamp > SequentialOrderHoldCooldownDuration))
						{
							newIndex = 0;    
						}
					}
					else
					{
						newIndex = 0;
					}
				}
				_currentIndex = newIndex + 1;
			}
			return RandomSfx[newIndex];
		}

		/// <summary>
		/// 强制将顺序索引重置为0
		/// </summary>
		public virtual void ResetSequentialIndex()
		{
			_currentIndex = 0;
		}

		/// <summary>
		/// 强制将顺序索引重置为参数中指定的值。
		/// </summary>
		/// <param name="newIndex"></param>
		public virtual void SetSequentialIndex(int newIndex)
		{
			_currentIndex = newIndex;
		}
		
		/// <summary>
		/// 在进行验证时，我们将时间随机化。 
		/// </summary>
		public override void OnValidate()
		{
			base.OnValidate();
			RandomizeTimes();
			
			if ((RandomSfx != null) && (RandomSfx.Length > 0))
			{
				_randomUniqueShuffleBag = new MMShufflebag<int>(RandomSfx.Length);
				for (int i = 0; i < RandomSfx.Length; i++)
				{
					_randomUniqueShuffleBag.Add(i,1);
				}
			}
		}

		#endregion
	}
}