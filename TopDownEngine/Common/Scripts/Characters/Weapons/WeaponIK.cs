using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类允许一个3D角色抓住它当前武器的把手，并看向它瞄准的方向
    /// 这里有一些设置。你需要在你的角色上有一个CharacterHandleWeapon组件，它需要一个带有IKPass活动的动画器（这是在动画器的Layers选项卡中设置的）
    /// 动画器的化身必须设置为 humanoid类人型
    /// 你需要将这个脚本（WeaponIK）放在与动画器相同的GameObject上（否则它不会起作用）
    /// 最后，你需要在你的武器上设置左右把手（或只设置其中一个）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon IK")]
	public class WeaponIK : TopDownMonoBehaviour
	{
		[Header("Bindings绑定")]
		/// The transform to use as a target for the left hand
		[Tooltip("用作左手目标的变换")]
		public Transform LeftHandTarget = null;
		/// The transform to use as a target for the right hand
		[Tooltip("用作右手目标的变换")]
		public Transform RightHandTarget = null;

		[Header("Attachments附件")]
		/// whether or not to attach the left hand to its target
		[Tooltip("是否将左手附加到其目标上")]
		public bool AttachLeftHand = true;
		/// whether or not to attach the right hand to its target
		[Tooltip("是否将右手附加到其目标上")]
		public bool AttachRightHand = true;
        
		[Header("Head")]
        /// 使用IK时应用于头部查看权重的最小和最大权重
        [MMVector("Min","Max")]
		public Vector2 HeadWeights = new Vector2(0f, 1f);
        
		protected Animator _animator;

		protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
		}

        /// <summary>
        /// 在动画器的IK过程中，尝试将化身的手附加到武器上
        /// </summary>
        protected virtual void OnAnimatorIK(int layerIndex)
		{
			if (_animator == null)
			{
				return;
			}

            //如果IK处于活动状态，则直接将位置和旋转设置为目标值

            if (AttachLeftHand)
			{
				if (LeftHandTarget != null)
				{
					AttachHandToHandle(AvatarIKGoal.LeftHand, LeftHandTarget);

					_animator.SetLookAtWeight(HeadWeights.y);
					_animator.SetLookAtPosition(LeftHandTarget.position);
				}
				else
				{
					DetachHandFromHandle(AvatarIKGoal.LeftHand);
				}
			}
			
			if (AttachRightHand)
			{
				if (RightHandTarget != null)
				{
					AttachHandToHandle(AvatarIKGoal.RightHand, RightHandTarget);
				}
				else
				{
					DetachHandFromHandle(AvatarIKGoal.RightHand);
				}
			}
			

		}

        /// <summary>
        /// Attaches the hands to the handles将手附加到把手上
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="handle"></param>
        protected virtual void AttachHandToHandle(AvatarIKGoal hand, Transform handle)
		{
			_animator.SetIKPositionWeight(hand,1);
			_animator.SetIKRotationWeight(hand,1);  
			_animator.SetIKPosition(hand,handle.position);
			_animator.SetIKRotation(hand,handle.rotation);
		}

        /// <summary>
        /// 如果IK不处于活动状态，则将手从把手上分离，并将手和头的位置和旋转设置回原始位置
        /// </summary>
        /// <param name="hand">Hand.</param>
        protected virtual void DetachHandFromHandle(AvatarIKGoal hand)
		{
			_animator.SetIKPositionWeight(hand,0);
			_animator.SetIKRotationWeight(hand,0); 
			_animator.SetLookAtWeight(HeadWeights.x);
		}

        /// <summary>
        /// 将角色的手绑定到把手目标上
        /// </summary>
        /// <param name="leftHand">Left hand.</param>
        /// <param name="rightHand">Right hand.</param>
        public virtual void SetHandles(Transform leftHand, Transform rightHand)
		{
			LeftHandTarget = leftHand;
			RightHandTarget = rightHand;
		}
	}
}