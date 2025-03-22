using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果没有通往当前大脑目标的路径，这个决定将返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Pathfinder Path To Target Exists")]
	public class AIDecisionPathfinderPathToTargetExists : AIDecision
	{
		protected CharacterPathfinder3D _characterPathfinder3D;

        /// <summary>
        /// 在init中，我们获取探路者能力
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_characterPathfinder3D = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterPathfinder3D>();
		}

        /// <summary>
        /// 我们在决定时返回true
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			bool pathIsComplete = _characterPathfinder3D.PathExists(this.transform.position, _brain.Target.position);
			
			return pathIsComplete;
		}
	}
}