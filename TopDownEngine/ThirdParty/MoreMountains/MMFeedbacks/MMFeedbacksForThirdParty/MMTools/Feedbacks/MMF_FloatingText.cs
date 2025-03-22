using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will request the spawn of a floating text, usually to signify damage, but not necessarily
	/// This requires that a MMFloatingTextSpawner be correctly setup in the scene, otherwise nothing will happen.
	/// To do so, create a new empty object, add a MMFloatingTextSpawner to it. Drag (at least) one MMFloatingText prefab into its PooledSimpleMMFloatingText slot.
	/// You'll find such prefabs already made in the MMTools/Tools/MMFloatingText/Prefabs folder, but feel free to create your own.
	/// Using that feedback will always spawn the same text. While this may be what you want, if you're using the Corgi Engine or TopDown Engine, you'll find dedicated versions
	/// directly hooked to the Health component, letting you display damage taken.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将请求生成一条浮动文本，通常用于表示造成了伤害，但也不一定总是如此。  " +
	              "这要求在场景中正确设置一个MMFloatingTextSpawner（浮动文本生成器），否则将不会有任何效果。  " +
	              "要做到这一点，创建一个新的空对象，为其添加一个MMFloatingTextSpawner（浮动文本生成器）。将（至少）一个MMFloatingText预制体拖到它的“PooledSimpleMMFloatingText（池化的简单MM浮动文本）”插槽中。  " +
	              "你会在“MMTools/Tools/MMFloatingText/Prefabs”文件夹中找到已经制作好的此类预制体，但你也可以随意创建自己的预制体。  " +
	              "使用该反馈将始终生成相同的文本。虽然这可能正是你想要的效果，但如果你正在使用柯基引擎（Corgi Engine）或俯视视角引擎（TopDown Engine），你会找到专门的版本。   " +
	              "直接连接到“生命值（Health）”组件上，使你能够显示所受到的伤害。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("UI/Floating Text")]
	public class MMF_FloatingText : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		#endif

		/// 此反馈的持续时间是一个固定值，或者是其存在的时长。 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Lifetime); } set { Lifetime = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		/// 浮动文本可能生成的位置 
		public enum PositionModes { TargetTransform, FeedbackPosition, PlayPosition }

		[MMFInspectorGroup("Floating Text", true, 64)]
		/// the Intensity to spawn this text with, will act as a lifetime/movement/scale multiplier based on the spawner's settings
		[Tooltip("生成此文本时所使用的强度，将根据生成器的设置，充当持续时间、移动距离或缩放比例的乘数。 ")]
		public float Intensity = 1f;
		/// the value to display when spawning this text
		[Tooltip("生成此文本时要显示的值")]
		public string Value = "100";
		/// if this is true, the intensity passed to this feedback will be the value displayed
		[Tooltip("如果这为真，传递给此反馈的强度将是所显示的值。 ")]
		public bool UseIntensityAsValue = false;
		
		/// 可能应用于输出值的方法（当使用强度作为输出值时，字符串值将不会被取整） 
		public enum RoundingMethods { NoRounding, Round, Ceil, Floor }
		
		/// the rounding methods to apply to the output value (when using intensity as the output value, string values won't get rounded)
		[Tooltip("要应用于输出值的取整方法（当使用强度作为输出值时，字符串值将不会被取整） ")]
		[MMFInspectorGroup("Rounding", true, 68)]
		public RoundingMethods RoundingMethod = RoundingMethods.NoRounding;

		[MMFInspectorGroup("Color", true, 65)]
		/// whether or not to force a color on the new text, if not, the default colors of the spawner will be used
		[Tooltip("是否要为新文本强制设置一种颜色，如果不强制设置，将使用生成器的默认颜色。 ")]
		public bool ForceColor = false;
		/// the gradient to apply over the lifetime of the text
		[Tooltip("在文本显示的整个持续时间内要应用的渐变效果 ")]
		[GradientUsage(true)]
		public Gradient AnimateColorGradient = new Gradient();

		[MMFInspectorGroup("Lifetime", true, 66)]
		/// whether or not to force a lifetime on the new text, if not, the default colors of the spawner will be used
		[Tooltip("是否要为新文本强制设定一个显示时长，如果不强制设定，将使用生成器的默认显示时长设置")]
		public bool ForceLifetime = false;
		/// the forced lifetime for the spawned text
		[Tooltip("所生成文本的强制显示时长 ")]
		[MMFCondition("ForceLifetime", true)]
		public float Lifetime = 0.5f;

		[MMFInspectorGroup("Position", true, 67)]
		/// where to spawn the new text (at the position of the feedback, or on a specified Transform)
		[Tooltip("新文本的生成位置（在反馈的位置，还是在指定的变换组件所在位置） ")]
		public PositionModes PositionMode = PositionModes.FeedbackPosition;
		/// in transform mode, the Transform on which to spawn the new floating text
		[Tooltip("在变换模式下，用于生成新的浮动文本的那个变换组件。 ")]
		[MMFEnumCondition("PositionMode", (int)PositionModes.TargetTransform)]
		public Transform TargetTransform;
		/// the direction to apply to the new floating text (leave it to 0 to let the Spawner decide based on its settings)
		[Tooltip("应用于新浮动文本的方向（将其设为0，以便让生成器根据其自身设置来决定方向） ")]
		public Vector3 Direction = Vector3.zero;

		protected Vector3 _playPosition;
		protected string _value;

		/// <summary>
		/// 在游戏运行时，我们要求指定通道上的生成器生成一个新的浮动文本。 
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
			switch (PositionMode)
			{
				case PositionModes.FeedbackPosition:
					_playPosition = Owner.transform.position;
					break;
				case PositionModes.PlayPosition:
					_playPosition = position;
					break;
				case PositionModes.TargetTransform:
					_playPosition = TargetTransform.position;
					break;
			}

			if (RoundingMethod != RoundingMethods.NoRounding)
			{
				switch (RoundingMethod)
				{
					case RoundingMethods.Ceil:
						

						break;
				}
			}

			feedbacksIntensity = ApplyRounding(feedbacksIntensity);
			
			_value = UseIntensityAsValue ? feedbacksIntensity.ToString() : Value;
			
			MMFloatingTextSpawnEvent.Trigger(ChannelData, _playPosition, _value, Direction, Intensity * intensityMultiplier, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient, ComputedTimescaleMode == TimescaleModes.Unscaled);
		}

		protected virtual float ApplyRounding(float value)
		{
			if (RoundingMethod == RoundingMethods.NoRounding)
			{
				return value;
			}

			switch (RoundingMethod)
			{
				case RoundingMethods.Round:
					return Mathf.Round(value);
				case RoundingMethods.Ceil:
					return Mathf.Ceil(value);
				case RoundingMethods.Floor:
					return Mathf.Floor(value);
			}

			return value;
		}
	}
}