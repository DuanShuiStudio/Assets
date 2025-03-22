using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于处理浮动文本的移动和行为的类，通常用于显示伤害数值文本。 
    /// 这是设计为由 MMFloatingTextSpawner 来生成实例的，不能单独使用。
    /// 它还需要特定的层级结构。你可以在 MMTools/Tools/MMFloatingText/Prefabs 文件夹中找到相关示例。
    /// </summary>
    public class MMFloatingText : MonoBehaviour
	{
		[Header("Bindings绑定")]

		/// the part of the prefab that we'll move
		[Tooltip("预制体中我们要移动的部分")]
		public Transform MovingPart;
		/// the part of the prefab that we'll rotate to face the target camera
		[Tooltip("预制体中我们将旋转以面向目标相机的部分。")]
		public Transform Billboard;
		/// the TextMesh used to display the value
		[Tooltip("用于显示数值的 TextMesh。")]
		public TextMesh TargetTextMesh;
        
		[Header("Debug")]

		/// the direction of this floating text, used for debug only
		[Tooltip("这个浮动文本的方向，仅用于调试。 ")]
		[MMReadOnly]
		public Vector3 Direction = Vector3.up;

		protected bool _useUnscaledTime = false;
		public virtual float GetTime() { return (_useUnscaledTime) ? Time.unscaledTime : Time.time; }
		public virtual float GetDeltaTime() { return _useUnscaledTime ? Time.unscaledDeltaTime : Time.unscaledTime; }
       
		protected float _startedAt;
		protected float _lifetime;
		protected Vector3 _newPosition;
		protected Color _initialTextColor;
		protected bool _animateMovement;
		protected bool _animateX;
		protected AnimationCurve _animateXCurve;
		protected float _remapXZero;
		protected float _remapXOne;
		protected bool _animateY;
		protected AnimationCurve _animateYCurve;
		protected float _remapYZero;
		protected float _remapYOne;
		protected bool _animateZ;
		protected AnimationCurve _animateZCurve;
		protected float _remapZZero;
		protected float _remapZOne;
		protected MMFloatingTextSpawner.AlignmentModes _alignmentMode;
		protected Vector3 _fixedAlignment;
		protected Vector3 _movementDirection;
		protected Vector3 _movingPartPositionLastFrame;
		protected bool _alwaysFaceCamera;
		protected Camera _targetCamera;
		protected Quaternion _targetCameraRotation;
		protected bool _animateOpacity;
		protected AnimationCurve _animateOpacityCurve;
		protected float _remapOpacityZero;
		protected float _remapOpacityOne;
		protected bool _animateScale;
		protected AnimationCurve _animateScaleCurve;
		protected float _remapScaleZero;
		protected float _remapScaleOne;
		protected bool _animateColor;
		protected Gradient _animateColorGradient;
		protected Vector3 _newScale;
		protected Color _newColor;

		protected float _elapsedTime;
		protected float _remappedTime;

        /// <summary>
        /// 当启用时，我们对我们的浮动文本进行初始化操作。 
        /// </summary>
        protected virtual void OnEnable()
		{
			Initialization();
		}

        /// <summary>
        /// 更改此浮动文本是否应使用不受时间缩放影响的时间（固定时间）。 
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetUseUnscaledTime(bool status, bool resetStartedAt)
		{
			_useUnscaledTime = status;
			if (resetStartedAt)
			{
				_startedAt = GetTime();    
			}
		}

        /// <summary>
        /// 存储开始时间和初始颜色。
        /// </summary>
        protected virtual void Initialization()
		{
			_startedAt = GetTime();
			if (TargetTextMesh != null)
			{
				_initialTextColor = TargetTextMesh.color;
			}            
		}

        /// <summary>
        /// 在每帧更新时，我们移动文本。
        /// </summary>
        protected virtual void Update()
		{
			UpdateFloatingText();
		}

        /// <summary>
        /// 处理文本的生命周期、移动、缩放、颜色、不透明度、对齐方式以及公告板效果（使其始终面向相机）。 
        /// </summary>
        protected virtual void UpdateFloatingText()
		{
            
			_elapsedTime = GetTime() - _startedAt;
			_remappedTime = MMMaths.Remap(_elapsedTime, 0f, _lifetime, 0f, 1f);
            
			// lifetime
			if (_elapsedTime > _lifetime)
			{
				TurnOff();
			}

			HandleMovement();
			HandleColor();
			HandleOpacity();
			HandleScale();
			HandleAlignment();            
			HandleBillboard();
		}

        /// <summary>
        /// 沿着指定的曲线移动文本。 
        /// </summary>
        protected virtual void HandleMovement()
		{
            // 位置移动
            if (_animateMovement)
			{
				this.transform.up = Direction;

				_newPosition.x = _animateX ? MMMaths.Remap(_animateXCurve.Evaluate(_remappedTime), 0f, 1, _remapXZero, _remapXOne) : 0f;
				_newPosition.y = _animateY ? MMMaths.Remap(_animateYCurve.Evaluate(_remappedTime), 0f, 1, _remapYZero, _remapYOne) : 0f;
				_newPosition.z = _animateZ ? MMMaths.Remap(_animateZCurve.Evaluate(_remappedTime), 0f, 1, _remapZZero, _remapZOne) : 0f;

                // 我们移动那个可移动的部分。 
                MovingPart.transform.localPosition = _newPosition;

                // 我们存储上一个位置。 
                if (Vector3.Distance(_movingPartPositionLastFrame, MovingPart.position) > 0.5f)
				{
					_movingPartPositionLastFrame = MovingPart.position;
				}
			}
		}

        /// <summary>
        /// 按照指定的渐变色来对文本的颜色进行动画处理。 
        /// </summary>
        protected virtual void HandleColor()
		{
			if (_animateColor)
			{
				_newColor = _animateColorGradient.Evaluate(_remappedTime);
				SetColor(_newColor);
			}
		}

        /// <summary>
        /// 根据指定的曲线对文本的不透明度进行动画处理。
        /// </summary>
        protected virtual void HandleOpacity()
		{
			if (_animateOpacity)
			{
				float newOpacity = MMMaths.Remap(_animateOpacityCurve.Evaluate(_remappedTime), 0f, 1f, _remapOpacityZero, _remapOpacityOne);
				SetOpacity(newOpacity);
			}
		}

        /// <summary>
        /// 根据指定的曲线对文本的缩放进行动画处理。
        /// </summary>
        protected virtual void HandleScale()
		{
			if (_animateScale)
			{
				_newScale = Vector3.one * MMMaths.Remap(_animateScaleCurve.Evaluate(_remappedTime), 0f, 1f, _remapScaleZero, _remapScaleOne);
				MovingPart.transform.localScale = _newScale;
			}
		}

        /// <summary>
        /// 处理文本的旋转，使其与固定对齐方式、初始方向或移动方向相匹配。 
        /// </summary>
        protected virtual void HandleAlignment()
		{
			if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.Fixed)
			{
				MovingPart.transform.up = _fixedAlignment;
			}
			else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchInitialDirection)
			{
				MovingPart.transform.up = this.transform.up;
			}
			else if (_alignmentMode == MMFloatingTextSpawner.AlignmentModes.MatchMovementDirection)
			{
				_movementDirection = MovingPart.position - _movingPartPositionLastFrame;
				MovingPart.transform.up = _movementDirection.normalized;
			}
		}

        /// <summary>
        /// 强制让文本面向相机。 
        /// </summary>
        protected virtual void HandleBillboard()
		{
			if (_alwaysFaceCamera)
			{
				_targetCameraRotation = _targetCamera.transform.rotation;
				Billboard.transform.LookAt(MovingPart.transform.position + _targetCameraRotation * Vector3.forward, _targetCameraRotation * MovingPart.up);
			}
		}

        /// <summary>
        /// 由生成器调用，设置所有必需的变量。 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lifetime"></param>
        /// <param name="direction"></param>
        /// <param name="animateMovement"></param>
        /// <param name="alignmentMode"></param>
        /// <param name="fixedAlignment"></param>
        /// <param name="alwaysFaceCamera"></param>
        /// <param name="targetCamera"></param>
        /// <param name="animateX"></param>
        /// <param name="animateXCurve"></param>
        /// <param name="remapXZero"></param>
        /// <param name="remapXOne"></param>
        /// <param name="animateY"></param>
        /// <param name="animateYCurve"></param>
        /// <param name="remapYZero"></param>
        /// <param name="remapYOne"></param>
        /// <param name="animateZ"></param>
        /// <param name="animateZCurve"></param>
        /// <param name="remapZZero"></param>
        /// <param name="remapZOne"></param>
        /// <param name="animateOpacity"></param>
        /// <param name="animateOpacityCurve"></param>
        /// <param name="remapOpacityZero"></param>
        /// <param name="remapOpacityOne"></param>
        /// <param name="animateScale"></param>
        /// <param name="animateScaleCurve"></param>
        /// <param name="remapScaleZero"></param>
        /// <param name="remapScaleOne"></param>
        /// <param name="animateColor"></param>
        /// <param name="animateColorGradient"></param>
        public virtual void SetProperties(string value, float lifetime, Vector3 direction, bool animateMovement, 
			MMFloatingTextSpawner.AlignmentModes alignmentMode, Vector3 fixedAlignment,
			bool alwaysFaceCamera, Camera targetCamera,
			bool animateX, AnimationCurve animateXCurve, float remapXZero, float remapXOne,
			bool animateY, AnimationCurve animateYCurve, float remapYZero, float remapYOne,
			bool animateZ, AnimationCurve animateZCurve, float remapZZero, float remapZOne,
			bool animateOpacity, AnimationCurve animateOpacityCurve, float remapOpacityZero, float remapOpacityOne,
			bool animateScale, AnimationCurve animateScaleCurve, float remapScaleZero, float remapScaleOne,
			bool animateColor, Gradient animateColorGradient)
		{
			SetText(value);
			_lifetime = lifetime;
			Direction = direction;
			_animateMovement = animateMovement;
			_animateX =  animateX;
			_animateXCurve =  animateXCurve;
			_remapXZero =  remapXZero;
			_remapXOne =  remapXOne;
			_animateY =  animateY;
			_animateYCurve =  animateYCurve;
			_remapYZero =  remapYZero;
			_remapYOne =  remapYOne;
			_animateZ =  animateZ;
			_animateZCurve =  animateZCurve;
			_remapZZero =  remapZZero;
			_remapZOne =  remapZOne;
			_alignmentMode = alignmentMode;
			_fixedAlignment = fixedAlignment;
			_alwaysFaceCamera = alwaysFaceCamera;
			_targetCamera = targetCamera;
			_animateOpacity = animateOpacity;
			_animateOpacityCurve = animateOpacityCurve;
			_remapOpacityZero = remapOpacityZero;
			_remapOpacityOne = remapOpacityOne;
			_animateScale = animateScale;
			_animateScaleCurve = animateScaleCurve;
			_remapScaleZero = remapScaleZero;
			_remapScaleOne = remapScaleOne;
			_animateColor = animateColor;
			_animateColorGradient = animateColorGradient;
			UpdateFloatingText();
		}

        /// <summary>
        /// 重置此文本的位置。
        /// </summary>
        public virtual void ResetPosition()
		{
			if (_animateMovement)
			{
				MovingPart.transform.localPosition = Vector3.zero;    
			}
			_movingPartPositionLastFrame = MovingPart.position - Direction;
		}

        /// <summary>
        /// 设置目标网格的文本值。 
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetText(string newValue)
		{
			TargetTextMesh.text = newValue;
		}

        /// <summary>
        /// 设置目标文本的颜色。
        /// </summary>
        /// <param name="newColor"></param>
        public virtual void SetColor(Color newColor)
		{
			TargetTextMesh.color = newColor;
		}

        /// <summary>
        /// 设置目标文本的不透明度。
        /// </summary>
        /// <param name="newOpacity"></param>
        public virtual void SetOpacity(float newOpacity)
		{
			_newColor = TargetTextMesh.color;
			_newColor.a = newOpacity;
			TargetTextMesh.color = _newColor;
		}

        /// <summary>
        /// 关闭该文本以便进行回收处理
        /// </summary>
        protected virtual void TurnOff()
		{
			this.gameObject.SetActive(false);
		}
	}
}