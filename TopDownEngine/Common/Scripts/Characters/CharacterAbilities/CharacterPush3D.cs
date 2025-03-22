using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到角色中，使其能够推动刚体
    /// 动画参数 : Pushing (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Push 3D")] 
	public class CharacterPush3D : CharacterAbility
	{
		[Header("Physics interaction物理交互")]
		/// if this is true, the controller will be able to apply forces to colliding rigidbodies
		[Tooltip("如果这个条件为真，控制器将能够对碰撞的刚体施加力")]
		public bool AllowPhysicsInteractions = true;
		/// the length of the ray to cast in front of the character to detect pushables
		[Tooltip("在角色前方投射的射线长度，用于检测可推动的物体")]
		public float PhysicsInteractionsRaycastLength = 0.05f;
		/// the offset to apply to the origin of the physics interaction raycast (by default, the character's collider's center
		[Tooltip("应用到物理交互射线投射原点的偏移（默认情况下，角色碰撞体的中心）")]
		public Vector3 PhysicsInteractionsRaycastOffset = Vector3.zero;
		/// the force to apply when colliding with rigidbodies
		[Tooltip("与刚体碰撞时施加的力")]
		public float PushPower = 1850f;

		protected const string _pushingAnimationParameterName = "Pushing";
		protected int _pushingAnimationParameter;
		protected CharacterController _characterController;
		protected RaycastHit _raycastHit;
		protected Rigidbody _pushedRigidbody;
		protected Vector3 _pushDirection;
		protected bool _pushing = false;

        /// <summary>
        /// 在init上，抓取控制器
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_characterController = _controller.GetComponent<CharacterController>();
			_controller3D = _controller.GetComponent<TopDownController3D>();
		}

        /// <summary>
        /// 每一帧，处理物理交互
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (!AbilityAuthorized
			    || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				return;
			}

			HandlePhysicsInteractions();
		}

        /// <summary>
        /// 检查是否有可推的物体并施加指定的力
        /// </summary>
        protected virtual void HandlePhysicsInteractions()
		{
			if (!AllowPhysicsInteractions)
			{
				return;
			}

            // 我们向移动方向投射光线来处理推物体
            Physics.Raycast(_controller3D.transform.position + _characterController.center + PhysicsInteractionsRaycastOffset, _controller.CurrentMovement.normalized, out _raycastHit, 
				_characterController.radius + _characterController.skinWidth + PhysicsInteractionsRaycastLength, _controller3D.ObstaclesLayerMask);

			_pushing = (_raycastHit.collider != null);
            
			if (_pushing)
			{
				HandlePush(_controller3D, _raycastHit, _raycastHit.point);
			}
		}

        /// <summary>
        /// 在撞击位置为碰撞对象添加一个力，与物理世界进行交互
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="hitPosition"></param>
        protected virtual void HandlePush(TopDownController3D controller3D, RaycastHit hit, Vector3 hitPosition)
		{
			_pushedRigidbody = hit.collider.attachedRigidbody;
            
			if ((_pushedRigidbody == null) || (_pushedRigidbody.isKinematic))
			{
				return;
			}
            
			_pushDirection.x = controller3D.CurrentMovement.normalized.x;
			_pushDirection.y = 0;
			_pushDirection.z = controller3D.CurrentMovement.normalized.z;

			_pushedRigidbody.AddForceAtPosition(_pushDirection * PushPower * Time.deltaTime, hitPosition);
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_pushingAnimationParameterName, AnimatorControllerParameterType.Float, out _pushingAnimationParameter);
		}

        /// <summary>
        /// 将当前速度和推送状态的当前值发送给动画器
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _pushingAnimationParameter, _pushing,_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}