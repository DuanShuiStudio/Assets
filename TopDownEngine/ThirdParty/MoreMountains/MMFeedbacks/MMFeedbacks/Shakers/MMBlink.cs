using UnityEngine;
using System.Collections;
using MoreMountains.Feedbacks;
using System;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// Describes a blink phase, defined by a duration for the phase, and the time it should remain inactive and active, sequentially
	/// For the duration of the phase, the object will be off for OffDuration, then on for OnDuration, then off again for OffDuration, etc
	/// If you want a grenade to blink briefly every .2 seconds, for 1 second, these parameters are what you're after :
	/// PhaseDuration = 1f;
	/// OffDuration = 0.2f;
	/// OnDuration = 0.1f;
	/// </summary>
	[Serializable]
	public class BlinkPhase
	{
		/// the duration of that specific phase, in seconds
		public float PhaseDuration = 1f;
		/// the time the object should remain off
		public float OffDuration = 0.2f;
		/// the time the object should then remain on
		public float OnDuration = 0.1f;
		/// the speed at which to lerp to off state
		public float OffLerpDuration = 0.05f;
		/// the speed at which to lerp to on state
		public float OnLerpDuration = 0.05f;
	}

	[Serializable]
	public class BlinkTargetRenderer
	{
		public Renderer TargetRenderer;
		public int TargetMaterialIndex;
	}

    /// <summary>
    /// 将这个类添加到一个游戏对象（GameObject）上，以便让它闪烁，可通过启用/禁用该游戏对象、更改其透明度（alpha 值）、发光强度，或者更改着色器（shader）上的某个值来实现。  
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MM Blink")]
	public class MMBlink : MMMonoBehaviour
	{
        /// 闪烁对象的可能状态
        public enum States { On, Off }
        /// 使一个物体闪烁的可能方法
        public enum Methods { SetGameObjectActive, MaterialAlpha, MaterialEmissionIntensity, ShaderFloatValue }
        
		[MMInspectorGroup("Blink Method", true, 17)] 
		/// the selected method to blink the target object
		[Tooltip("用于使目标对象闪烁的选定方法")]
		public Methods Method = Methods.SetGameObjectActive;
		/// the object to set active/inactive if that method was chosen
		[Tooltip("如果选择了那种方法（使物体闪烁的方法），那么这个就是要设置为激活/非激活状态的对象。  ")]
		[MMFEnumCondition("Method", (int)Methods.SetGameObjectActive)]
		public GameObject TargetGameObject;
		/// the target renderer to work with
		[Tooltip("要使用的目标渲染器。 ")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public Renderer TargetRenderer;
		/// the material index to target
		[Tooltip("要作为目标的材质索引")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public int MaterialIndex = 0;
		/// the shader property to alter a float on
		[Tooltip("要更改其浮点数的着色器属性")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public string ShaderPropertyName = "_Color";
		/// the value to apply when blinking is off
		[Tooltip("当闪烁关闭时要应用的值。 ")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public float OffValue = 0f;
		/// the value to apply when blinking is on
		[Tooltip("当闪烁开启时要应用的值。 ")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public float OnValue = 1f;
		/// whether to lerp these values or not
		[Tooltip("是否对这些值进行线性插值。")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public bool LerpValue = true;
		/// the curve to apply to the lerping
		[Tooltip("要应用于线性插值的曲线。")]
		[MMFEnumCondition("Method", (int)Methods.MaterialAlpha, (int)Methods.MaterialEmissionIntensity, (int)Methods.ShaderFloatValue)]
		public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.05f), new Keyframe(1, 0));
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("如果此选项为真，该组件将使用材质属性块，而不是直接操作材质实例。")] 
		public bool UseMaterialPropertyBlocks = false;
		
		[MMInspectorGroup("Extra Targets", true, 12)] 
		/// a list of optional extra renderers and their material index to target
		[Tooltip("一份可选的额外渲染器及其要作为目标的材质索引的列表。 ")]
		public List<BlinkTargetRenderer> ExtraRenderers;
		/// a list of optional extra game objects to target
		[Tooltip("一份可选的要作为目标的额外游戏对象的列表。 ")]
		public List<GameObject> ExtraGameObjects;

		[MMInspectorGroup("State", true, 18)] 
		/// whether the object should blink or not
		[Tooltip("该对象是否应该闪烁。")]
		public bool Blinking = true;
		/// whether or not to force a certain state on exit
		[Tooltip("在退出时是否强制进入某种特定状态")]
		public bool ForceStateOnExit = false;
		/// the state to apply on exit
		[Tooltip("退出时要应用的状态。 ")]
		[MMFCondition("ForceStateOnExit", true)]
		public States StateOnExit = States.On;

		[MMInspectorGroup("TimeScale", true, 120)] 
		/// whether or not this MMBlink should operate on unscaled time 
		[Tooltip("这个MMBlink（可能是某个组件或功能的名称）是否应该基于非缩放时间来运行。 ")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
        
		[MMInspectorGroup("Sequence", true, 121)] 
		/// how many times the sequence should repeat (-1 : infinite)
		[Tooltip("该序列应该重复多少次（-1：表示无限次） ")]
		public int RepeatCount = 0;
		/// The list of phases to apply blinking with
		[Tooltip("用于应用闪烁效果的阶段列表")]
		public List<BlinkPhase> Phases;
        
		[MMInspectorGroup("Debug", true, 122)] 
		
		[MMInspectorButtonBar(new string[] { "ToggleBlinking", "StartBlinking", "StopBlinking" }, 
			new string[] { "ToggleBlinking", "StartBlinking", "StopBlinking" }, 
			new bool[] { true, true, true },
			new string[] { "main-call-to-action", "", "" })]
		public bool DebugToolbar;
		
		/// is the blinking object in an active state right now?
		[Tooltip("目前这个正在闪烁的对象处于激活状态吗？ ")]
		[MMFReadOnly]
		public bool Active = false;
		/// the index of the phase we're currently in
		[Tooltip("我们当前所处阶段的索引。")]
		[MMFReadOnly]
		public int CurrentPhaseIndex = 0;
        
        
		public virtual float GetTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (TimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }

		protected float _lastBlinkAt = 0f;
		protected float _currentPhaseStartedAt = 0f;
		protected float _currentBlinkDuration;
		protected float _currentLerpDuration;
		protected int _propertyID;
		protected float _initialShaderFloatValue;
		protected Color _initialColor;
		protected Color _currentColor;
		protected int _repeatCount;
		protected MaterialPropertyBlock _propertyBlock;
		protected List<MaterialPropertyBlock> _extraPropertyBlocks;
		protected List<Color> _extraInitialColors;

        /// <summary>
        /// 如果该对象原本没有在闪烁，就使其开始闪烁；否则，就停止其闪烁。 
        /// </summary>
        public virtual void ToggleBlinking()
		{
			Blinking = !Blinking;
			ResetBlinkProperties();
		}

        /// <summary>
        /// 使该对象开始闪烁。
        /// </summary>
        public virtual void StartBlinking()
		{
			this.enabled = true;
			Blinking = true;
			ResetBlinkProperties();
		}

        /// <summary>
        /// 使该对象停止闪烁。
        /// </summary>
        public virtual void StopBlinking()
		{
			Blinking = false;
			ResetBlinkProperties();
		}

        /// <summary>
        /// 在执行更新操作时，如果符合应该闪烁的条件，我们就会让其闪烁。 
        /// </summary>
        protected virtual void Update()
		{
			DetermineState();

			if (!Blinking)
			{
				return;
			}

			Blink();
		}

        /// <summary>
        /// 确定当前阶段，并判断该对象应该处于激活状态还是非激活状态。 
        /// </summary>
        protected virtual void DetermineState()
		{
			DetermineCurrentPhase();
            
			if (!Blinking)
			{
				return;
			}

			if (Active)
			{
				if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OnDuration)
				{
					Active = false;
					_lastBlinkAt = GetTime();
				}
			}
			else
			{
				if (GetTime() - _lastBlinkAt > Phases[CurrentPhaseIndex].OffDuration)
				{
					Active = true;
					_lastBlinkAt = GetTime();
				}
			}
			_currentBlinkDuration = Active ? Phases[CurrentPhaseIndex].OnDuration : Phases[CurrentPhaseIndex].OffDuration;
			_currentLerpDuration = Active ? Phases[CurrentPhaseIndex].OnLerpDuration : Phases[CurrentPhaseIndex].OffLerpDuration;
		}

        /// <summary>
        /// 根据对象计算出的状态使其闪烁。 
        /// </summary>
        protected virtual void Blink()
		{
			float currentValue = _currentColor.a;
			float initialValue = Active ? OffValue : OnValue;
			float targetValue = Active ? OnValue : OffValue;
			float newValue = targetValue;

			if (LerpValue && (GetTime() - _lastBlinkAt < _currentLerpDuration))
			{
				float t = MMFeedbacksHelpers.Remap(GetTime() - _lastBlinkAt, 0f, _currentLerpDuration, 0f, 1f);
				newValue = Curve.Evaluate(t);
				newValue = MMFeedbacksHelpers.Remap(newValue, 0f, 1f, initialValue, targetValue);
			}
			else
			{
				newValue = targetValue;
			}
            
			ApplyBlink(Active, newValue);
		}

        /// <summary>
        /// 闪烁的持续时间是其各个阶段持续时间的总和，再加上完整重复所有阶段所花费的时间。 
        /// </summary>
        public virtual float Duration
		{
			get
			{
				if ((RepeatCount < 0)
				    || (Phases.Count == 0))
				{
					return 0f;
				}

				float totalDuration = 0f;
				foreach (BlinkPhase phase in Phases)
				{
					totalDuration += phase.PhaseDuration;
				}
				return totalDuration + totalDuration * RepeatCount;
			}
		}

        /// <summary>
        /// 根据对象的类型将闪烁效果应用到该对象上。 
        /// </summary>
        /// <param name="active"></param>
        /// <param name="value"></param>
        protected virtual void ApplyBlink(bool active, float value)
		{
			switch (Method)
			{
				case Methods.SetGameObjectActive:
					TargetGameObject.SetActive(active);
					foreach (GameObject go in ExtraGameObjects)
					{
						go.SetActive(active);
					}
					break;
				case Methods.MaterialAlpha:
					_currentColor.a = value;
					ApplyCurrentColor(TargetRenderer);
					for (var index = 0; index < ExtraRenderers.Count; index++)
					{
						var blinkRenderer = ExtraRenderers[index];
						ApplyCurrentColor(blinkRenderer.TargetRenderer);
					}
					break;
				case Methods.MaterialEmissionIntensity:
					_currentColor = _initialColor * value;
					ApplyCurrentColor(TargetRenderer);
					for (var index = 0; index < ExtraRenderers.Count; index++)
					{
						var blinkRenderer = ExtraRenderers[index];
						ApplyCurrentColor(blinkRenderer.TargetRenderer);
					}
					break;
				case Methods.ShaderFloatValue:
					ApplyFloatValue(TargetRenderer, value);
					for (var index = 0; index < ExtraRenderers.Count; index++)
					{
						var blinkRenderer = ExtraRenderers[index];
						ApplyFloatValue(blinkRenderer.TargetRenderer, value);
					}
					break;
			}
		}

		protected virtual void ApplyFloatValue(Renderer targetRenderer, float value)
		{
			if (UseMaterialPropertyBlocks)
			{
				targetRenderer.GetPropertyBlock(_propertyBlock, MaterialIndex);
				_propertyBlock.SetFloat(_propertyID, value);
				targetRenderer.SetPropertyBlock(_propertyBlock);
			}
			else
			{
				targetRenderer.materials[MaterialIndex].SetFloat(_propertyID, value); 
			}
		}

		protected virtual void ApplyCurrentColor(Renderer targetRenderer)
		{
			if (UseMaterialPropertyBlocks)
			{
				targetRenderer.GetPropertyBlock(_propertyBlock, MaterialIndex);
				_propertyBlock.SetColor(_propertyID, _currentColor);
				targetRenderer.SetPropertyBlock(_propertyBlock);
			}
			else
			{
				targetRenderer.materials[MaterialIndex].SetColor(_propertyID, _currentColor);    
			}
		}

        /// <summary>
        /// 根据各个阶段的持续时长来确定当前的阶段索引。 
        /// </summary>
        protected virtual void DetermineCurrentPhase()
		{
            // 如果阶段持续时间为空或者小于等于零，我们将永远处于该阶段，然后返回。 
            if (Phases[CurrentPhaseIndex].PhaseDuration <= 0)
			{
				return;
			}
            // 如果某个阶段的持续时间已过，我们就会进入到下一个阶段。 
            if (GetTime() - _currentPhaseStartedAt > Phases[CurrentPhaseIndex].PhaseDuration)
			{
				CurrentPhaseIndex++;
				_currentPhaseStartedAt = GetTime();
			}
			if (CurrentPhaseIndex > Phases.Count -1)
			{
				CurrentPhaseIndex = 0;
				if (RepeatCount != -1)
				{
					_repeatCount--;
					if (_repeatCount < 0)
					{
						ResetBlinkProperties();

						if (ForceStateOnExit)
						{
							if (StateOnExit == States.Off)
							{
								ApplyBlink(false, 0f);
							}
							else
							{
								ApplyBlink(true, 1f);
							}
						}

						Blinking = false;
					}
				}                
			}
		}

        /// <summary>
        /// 在启用时，初始化闪烁属性。 
        /// </summary>
        protected virtual void OnEnable()
		{
			InitializeBlinkProperties();            
		}

        /// <summary>
        /// 重置计数器，获取属性以及初始颜色。 
        /// </summary>
        protected virtual void InitializeBlinkProperties()
		{
			if (Phases.Count == 0)
			{
				Debug.LogError("MMBlink : 为了使此组件能够正常工作，你至少需要定义一个阶段。 ");
				this.enabled = false;
				return;
			}
            
			_currentPhaseStartedAt = GetTime();
			CurrentPhaseIndex = 0;
			_repeatCount = RepeatCount;
			_propertyBlock = new MaterialPropertyBlock();
            
			switch (Method)
			{
				case Methods.MaterialAlpha:
					GetInitialColor();
					break;
				case Methods.MaterialEmissionIntensity:
					GetInitialColor();
					break;
				case Methods.ShaderFloatValue:
					GetInitialFloatValue();
					break;
			}
		}

		protected virtual void GetInitialColor()
		{
			TargetRenderer.GetPropertyBlock(_propertyBlock, MaterialIndex);
			_propertyID = Shader.PropertyToID(ShaderPropertyName);
			_initialColor = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterials[MaterialIndex].GetColor(_propertyID) : TargetRenderer.materials[MaterialIndex].GetColor(_propertyID);
			_currentColor = _initialColor;
		}

		protected virtual void GetInitialFloatValue()
		{
			TargetRenderer.GetPropertyBlock(_propertyBlock, MaterialIndex);
			_propertyID = Shader.PropertyToID(ShaderPropertyName);
			_initialShaderFloatValue = UseMaterialPropertyBlocks ? TargetRenderer.sharedMaterials[MaterialIndex].GetFloat(_propertyID) : TargetRenderer.materials[MaterialIndex].GetFloat(_propertyID);
		}

        /// <summary>
        /// 将闪烁属性重置为原始值。
        /// </summary>
        protected virtual void ResetBlinkProperties()
		{
			_currentPhaseStartedAt = GetTime();
			CurrentPhaseIndex = 0;
			_repeatCount = RepeatCount;

			float value = 1f;
			if (Method == Methods.ShaderFloatValue)
			{
				value = _initialShaderFloatValue; 
			}
			ApplyBlink(false, value);
		}

		protected void OnDisable()
		{
			if (ForceStateOnExit)
			{
				if (StateOnExit == States.Off)
				{
					ApplyBlink(false, 0f);
				}
				else
				{
					ApplyBlink(true, 1f);
				}
			}
		}
	}
}