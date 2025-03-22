using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个AIACtion，用于将指定的Transform设置为目标
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Set Transform As Target")]
	public class AIActionSetTransformAsTarget : AIAction
	{
		public Transform TargetTransform;
		public bool OnlyRunOnce = true;
    
		protected bool _alreadyRan = false;

        /// <summary>
        /// 在init中，我们初始化我们的动作
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_alreadyRan = false;
		}

        /// <summary>
        /// 设定一个新目标
        /// </summary>
        public override void PerformAction()
		{
			if (OnlyRunOnce && _alreadyRan)
			{
				return;
			}
			_brain.Target = TargetTransform;
		}

        /// <summary>
        /// 在进入状态时，我们重置我们的标志
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_alreadyRan = false;
		}
	}
}