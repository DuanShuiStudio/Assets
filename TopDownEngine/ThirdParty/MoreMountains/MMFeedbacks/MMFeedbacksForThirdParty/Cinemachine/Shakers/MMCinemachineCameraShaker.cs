using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Feedbacks;

namespace MoreMountains.FeedbacksForThirdParty
{
    /// <summary>
    /// 将此组件添加到你的虚拟相机（Cinemachine Virtual Camera）中，以便在调用其“抖动相机（ShakeCamera）”方法时让相机产生抖动效果。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Cinemachine/MM Cinemachine Camera Shaker")]
	#if MM_CINEMACHINE
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	#elif MM_CINEMACHINE3
	[RequireComponent(typeof(CinemachineCamera))]
	#endif
	public class MMCinemachineCameraShaker : MonoBehaviour 
	{
		[Header("Settings设置")]
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是要监听由一个整数定义的通道，还是由一个MMChannel可编写脚本对象定义的通道。整数设置起来很简单，但可能会变得混乱，并且更难记住哪个整数对应着什么。  " +
                 "MMChannel可编写脚本对象要求你提前创建它们，但它们带有易读的名称，并且更具可扩展性。 ")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的那个通道相匹配。 ")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("要用于监听事件的 MMChannel 定义资源。以这个抖动器为目标的反馈必须引用相同的 MMChannel 定义才能接收事件。" +
                 "要创建一个 MMChannel，在项目中的任意位置（通常在一个数据文件夹中）右键单击，然后选择 “MoreMountains”>“MMChannel”，接着给它起一个唯一的名称。")]
		[MMFEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// The default amplitude that will be applied to your shakes if you don't specify one
		[Tooltip("如果你不指定振幅，将会应用于相机抖动的默认振幅。 ")]
		public float DefaultShakeAmplitude = .5f;
		/// The default frequency that will be applied to your shakes if you don't specify one
		[Tooltip("如果你不指定频率，将会应用于你的抖动效果的默认频率。 ")]
		public float DefaultShakeFrequency = 10f;
		/// the amplitude of the camera's noise when it's idle
		[Tooltip("相机处于闲置状态时其噪点的振幅 ")]
		[MMFReadOnly]
		public float IdleAmplitude;
		/// the frequency of the camera's noise when it's idle
		[Tooltip("相机处于闲置状态时其噪点的频率")]
		[MMFReadOnly]
		public float IdleFrequency = 1f;
		/// the speed at which to interpolate the shake
		[Tooltip("进行抖动插值时的速度")]
		public float LerpSpeed = 5f;

		[Header("Test测试")]
		/// a duration (in seconds) to apply when testing this shake via the TestShake button
		[Tooltip("通过“测试抖动（TestShake）”按钮测试此抖动效果时应用的时长（以秒为单位） ")]
		public float TestDuration = 0.3f;
		/// the amplitude to apply when testing this shake via the TestShake button
		[Tooltip("通过“测试抖动（TestShake）”按钮测试此抖动效果时应用的振幅 ")]
		public float TestAmplitude = 2f;
		/// the frequency to apply when testing this shake via the TestShake button
		[Tooltip("通过“测试抖动（TestShake）”按钮测试此抖动效果时应用的频率 ")]
		public float TestFrequency = 20f;

		[MMFInspectorButton("TestShake")]
		public bool TestShakeButton;

		public virtual float GetTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (_timescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected TimescaleModes _timescaleMode;
		protected Vector3 _initialPosition;
		protected Quaternion _initialRotation;
		#if MM_CINEMACHINE
		protected Cinemachine.CinemachineBasicMultiChannelPerlin _perlin;
		protected Cinemachine.CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineBasicMultiChannelPerlin _perlin;
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _targetAmplitude;
		protected float _targetFrequency;
		private Coroutine _shakeCoroutine;

        /// <summary>
        /// 在唤醒（Awake）时，我们获取我们的组件。 
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = this.gameObject.GetComponent<CinemachineVirtualCamera>();
			_perlin = _virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineBasicMultiChannelPerlin>();
			#elif MM_CINEMACHINE3
			_virtualCamera = this.gameObject.GetComponent<CinemachineCamera>();
			_perlin = _virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Noise) as CinemachineBasicMultiChannelPerlin;
			#endif
		}

        /// <summary>
        /// 在（脚本）开始（Start）时，我们重置相机，以便应用我们的基础振幅和频率。 
        /// </summary>
        protected virtual void Start()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (_perlin != null)
			{
				#if MM_CINEMACHINE
				IdleAmplitude = _perlin.m_AmplitudeGain;
				IdleFrequency = _perlin.m_FrequencyGain;
				#elif MM_CINEMACHINE3
				IdleAmplitude = _perlin.AmplitudeGain;
				IdleFrequency = _perlin.FrequencyGain;
				#endif
			}            
			#endif

			_targetAmplitude = IdleAmplitude;
			_targetFrequency = IdleFrequency;
		}

		protected virtual void Update()
		{
			#if MM_CINEMACHINE
			if (_perlin != null)
			{
				_perlin.m_AmplitudeGain = _targetAmplitude;
				_perlin.m_FrequencyGain = Mathf.Lerp(_perlin.m_FrequencyGain, _targetFrequency, GetDeltaTime() * LerpSpeed);
			}
			#elif MM_CINEMACHINE3
			if (_perlin != null)
			{
				_perlin.AmplitudeGain = _targetAmplitude;
				_perlin.FrequencyGain = Mathf.Lerp(_perlin.FrequencyGain, _targetFrequency, GetDeltaTime() * LerpSpeed);
			}
			#endif
		}

        /// <summary>
        /// 使用此方法，让相机以默认的振幅和频率抖动指定的时长（以秒为单位）。 
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void ShakeCamera(float duration, bool infinite, bool useUnscaledTime = false)
		{
			StartCoroutine(ShakeCameraCo(duration, DefaultShakeAmplitude, DefaultShakeFrequency, infinite, useUnscaledTime));
		}

        /// <summary>
        /// 使用此方法，让相机以指定的时长（以秒为单位）、振幅和频率进行抖动。 
        /// </summary>
        /// <param name="duration">Duration.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        public virtual void ShakeCamera(float duration, float amplitude, float frequency, bool infinite, bool useUnscaledTime = false)
		{
			if (_shakeCoroutine != null)
			{
				StopCoroutine(_shakeCoroutine);
			}
			_shakeCoroutine = StartCoroutine(ShakeCameraCo(duration, amplitude, frequency, infinite, useUnscaledTime));
		}

        /// <summary>
        /// 这个协程将会使…… 产生抖动。
        /// </summary>
        /// <returns>The camera co.</returns>
        /// <param name="duration">Duration.</param>
        /// <param name="amplitude">Amplitude.</param>
        /// <param name="frequency">Frequency.</param>
        protected virtual IEnumerator ShakeCameraCo(float duration, float amplitude, float frequency, bool infinite, bool useUnscaledTime)
		{
			_targetAmplitude  = amplitude;
			_targetFrequency = frequency;
			_timescaleMode = useUnscaledTime ? TimescaleModes.Unscaled : TimescaleModes.Scaled;
			if (!infinite)
			{
				yield return new WaitForSeconds(duration);
				CameraReset();
			}                        
		}

        /// <summary>
        /// 将相机的噪点值重置为其闲置状态下的值。 
        /// </summary>
        public virtual void CameraReset()
		{
			_targetAmplitude = IdleAmplitude;
			_targetFrequency = IdleFrequency;
		}

		public virtual void OnCameraShakeEvent(float duration, float amplitude, float frequency, float amplitudeX, float amplitudeY, float amplitudeZ, bool infinite, MMChannelData channelData, bool useUnscaledTime)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			this.ShakeCamera(duration, amplitude, frequency, infinite, useUnscaledTime);
		}

		public virtual void OnCameraShakeStopEvent(MMChannelData channelData)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}
			if (_shakeCoroutine != null)
			{
				StopCoroutine(_shakeCoroutine);
			}            
			CameraReset();
		}

		protected virtual void OnEnable()
		{
			MMCameraShakeEvent.Register(OnCameraShakeEvent);
			MMCameraShakeStopEvent.Register(OnCameraShakeStopEvent);
		}

		protected virtual void OnDisable()
		{
			MMCameraShakeEvent.Unregister(OnCameraShakeEvent);
			MMCameraShakeStopEvent.Unregister(OnCameraShakeStopEvent);
		}

		protected virtual void TestShake()
		{
			MMCameraShakeEvent.Trigger(TestDuration, TestAmplitude, TestFrequency, 0f, 0f, 0f, false, new MMChannelData(ChannelMode, Channel, MMChannelDefinition));
		}
	}
}