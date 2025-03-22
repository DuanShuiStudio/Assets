using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到一个触发碰撞器上，它将让你为进入它的角色应用移动速度倍增器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Movement Zone")]
	public class MovementZone : ButtonActivated
	{
        /// <summary>
        /// 用于存储移动区域设置的类
        /// </summary>
        public class MovementZoneCollidingEntity
		{
			public Character TargetCharacter;
			public CharacterMovement TargetCharacterMovement;
		}

		[MMInspectorGroup("Movement Zone", true, 18)]
		/// the new movement multiplier to apply
		[Tooltip("要应用的新移动倍增器")]
		public float MovementSpeedMultiplier = 0.5f;

		protected List<MovementZoneCollidingEntity> CollidingEntities;

        /// <summary>
        /// 在初始化时，我们初始化我们的列表
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			CollidingEntities = new List<MovementZoneCollidingEntity>();
		}

        /// <summary>
        /// 当按钮被按下时，我们开始修改时间缩放
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			ChangeSpeed();
		}

        /// <summary>
        /// 改变进入该区域的角色的CharacterMovement的速度
        /// </summary>
        public virtual void ChangeSpeed()
		{
			if (_characterButtonActivation == null)
			{
				return;
			}

			MovementZoneCollidingEntity collidingEntity = new MovementZoneCollidingEntity();
			collidingEntity.TargetCharacter = _characterButtonActivation.gameObject.GetComponentInParent<Character>();

			foreach (MovementZoneCollidingEntity entity in CollidingEntities)
			{
				if (entity.TargetCharacter.gameObject == collidingEntity.TargetCharacter.gameObject)
				{
					return;
				}
			}

			collidingEntity.TargetCharacterMovement = _characterButtonActivation.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterMovement>();
			CollidingEntities.Add(collidingEntity);
			collidingEntity.TargetCharacterMovement.SetContextSpeedMultiplier(MovementSpeedMultiplier);
		}

        /// <summary>
        /// 当离开时，如果需要，我们重置角色的速度并将其从我们的列表中移除
        /// </summary>
        /// <param name="collider"></param>
        public override void TriggerExitAction(GameObject collider)
		{
			foreach (MovementZoneCollidingEntity collidingEntity in CollidingEntities)
			{
				if (collidingEntity.TargetCharacter.gameObject == collider)
				{
					collidingEntity.TargetCharacterMovement.ResetContextSpeedMultiplier();
					CollidingEntities.Remove(collidingEntity);
					break;
				}
			}
		}
	}
}