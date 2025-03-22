using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if UNITY_EDITOR
using UnityEditor.Animations;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 用于存储字符动画参数定义的结构体，将由CharacterAnimationParametersInitializer类使用
    /// </summary>
    public struct TopDownCharacterAnimationParameter
	{
        /// 参数的名称
        public string ParameterName;
        /// 参数的类型
        public AnimatorControllerParameterType ParameterType;

		public TopDownCharacterAnimationParameter(string name, AnimatorControllerParameterType type)
		{
			ParameterName = name;
			ParameterType = type;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class CharacterAnimationParametersInitializer : TopDownMonoBehaviour
	{
		[Header("Initialization初始化")]
		/// if this is true, this component will remove itself after adding the character parameters
		[Tooltip("如果这个条件为真，那么在添加角色参数后，这个组件将自行移除")]
		public bool AutoRemoveAfterInitialization = true;
		[MMInspectorButton("AddAnimationParameters")]
		public bool AddAnimationParametersButton;

		protected TopDownCharacterAnimationParameter[] ParametersArray = new TopDownCharacterAnimationParameter[]
		{
			new TopDownCharacterAnimationParameter("Alive", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Grounded", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Idle", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Walking", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Running", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Activating", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Crouching", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Crawling", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Damage", AnimatorControllerParameterType.Trigger),
			new TopDownCharacterAnimationParameter("Dashing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DamageDashing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DashingDirectionX", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashingDirectionY", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashingDirectionZ", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("DashStarted", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Death", AnimatorControllerParameterType.Trigger),
			new TopDownCharacterAnimationParameter("Health", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("HealthAsInt", AnimatorControllerParameterType.Int),
			new TopDownCharacterAnimationParameter("FacingDirection2D", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("FallingDownHole", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("WeaponEquipped", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("WeaponEquippedID", AnimatorControllerParameterType.Int),
			new TopDownCharacterAnimationParameter("Jumping", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("DoubleJumping", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("HitTheGround", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Random", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RandomConstant", AnimatorControllerParameterType.Int),
			new TopDownCharacterAnimationParameter("Direction", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("Speed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("xSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("ySpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("zSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("HorizontalDirection", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("VerticalDirection", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeForwardSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeLateralSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeForwardSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RelativeLateralSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedForwardSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedLateralSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("RemappedSpeedNormalized", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("Stunned", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("Pushing", AnimatorControllerParameterType.Bool),
			new TopDownCharacterAnimationParameter("YRotationSpeed", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("YRotationOffset", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("TransformVelocityX", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("TransformVelocityY", AnimatorControllerParameterType.Float),
			new TopDownCharacterAnimationParameter("TransformVelocityZ", AnimatorControllerParameterType.Float)
		};

		protected Animator _animator;
		#if UNITY_EDITOR
		protected AnimatorController _controller;
		#endif
		protected List<string> _parameters = new List<string>();

        /// <summary>
        /// 在角色的动画器上添加所有默认动画参数
        /// </summary>
        public virtual void AddAnimationParameters()
		{
            // 我们抓住动画器
            _animator = this.gameObject.GetComponent<Animator>();
			if (_animator == null)
			{
				Debug.LogError(".");
			}

            // 我们抓取控制器
#if UNITY_EDITOR
            _controller = _animator.runtimeAnimatorController as AnimatorController;
			if (_controller == null)
			{
				Debug.LogError("你需要将AnimationParameterInitializer类添加到一个带有Animator的游戏对象上。");
			}
#endif

            // 我们存储它的参数
            _parameters.Clear();
			foreach (AnimatorControllerParameter param in _animator.parameters)
			{
				_parameters.Add(param.name);
			}

            // 我们添加列出的所有参数
            foreach (TopDownCharacterAnimationParameter parameter in ParametersArray)
			{
				if (!_parameters.Contains(parameter.ParameterName))
				{
					#if UNITY_EDITOR
					_controller.AddParameter(parameter.ParameterName, parameter.ParameterType);
					#endif
				}
			}

            // 如果需要，我们将删除该组件
            if (AutoRemoveAfterInitialization)
			{
				DestroyImmediate(this);
			}
		}
	}
}