using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一种用于在相关动画器上触发动画（布尔值、整数、浮点数或触发器）的反馈，可以带有或不带随机性
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("这种反馈允许您向绑定在其检查器中的动画器发送布尔值、整数、浮点数或触发器参数，从而触发动画，可以带有或不带随机性")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Animation/Animation Parameter")]
	public class MMF_Animation : MMF_Feedback 
	{
        /// 一个静态布尔值用于一次性禁用这种类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;

        /// 触发器可能的模式        
        public enum TriggerModes { SetTrigger, ResetTrigger }

        ///设置值的可能方式
        public enum ValueModes { None, Constant, Random, Incremental }

        /// 为该反馈设置检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.AnimationColor; } }
		public override bool EvaluateRequiresSetup() { return (BoundAnimator == null); }
		public override string RequiredTargetText { get { return BoundAnimator != null ? BoundAnimator.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置 BoundAnimator 才能正常工作。您可以在下面设置一个"; } }
#endif

        /// 此反馈的持续时间是声明的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(DeclaredDuration); } set { DeclaredDuration = value;  } }
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundAnimator = FindAutomatedTarget<Animator>();

		[MMFInspectorGroup("Animation", true, 12, true)]
		/// the animator whose parameters you want to update
		[Tooltip("要更新其参数的动画器")]
		public Animator BoundAnimator;
		/// the list of extra animators whose parameters you want to update
		[Tooltip("要更新其参数的额外动画器列表")]
		public List<Animator> ExtraBoundAnimators;
		/// the duration for the player to consider. This won't impact your animation, but is a way to communicate to the MMF Player the duration of this feedback. Usually you'll want it to match your actual animation, and setting it can be useful to have this feedback work with holding pauses.
		[Tooltip("供播放器考虑的持续时间。这不会影响您的动画，而是向MMF播放器传达此反馈持续时间的一种方式。通常，您会希望它与实际动画匹配，并且设置它可以使此反馈与保持暂停功能协同工作")]
		public float DeclaredDuration = 0f;
        
		[MMFInspectorGroup("Trigger", true, 16)]
		/// if this is true, will update the specified trigger parameter
		[Tooltip("如果为真，将更新指定的触发器参数")]
		public bool UpdateTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("与该触发器交互所选的模式")]
		[MMFCondition("UpdateTrigger", true)]
		public TriggerModes TriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameter to, well, trigger when the feedback is played
		[Tooltip("当播放反馈时要触发的触发器动画器参数")]
		[MMFCondition("UpdateTrigger", true)]
		public string TriggerParameterName;
        
		[MMFInspectorGroup("Random Trigger", true, 20)]
		/// if this is true, will update a random trigger parameter, picked from the list below
		[Tooltip("如果为真，将从下面的列表中选择一个随机触发器参数进行更新")]
		public bool UpdateRandomTrigger = false;
		/// the selected mode to interact with this trigger
		[Tooltip("the selected mode to interact with this trigger")]
		[MMFCondition("UpdateRandomTrigger", true)]
		public TriggerModes RandomTriggerMode = TriggerModes.SetTrigger;
		/// the trigger animator parameters to trigger at random when the feedback is played
		[Tooltip("当播放反馈时随机触发的触发器动画器参数")]
		public List<string> RandomTriggerParameterNames;
        
		[MMFInspectorGroup("Bool", true, 17)]
		/// if this is true, will update the specified bool parameter
		[Tooltip("如果为true，将更新指定的布尔型参数")]
		public bool UpdateBool = false;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的布尔型参数")]
		[MMFCondition("UpdateBool", true)]
		public string BoolParameterName;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("在布尔模式下，是否将布尔参数设置为 true 或 false。")]
		[MMFCondition("UpdateBool", true)]
		public bool BoolParameterValue = true;
        
		[MMFInspectorGroup("Random Bool", true, 19)]
		/// if this is true, will update a random bool parameter picked from the list below
		[Tooltip("如果为真，将从下面的列表中选择一个随机布尔参数进行更新")]
		public bool UpdateRandomBool = false;
		/// when in bool mode, whether to set the bool parameter to true or false
		[Tooltip("在布尔模式下，是否将布尔参数设置为 true 或 false。")]
		[MMFCondition("UpdateRandomBool", true)]
		public bool RandomBoolParameterValue = true;
		/// the bool parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的布尔型参数")]
		public List<string> RandomBoolParameterNames;
        
		[MMFInspectorGroup("Int", true, 24)]
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的int型参数")]
		public ValueModes IntValueMode = ValueModes.None;
		/// the int parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的int型参数")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string IntParameterName;
		/// the value to set to that int parameter
		[Tooltip("要设置给该int型参数的值")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Constant)]
		public int IntValue;
		/// the min value (inclusive) to set at random to that int parameter
		[Tooltip("要设置给该int型参数的最小值（包含）")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMin;
		/// the max value (exclusive) to set at random to that int parameter
		[Tooltip("要设置给该int型参数的最大值（包含）")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Random)]
		public int IntValueMax = 5;
		/// the value to increment that int parameter by
		[Tooltip("要增加给该int型参数的值")]
		[MMFEnumCondition("IntValueMode", (int)ValueModes.Incremental)]
		public int IntIncrement = 1;

		[MMFInspectorGroup("Float", true, 22)]
		/// the Float parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的float型参数")]
		public ValueModes FloatValueMode = ValueModes.None;
		/// the float parameter to turn true when the feedback gets played
		[Tooltip("当播放反馈时将变为true的float型参数")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant, (int)ValueModes.Random, (int)ValueModes.Incremental)]
		public string FloatParameterName;
		/// the value to set to that float parameter
		[Tooltip("要设置给该float型参数的值")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Constant)]
		public float FloatValue;
		/// the min value (inclusive) to set at random to that float parameter
		[Tooltip("要设置给该float型参数的最小值（包含）")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMin;
		/// the max value (exclusive) to set at random to that float parameter
		[Tooltip("要设置给该float型参数的最小值（包含）")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Random)]
		public float FloatValueMax = 5;
		/// the value to increment that float parameter by
		[Tooltip("要增加给该float型参数的值")]
		[MMFEnumCondition("FloatValueMode", (int)ValueModes.Incremental)]
		public float FloatIncrement = 1;

		[MMFInspectorGroup("Layer Weights", true, 22)]
		/// whether or not to set layer weights on the specified layer when playing this feedback
		[Tooltip("当播放此反馈时，是否在指定图层上设置图层权重")]
		public bool SetLayerWeight = false;
		/// the index of the layer to target when changing layer weights
		[Tooltip("当改变图层权重时，要作为目标的图层的索引")]
		[MMFCondition("SetLayerWeight", true)]
		public int TargetLayerIndex = 1;
		/// the new weight to set on the target animator layer
		[Tooltip("要在目标动画器图层上设置的新权重")]
		[MMFCondition("SetLayerWeight", true)]
		public float NewWeight = 0.5f;

		protected int _triggerParameter;
		protected int _boolParameter;
		protected int _intParameter;
		protected int _floatParameter;
		protected List<int> _randomTriggerParameters;
		protected List<int> _randomBoolParameters;

        /// <summary>
        /// 自定义初始化
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			_triggerParameter = Animator.StringToHash(TriggerParameterName);
			_boolParameter = Animator.StringToHash(BoolParameterName);
			_intParameter = Animator.StringToHash(IntParameterName);
			_floatParameter = Animator.StringToHash(FloatParameterName);

			_randomTriggerParameters = new List<int>();
			foreach (string name in RandomTriggerParameterNames)
			{
				_randomTriggerParameters.Add(Animator.StringToHash(name));
			}

			_randomBoolParameters = new List<int>();
			foreach (string name in RandomBoolParameterNames)
			{
				_randomBoolParameters.Add(Animator.StringToHash(name));
			}
		}

        /// <summary>
        /// 在播放时，检查是否绑定了动画器并触发参数
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (BoundAnimator == null)
			{
				Debug.LogWarning("没有为 " + Owner.name + " 设置动画器");
				return;
			}

			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);

			ApplyValue(BoundAnimator, intensityMultiplier);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				ApplyValue(animator, intensityMultiplier);
			}
		}

        /// <summary>
        /// 将值应用于目标动画器
        /// </summary>
        /// <param name="targetAnimator"></param>
        /// <param name="intensityMultiplier"></param>
        protected virtual void ApplyValue(Animator targetAnimator, float intensityMultiplier)
		{
			if (UpdateTrigger)
			{
				if (TriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(_triggerParameter);
				}
				if (TriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(_triggerParameter);
				}
			}
            
			if (UpdateRandomTrigger)
			{
				int randomParameter = _randomTriggerParameters[Random.Range(0, _randomTriggerParameters.Count)];
                
				if (RandomTriggerMode == TriggerModes.SetTrigger)
				{
					targetAnimator.SetTrigger(randomParameter);
				}
				if (RandomTriggerMode == TriggerModes.ResetTrigger)
				{
					targetAnimator.ResetTrigger(randomParameter);
				}
			}

			if (UpdateBool)
			{
				targetAnimator.SetBool(_boolParameter, BoolParameterValue);
			}

			if (UpdateRandomBool)
			{
				int randomParameter = _randomBoolParameters[Random.Range(0, _randomBoolParameters.Count)];
                
				targetAnimator.SetBool(randomParameter, RandomBoolParameterValue);
			}

			switch (IntValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetInteger(_intParameter, IntValue);
					break;
				case ValueModes.Incremental:
					int newValue = targetAnimator.GetInteger(_intParameter) + IntIncrement;
					targetAnimator.SetInteger(_intParameter, newValue);
					break;
				case ValueModes.Random:
					int randomValue = Random.Range(IntValueMin, IntValueMax);
					targetAnimator.SetInteger(_intParameter, randomValue);
					break;
			}

			switch (FloatValueMode)
			{
				case ValueModes.Constant:
					targetAnimator.SetFloat(_floatParameter, FloatValue * intensityMultiplier);
					break;
				case ValueModes.Incremental:
					float newValue = targetAnimator.GetFloat(_floatParameter) + FloatIncrement * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, newValue);
					break;
				case ValueModes.Random:
					float randomValue = Random.Range(FloatValueMin, FloatValueMax) * intensityMultiplier;
					targetAnimator.SetFloat(_floatParameter, randomValue);
					break;
			}

			if (SetLayerWeight)
			{
				targetAnimator.SetLayerWeight(TargetLayerIndex, NewWeight);
			}
		}

        /// <summary>
        /// 在停止时，将布尔参数设置为false
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !UpdateBool || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			BoundAnimator.SetBool(_boolParameter, false);
			foreach (Animator animator in ExtraBoundAnimators)
			{
				animator.SetBool(_boolParameter, false);
			}
		}
	}
}