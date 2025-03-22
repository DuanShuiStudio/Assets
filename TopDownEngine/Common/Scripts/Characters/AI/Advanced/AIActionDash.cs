using System;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 顾名思义，什么也不做的动作。就在那里等着。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Dash")]
	public class AIActionDash : AIAction
	{
        /// 破折号应该出现的方向
        public enum Modes { TowardsTarget, AwayFromTarget, None }

		[Header("Dash冲刺")]
        /// 破折号应该出现的方向
        [Tooltip("冲刺应该出现的方向")]
		public Modes Mode = Modes.TowardsTarget;
		/// whether or not the dash mode should be setup to Script automatically on the dash ability
		[Tooltip("是否应将冲刺模式设置为在冲刺能力上自动通过脚本设置")] 
		public bool AutoSetupDashMode = true;
		
		protected CharacterDash2D _characterDash2D;
		protected CharacterDash3D _characterDash3D;

        /// <summary>
        /// 在初始化时，我们获取猛冲能力，并在需要时自动设置它
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterDash2D = this.gameObject.GetComponentInParent<CharacterDash2D>();
			_characterDash3D = this.gameObject.GetComponentInParent<CharacterDash3D>();
			if (AutoSetupDashMode)
			{
				if (_characterDash2D != null)
				{
					_characterDash2D.DashMode = CharacterDash2D.DashModes.Script;
				}
				if (_characterDash3D != null)
				{
					_characterDash3D.DashMode = CharacterDash3D.DashModes.Script;
				}
			}
		}

        /// <summary>
        /// 在PerformAction中，我们设置了猛冲方向并启动了猛冲
        /// </summary>
        public override void PerformAction()
		{
			if (_characterDash2D != null)
			{
				if (_brain.Target != null)
				{
					switch (Mode)
					{
						case Modes.TowardsTarget:
							_characterDash2D.DashDirection = (_brain.Target.transform.position - this.transform.position).normalized;
							break;
						case Modes.AwayFromTarget:
							_characterDash2D.DashDirection = (this.transform.position - _brain.Target.transform.position).normalized;
							break;
					}	
				}
				_characterDash2D.DashStart();
			}
			else if (_characterDash3D != null)
			{
				if (_brain.Target != null)
				{
					switch (Mode)
					{
						case Modes.TowardsTarget:
							_characterDash3D.DashDirection = (_brain.Target.transform.position - this.transform.position).normalized;
							break;
						case Modes.AwayFromTarget:
							_characterDash3D.DashDirection = (this.transform.position - _brain.Target.transform.position).normalized;
							break;
					}	
				}
				_characterDash3D.DashStart();
			}
		}
	}
}