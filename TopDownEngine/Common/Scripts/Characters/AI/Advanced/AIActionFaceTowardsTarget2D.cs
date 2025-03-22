using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个AI动作将让一个具有CharacterOrientation2D能力的代理面对它的目标
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Face Towards Target 2D")]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	public class AIActionFaceTowardsTarget2D : AIAction
	{
        /// 你可以让AI面对的可能模式（应该-通常-匹配你的CharacterOrientation2D设置）
        public enum Modes { LeftRight, FacingDirections }

		[Header("Face Towards Target 面向目标")] 
		/// the selected facing mode
		public Modes Mode = Modes.LeftRight;
        
		protected CharacterOrientation2D _characterOrientation2D;
		protected Vector3 _targetPosition;
		protected Vector2 _distance;
		protected bool _chacterOrientation2DNotNull;
		protected Character.FacingDirections _newFacingDirection;
		protected CharacterOrientation2D.FacingModes _initialFacingMode;

        /// <summary>
        /// 在init中，我们获取CharacterOrientation2D能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterOrientation2D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterOrientation2D>();
			if (_characterOrientation2D != null)
			{
				_chacterOrientation2DNotNull = true;
				_initialFacingMode = _characterOrientation2D.FacingMode;    
			}
		}
		
		public override void OnEnterState()
		{
			_characterOrientation2D.FacingMode = CharacterOrientation2D.FacingModes.None;
		}

        /// <summary>
        /// 描述当大脑退出该动作所处的状态时发生的情况。意味着被覆盖。
        /// </summary>
        public override void OnExitState()
		{
			_characterOrientation2D.FacingMode = _initialFacingMode;
		}

        /// <summary>
        /// 在performance action上，我们面对我们的目标
        /// </summary>
        public override void PerformAction()
		{
			FaceTarget();
		}

        /// <summary>
        /// 使定向二维能力面向大脑目标
        /// </summary>
        protected virtual void FaceTarget()
		{
			if ((_brain.Target == null) || !_chacterOrientation2DNotNull)
			{
				return;
			}
			_targetPosition = _brain.Target.transform.position;

			if (Mode == Modes.LeftRight)
			{
				int facingDirection = (_targetPosition.x < this.transform.position.x) ? -1 : 1;
				_characterOrientation2D.FaceDirection(facingDirection);    
			}
			else
			{
				_distance = _targetPosition - this.transform.position;
				if (Mathf.Abs(_distance.y) > Mathf.Abs(_distance.x))
				{
					_newFacingDirection = (_distance.y > 0) ? Character.FacingDirections.North : Character.FacingDirections.South;
				}
				else
				{
					_newFacingDirection = (_distance.x > 0) ? Character.FacingDirections.East : Character.FacingDirections.West;
				}
				_characterOrientation2D.Face(_newFacingDirection);
			}
		}
	}
}