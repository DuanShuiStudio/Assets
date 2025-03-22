using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	#region Events
	/// <summary>
	/// An event used (usually by feedbacks) to trigger the spawn of a new floating text
	/// </summary>
	public struct MMFloatingTextSpawnEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false);
		static public void Trigger(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
		{
			OnEvent?.Invoke(channelData, spawnPosition, value, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient, useUnscaledTime);
		} 
	}
    #endregion

    /// <summary>
    /// 这个类能让你对浮动文本进行对象池管理、回收以及生成操作，通常用于显示伤害信息。 
    /// 它需要一个 `MMFloatingText` 对象作为输入。 
    /// </summary>
    public class MMFloatingTextSpawner : MMMonoBehaviour
	{
        /// 是生成单个预设体还是随机生成一个预设体。 
        public enum PoolerModes { Simple, Multiple }
        /// 所生成的文本是应该具有固定的对齐方式，还是使其方向与初始生成方向相匹配，亦或是与它的移动曲线相适配。  
        public enum AlignmentModes { Fixed, MatchInitialDirection, MatchMovementDirection }

		[MMInspectorGroup("General Settings", true, 10)]
        
		/// whether to listen on a channel defined by an int or by a MMChannel scriptable object. Ints are simple to setup but can get messy and make it harder to remember what int corresponds to what.
		/// MMChannel scriptable objects require you to create them in advance, but come with a readable name and are more scalable
		[Tooltip("是监听由一个整数定义的通道，还是监听由一个 `MMChannel` 可编写脚本对象定义的通道。整数设置起来很简单，但可能会变得杂乱无章，并且更难记住哪个整数对应着什么内容。 " +
                 "`MMChannel` 可编写脚本对象要求你预先创建它们，但它们带有易读的名称，并且更具可扩展性。 ")]
		public MMChannelModes ChannelMode = MMChannelModes.Int;
		/// the channel to listen to - has to match the one on the feedback
		[Tooltip("要监听的通道——必须与反馈中的那个通道相匹配。 ")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.Int)]
		public int Channel = 0;
		/// the MMChannel definition asset to use to listen for events. The feedbacks targeting this shaker will have to reference that same MMChannel definition to receive events - to create a MMChannel,
		/// right click anywhere in your project (usually in a Data folder) and go MoreMountains > MMChannel, then name it with some unique name
		[Tooltip("用于监听事件的 MMChannel 定义资产。针对此震动器（shaker）的反馈必须引用相同的 MMChannel 定义才能接收事件。若要创建一个 MMChannel，" +
                 "在你的项目中的任意位置（通常是在一个名为 “Data” 的文件夹里）右键点击，然后选择 “MoreMountains > MMChannel”，接着给它取一个独特的名称")]
		[MMEnumCondition("ChannelMode", (int)MMChannelModes.MMChannel)]
		public MMChannel MMChannelDefinition = null;
		/// whether or not this spawner can spawn at this time
		[Tooltip("这个生成器当前是否能够进行生成操作。 ")]
		public bool CanSpawn = true;
		/// whether or not this spawner should spawn objects on unscaled time
		[Tooltip("这个生成器是否应该在不受时间缩放影响的时间（即不受游戏中时间快慢调整影响的正常时间）下生成对象。  ")]
		public bool UseUnscaledTime = false;
        
		[MMInspectorGroup("Pooler", true, 24)]

		/// the selected pooler mode (single prefab or multiple ones)
		[Tooltip("所选的对象池管理模式（单个预设体模式或多个预设体模式）")]
		public PoolerModes PoolerMode = PoolerModes.Simple;
		/// the prefab to spawn (ignored if in multiple mode)
		[Tooltip("要生成的预设体（如果处于多预设体模式，则此设置会被忽略）")]
		public MMFloatingText PooledSimpleMMFloatingText;
		/// the prefabs to spawn (ignored if in simple mode)
		[Tooltip("要生成的多个预设体（如果处于单预设体模式，则此设置会被忽略）")]
		public List<MMFloatingText> PooledMultipleMMFloatingText;
		/// the amount of objects to pool to avoid having to instantiate them at runtime. Should be bigger than the max amount of texts you plan on having on screen at any given moment
		[Tooltip("要进行对象池化的对象数量，目的是避免在运行时实例化这些对象。该数量应该大于你计划在任何时刻屏幕上显示的文本的最大数量。")]
		public int PoolSize = 20;
		/// whether or not to nest the waiting pools
		[Tooltip("是否嵌套等待对象池。")]
		public bool NestWaitingPool = true;
		/// whether or not to mutualize the waiting pools
		[Tooltip("是否共享等待对象池。\r\n")]
		public bool MutualizeWaitingPools = true;
		/// whether or not the text pool can expand if the pool is empty
		[Tooltip("如果文本对象池为空，该文本对象池是否能够进行扩容。 ")]
		public bool PoolCanExpand = true;

		[MMInspectorGroup("Spawn Settings", true, 14)]

		/// the random min and max lifetime duration for the spawned texts (in seconds)
		[Tooltip("生成的文本的随机最小和最大存在时间（以秒为单位）。")]
		[MMVector("Min", "Max")] 
		public Vector2 Lifetime = Vector2.one;
        
		[Header("Spawn Position Offset生成位置偏移量")]
		/// the random min position at which to spawn the text, relative to its intended spawn position
		[Tooltip("用于生成文本的随机最小位置，该位置相对于其预期的生成位置而言。 ")]
		public Vector3 SpawnOffsetMin = Vector3.zero;
		/// the random max position at which to spawn the text, relative to its intended spawn position
		[Tooltip("文本生成的随机最大位置，相对于其预期生成位置而言。")]
		public Vector3 SpawnOffsetMax = Vector3.zero;

		[MMInspectorGroup("Animate Position", true, 15)] 
        
		[Header("Movement位移")]

		/// whether or not to animate the movement of spawned texts
		[Tooltip("是否对生成的文本的移动进行动画处理")]
		public bool AnimateMovement = true;
		/// whether or not to animate the X movement of spawned texts
		[Tooltip("是否对生成文本在 X 轴方向上的移动进行动画处理")]
		public bool AnimateX = false;
		/// the value to which the x movement curve's zero should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 X 轴移动曲线的“0”值重新映射到的值，该值在其最小值和最大值之间随机化 - 如果你不希望有任何随机性，则在最小值和最大值中输入相同的值。")]
		[MMCondition("AnimateX", true)] 
		[MMVector("Min", "Max")]
		public Vector2 RemapXZero = Vector2.zero;
		/// the value to which the x movement curve's one should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 X 轴移动曲线的“1”值重新映射到的值，该值在其最小值和最大值之间随机确定。如果你不希望有任何随机性，就在最小值和最大值中都填入相同的值。  ")]
		[MMCondition("AnimateX", true)] 
		[MMVector("Min", "Max")]
		public Vector2 RemapXOne = Vector2.one;
		/// the curve on which to animate the x movement
		[Tooltip("用于对 X 轴方向移动进行动画处理的曲线。")]
		[MMCondition("AnimateX", true)]
		public AnimationCurve AnimateXCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		/// whether or not to animate the Y movement of spawned texts
		[Tooltip("是否对生成的文本在 Y 轴方向上的移动进行动画处理。")]
		public bool AnimateY = true;
		/// the value to which the y movement curve's zero should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 Y 轴移动曲线的“0”值重新映射到的值，该值在其最小值和最大值之间随机取值。如果你不希望有任何随机性，则将最小值和最大值都设置为相同的值。 ")]
		[MMCondition("AnimateY", true)] 
		[MMVector("Min", "Max")]
		public Vector2 RemapYZero = Vector2.zero;
		/// the value to which the y movement curve's one should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 Y 轴移动曲线的“1”值重新映射到的值，该值在其最小值和最大值之间随机取值。如果你不希望有任何随机性，则将最小值和最大值都设置为相同的值。")]
		[MMCondition("AnimateY", true)]
		[MMVector("Min", "Max")]
		public Vector2 RemapYOne = new Vector2(5f, 5f);
		/// the curve on which to animate the y movement
		[Tooltip("用于对生成文本在 Y 轴方向上的移动进行动画处理的曲线。")]
		[MMCondition("AnimateY", true)]
		public AnimationCurve AnimateYCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		/// whether or not to animate the Z movement of spawned texts
		[Tooltip("是否对生成的文本在 Z 轴方向上的移动进行动画处理。")]
		public bool AnimateZ = false;
		/// the value to which the z movement curve's zero should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 Z 轴移动曲线的“0”值重新映射到的值，该值在其最小值和最大值之间随机取值。如果你不希望有任何随机性，则将最小值和最大值都设置为相同的值。")]
		[MMCondition("AnimateZ", true)] 
		[MMVector("Min", "Max")]
		public Vector2 RemapZZero = Vector2.zero;
		/// the value to which the z movement curve's one should be remapped to, randomized between its min and max - put the same value in both min and max if you don't want any randomness
		[Tooltip("要将 Z 轴移动曲线的“1”值重新映射到的值，该值在其最小值和最大值之间随机取值。如果你不希望有任何随机性，则将最小值和最大值都设置为相同的值。")]
		[MMCondition("AnimateZ", true)] 
		[MMVector("Min", "Max")]
		public Vector2 RemapZOne = Vector2.one;
		/// the curve on which to animate the z movement
		[Tooltip("用于对文本在 Z 轴方向移动进行动画处理的曲线")]
		[MMCondition("AnimateZ", true)]
		public AnimationCurve AnimateZCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
		[MMInspectorGroup("Facing Directions", true, 16)]
        
		[Header("Alignment对齐")]

		/// the selected alignment mode (whether the spawned text should have a fixed alignment, orient to match the initial spawn direction, or its movement curve)
		[Tooltip("所选的对齐模式（即生成的文本是应采用固定对齐方式，还是应根据初始生成方向进行定向，亦或是依据其移动曲线进行对齐）。")]
		public AlignmentModes AlignmentMode = AlignmentModes.Fixed;
		/// when in fixed mode, the direction in which to keep the spawned texts
		[Tooltip("在固定模式下，用于保持所生成文本的方向。 ")]
		[MMEnumCondition("AlignmentMode", (int)AlignmentModes.Fixed)]
		public Vector3 FixedAlignment = Vector3.up;

		[Header("Billboard展示")]

		/// whether or not spawned texts should always face the camera
		[Tooltip("生成的文本是否应始终面向相机")]
		public bool AlwaysFaceCamera;
		/// whether or not this spawner should automatically grab the main camera on start
		[Tooltip("这个生成器在启动时是否应该自动获取主摄像机。 ")]
		[MMCondition("AlwaysFaceCamera", true)]
		public bool AutoGrabMainCameraOnStart = true;
		/// if not in auto grab mode, the camera to use for billboards
		[Tooltip("如果不处于自动抓取模式，那么用于实现广告牌效果（使对象始终面向相机）的摄像机 。 ")]
		[MMCondition("AlwaysFaceCamera", true)]
		public Camera TargetCamera;
                
		[MMInspectorGroup("Animate Scale", true, 46)]

		/// whether or not to animate the scale of spawned texts
		[Tooltip("是否对生成文本的大小进行动画处理")]
		public bool AnimateScale = true;
		/// the value to which the scale curve's zero should be remapped to
		[Tooltip("缩放曲线的0值应重新映射到的值")]
		[MMCondition("AnimateScale", true)]
		public Vector2 RemapScaleZero = Vector2.zero;
		/// the value to which the scale curve's one should be remapped to
		[Tooltip("缩放曲线的1值应重新映射到的值")]
		[MMCondition("AnimateScale", true)]
		public Vector2 RemapScaleOne = Vector2.one;
		/// the curve on which to animate the scale
		[Tooltip("用于对缩放进行动画处理的曲线")]
		[MMCondition("AnimateScale", true)]
		public AnimationCurve AnimateScaleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, 1f), new Keyframe(0.85f, 1f), new Keyframe(1f, 0f));
        
		[MMInspectorGroup("Animate Color", true, 55)]

		/// whether or not to animate the spawned text's color over time
		[Tooltip("是否随着时间的推移对生成的文本的颜色进行动画处理。")]
		public bool AnimateColor = false;
		/// the gradient over which to animate the spawned text's color over time
		[Tooltip("用于随着时间推移对生成文本的颜色进行动画处理的渐变效果。 ")]
		[GradientUsage(true)]
		public Gradient AnimateColorGradient = new Gradient();

		[MMInspectorGroup("Animate Opacity", true, 45)]

		/// whether or not to animate the opacity of the spawned texts
		[Tooltip("是否对生成的文本的不透明度进行动画处理。 ")]
		public bool AnimateOpacity = true;
		/// the value to which the opacity curve's zero should be remapped to
		[Tooltip("不透明度曲线的0值应重新映射到的值。 ")]
		[MMCondition("AnimateOpacity", true)]
		public Vector2 RemapOpacityZero = Vector2.zero;
		/// the value to which the opacity curve's one should be remapped to
		[Tooltip("不透明度曲线的1值应重新映射到的值。")]
		[MMCondition("AnimateOpacity", true)]
		public Vector2 RemapOpacityOne = Vector2.one;
		/// the curve on which to animate the opacity
		[Tooltip("用于对不透明度进行动画处理的曲线。 ")]
		[MMCondition("AnimateOpacity", true)]
		public AnimationCurve AnimateOpacityCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 1f), new Keyframe(0.8f, 1f), new Keyframe(1f, 0f));

		[MMInspectorGroup("Intensity Multipliers", true, 45)]

		/// whether or not the intensity multiplier should impact lifetime
		[Tooltip("强度乘数是否应该对持续时间产生影响。 ")]
		public bool IntensityImpactsLifetime = false;
		/// when getting an intensity multiplier, the value by which to multiply the lifetime
		[Tooltip("在获取强度乘数时，用于与持续时间相乘的数值。 ")]
		[MMCondition("IntensityImpactsLifetime", true)]
		public float IntensityLifetimeMultiplier = 1f;
		/// whether or not the intensity multiplier should impact movement
		[Tooltip("强度乘数是否应该影响运动。")]
		public bool IntensityImpactsMovement = false;
		/// when getting an intensity multiplier, the value by which to multiply the movement values
		[Tooltip("在获取强度乘数时，用于与运动数值相乘的那个数值。 ")]
		[MMCondition("IntensityImpactsMovement", true)]
		public float IntensityMovementMultiplier = 1f;
		/// whether or not the intensity multiplier should impact scale
		[Tooltip("强度乘数是否应该对缩放比例产生影响。 ")]
		public bool IntensityImpactsScale = false;
		/// when getting an intensity multiplier, the value by which to multiply the scale values
		[Tooltip("在获取强度乘数时，用于与缩放数值相乘的那个值。 ")]
		[MMCondition("IntensityImpactsScale", true)]
		public float IntensityScaleMultiplier = 1f;

		[MMInspectorGroup("Debug", true, 12)]

		/// a random value to display when pressing the TestSpawnOne button
		[Tooltip("按下“TestSpawnOne（测试生成一个）”按钮时显示的一个随机值。 ")]
		public Vector2Int DebugRandomValue = new Vector2Int(100, 500);
		/// the min and max bounds within which to pick a value to output when pressing the TestSpawnMany button
		[Tooltip("按下“TestSpawnMany（测试生成多个）”按钮时，用于选取要输出的值的最小边界值和最大边界值。 ")]
		[MMVector("Min", "Max")] 
		public Vector2 DebugInterval = new Vector2(0.3f, 0.5f);
		/// a button used to test the spawn of one text
		[Tooltip("一个用于测试生成一个文本的按钮。")]
		[MMInspectorButton("TestSpawnOne")]
		public bool TestSpawnOneBtn;
		/// a button used to start/stop the spawn of texts at regular intervals
		[Tooltip("一个用于启动或停止以固定时间间隔生成文本的按钮。 ")]
		[MMInspectorButton("TestSpawnMany")]
		public bool TestSpawnManyBtn;
        
		protected MMObjectPooler _pooler;
		protected MMFloatingText _floatingText;
		protected Coroutine _testSpawnCoroutine;
        
		protected float _lifetime;
		protected float _speed;
		protected Vector3 _spawnOffset;
		protected Vector3 _direction;
		protected Gradient _colorGradient;
		protected bool _animateColor;

		#region Initialization

		/// <summary>
		/// On awake we initialize our spawner
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// On Start we grab our main camera if needed
		/// </summary>
		protected virtual void Start()
		{
			GrabMainCamera();
		}

		/// <summary>
		/// On init, we instantiate our object pool and grab the main camera
		/// </summary>
		protected virtual void Initialization()
		{
			InstantiateObjectPool();
		}

		/// <summary>
		/// Instantiates the specified type of object pool
		/// </summary>
		protected virtual void InstantiateObjectPool()
		{
			if (_pooler == null)
			{
				if (PoolerMode == PoolerModes.Simple)
				{
					InstantiateSimplePool();
				}
				else
				{
					InstantiateMultiplePool();
				}
			}
		}

		/// <summary>
		/// Instantiates a simple object pooler and sets it up
		/// </summary>
		protected virtual void InstantiateSimplePool()
		{
			if (PooledSimpleMMFloatingText == null)
			{
				Debug.LogError(this.name + " : no PooledSimpleMMFloatingText prefab has been set.");
				return;
			}
			GameObject newPooler = new GameObject();
			SceneManager.MoveGameObjectToScene(newPooler, this.gameObject.scene);
			newPooler.name = PooledSimpleMMFloatingText.name + "_Pooler";
			newPooler.transform.SetParent(this.transform);
			MMSimpleObjectPooler simplePooler = newPooler.AddComponent<MMSimpleObjectPooler>();
			simplePooler.PoolSize = PoolSize;
			simplePooler.GameObjectToPool = PooledSimpleMMFloatingText.gameObject;
			simplePooler.NestWaitingPool = NestWaitingPool;
			simplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
			simplePooler.PoolCanExpand = PoolCanExpand;
			simplePooler.FillObjectPool();
			_pooler = simplePooler;
		}

		/// <summary>
		/// Instantiates a multiple object pooler and sets it up
		/// </summary>
		protected virtual void InstantiateMultiplePool()
		{
			GameObject newPooler = new GameObject();
			SceneManager.MoveGameObjectToScene(newPooler, this.gameObject.scene);
			newPooler.name = this.name + "_Pooler";
			newPooler.transform.SetParent(this.transform);
			MMMultipleObjectPooler multiplePooler = newPooler.AddComponent<MMMultipleObjectPooler>();
			multiplePooler.Pool = new List<MMMultipleObjectPoolerObject>();
			foreach (MMFloatingText obj in PooledMultipleMMFloatingText)
			{
				MMMultipleObjectPoolerObject item = new MMMultipleObjectPoolerObject();
				item.GameObjectToPool = obj.gameObject;
				item.PoolCanExpand = PoolCanExpand;
				item.PoolSize = PoolSize;
				item.Enabled = true;
				multiplePooler.Pool.Add(item);
			}
			multiplePooler.NestWaitingPool = NestWaitingPool;
			multiplePooler.MutualizeWaitingPools = MutualizeWaitingPools;
			multiplePooler.FillObjectPool();
			_pooler = multiplePooler;
		}

		/// <summary>
		/// Grabs the main camera if needed
		/// </summary>
		protected virtual void GrabMainCamera()
		{
			if (AutoGrabMainCameraOnStart)
			{
				TargetCamera = Camera.main;
			}
		}

        #endregion

        /// <summary>
        /// 生成一条新的悬浮文本。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <param name="intensity"></param>
        /// <param name="forceLifetime"></param>
        /// <param name="lifetime"></param>
        /// <param name="forceColor"></param>
        /// <param name="animateColorGradient"></param>
        protected virtual void Spawn(string value, Vector3 position, Vector3 direction, float intensity = 1f,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null)
		{
			if (!CanSpawn)
			{
				return;
			}

			_direction = (direction != Vector3.zero) ? direction + this.transform.up : this.transform.up;

			this.transform.position = position;

			GameObject nextGameObject = _pooler.GetPooledGameObject();

			float lifetimeMultiplier = IntensityImpactsLifetime ? intensity * IntensityLifetimeMultiplier : 1f;
			float movementMultiplier = IntensityImpactsMovement ? intensity * IntensityMovementMultiplier : 1f;
			float scaleMultiplier = IntensityImpactsScale ? intensity * IntensityScaleMultiplier : 1f;

			_lifetime = UnityEngine.Random.Range(Lifetime.x, Lifetime.y) * lifetimeMultiplier;
			_spawnOffset = MMMaths.RandomVector3(SpawnOffsetMin, SpawnOffsetMax);
			_animateColor = AnimateColor;
			_colorGradient = AnimateColorGradient;

			float remapXZero = UnityEngine.Random.Range(RemapXZero.x, RemapXZero.y);
			float remapXOne = UnityEngine.Random.Range(RemapXOne.x, RemapXOne.y) * movementMultiplier;
			float remapYZero = UnityEngine.Random.Range(RemapYZero.x, RemapYZero.y);
			float remapYOne = UnityEngine.Random.Range(RemapYOne.x, RemapYOne.y) * movementMultiplier;
			float remapZZero = UnityEngine.Random.Range(RemapZZero.x, RemapZZero.y);
			float remapZOne = UnityEngine.Random.Range(RemapZOne.x, RemapZOne.y) * movementMultiplier;
			float remapOpacityZero = UnityEngine.Random.Range(RemapOpacityZero.x, RemapOpacityZero.y);
			float remapOpacityOne = UnityEngine.Random.Range(RemapOpacityOne.x, RemapOpacityOne.y);
			float remapScaleZero = UnityEngine.Random.Range(RemapScaleZero.x, RemapOpacityZero.y);
			float remapScaleOne = UnityEngine.Random.Range(RemapScaleOne.x, RemapScaleOne.y) * scaleMultiplier;

			if (forceLifetime)
			{
				_lifetime = lifetime;
			}

			if (forceColor)
			{
				_animateColor = true;
				_colorGradient = animateColorGradient;
			}

            // 强制性检查
            if (nextGameObject==null) { return; }

            // 我们激活这个对象。
            nextGameObject.gameObject.SetActive(true);
			nextGameObject.gameObject.MMGetComponentNoAlloc<MMPoolableObject>().TriggerOnSpawnComplete();

            // 我们确定这个对象的位置。 
            nextGameObject.transform.position = this.transform.position + _spawnOffset;

			_floatingText = nextGameObject.MMGetComponentNoAlloc<MMFloatingText>();
			_floatingText.SetUseUnscaledTime(UseUnscaledTime, true);
			_floatingText.ResetPosition();
			_floatingText.SetProperties(value, _lifetime, _direction, AnimateMovement, 
				AlignmentMode, FixedAlignment, AlwaysFaceCamera, TargetCamera,
				AnimateX, AnimateXCurve, remapXZero, remapXOne,
				AnimateY, AnimateYCurve, remapYZero, remapYOne,
				AnimateZ, AnimateZCurve, remapZZero, remapZOne,
				AnimateOpacity, AnimateOpacityCurve, remapOpacityZero, remapOpacityOne,
				AnimateScale, AnimateScaleCurve, remapScaleZero, remapScaleOne,
				_animateColor, _colorGradient);            
		}

        /// <summary>
        /// 当在这个生成器的通道上接收到一个悬浮文本事件时，我们就会生成一条新的悬浮文本。 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="spawnPosition"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <param name="intensity"></param>
        /// <param name="forceLifetime"></param>
        /// <param name="lifetime"></param>
        /// <param name="forceColor"></param>
        /// <param name="animateColorGradient"></param>
        public virtual void OnMMFloatingTextSpawnEvent(MMChannelData channelData, Vector3 spawnPosition, string value, Vector3 direction, float intensity,
			bool forceLifetime = false, float lifetime = 1f, bool forceColor = false, Gradient animateColorGradient = null, bool useUnscaledTime = false)
		{
			if (!MMChannel.Match(channelData, ChannelMode, Channel, MMChannelDefinition))
			{
				return;
			}

			UseUnscaledTime = useUnscaledTime;
			Spawn(value, spawnPosition, direction, intensity, forceLifetime, lifetime, forceColor, animateColorGradient);
		}

        /// <summary>
        /// 在启用时，我们开始监听悬浮文本事件。 
        /// </summary>
        protected virtual void OnEnable()
		{
			MMFloatingTextSpawnEvent.Register(OnMMFloatingTextSpawnEvent);
		}

        /// <summary>
        /// 在禁用时，我们停止监听悬浮文本事件。 
        /// </summary>
        protected virtual void OnDisable()
		{
			MMFloatingTextSpawnEvent.Unregister(OnMMFloatingTextSpawnEvent);
		}

		// Test methods ----------------------------------------------------------------------------------------

		#region TestMethods

		/// <summary>
		/// A test method that spawns one floating text
		/// </summary>
		protected virtual void TestSpawnOne()
		{
			string test = UnityEngine.Random.Range(DebugRandomValue.x, DebugRandomValue.y).ToString();
			Spawn(test, this.transform.position, Vector3.zero);
		}

		/// <summary>
		/// A method used to start/stop the regular spawning of debug floating texts 
		/// </summary>
		protected virtual void TestSpawnMany()
		{
			if (_testSpawnCoroutine == null)
			{
				_testSpawnCoroutine = StartCoroutine(TestSpawnManyCo());    
			}
			else
			{
				StopCoroutine(_testSpawnCoroutine);
				_testSpawnCoroutine = null;
			}
		}

		/// <summary>
		/// A coroutine used to spawn debug floating texts until stopped 
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerator TestSpawnManyCo()
		{
			float lastSpawnAt = Time.time;
			float interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
			while (true)
			{
				if (Time.time - lastSpawnAt > interval)
				{
					TestSpawnOne();
					lastSpawnAt = Time.time;
					interval = UnityEngine.Random.Range(DebugInterval.x, DebugInterval.y);
				}
				yield return null;
			}
		}
        
		#endregion
	}
}