using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到一个2D盒子碰撞器中，这样你就可以定义一个区域，当进入该区域时，启用一个虚拟相机，让你轻松地在关卡内定义各个部分
    /// </summary>
    public class TopDownCinemachineZone2D : MMCinemachineZone2D
	{
		[Header("Top Down Engine引擎")]
		/// if this is true, the zone will require colliders that want to trigger it to have a Character components of type Player
		[Tooltip("如果这是真的，这个区域将要求希望触发它的碰撞器拥有一个类型为Player的角色组件")]
		public bool RequiresPlayerCharacter = true;
		protected CinemachineCameraController _cinemachineCameraController;
		protected Character _character;

#if MM_CINEMACHINE || MM_CINEMACHINE3
        /// <summary>
        /// 在唤醒时，如果需要的话，添加一个相机控制器
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			if (Application.isPlaying)
			{
				_cinemachineCameraController = VirtualCamera.gameObject.MMGetComponentAroundOrAdd<CinemachineCameraController>();
				_cinemachineCameraController.ConfineCameraToLevelBounds = false;    
			}
		}

        /// <summary>
        /// 启用/禁用相机
        /// </summary>
        /// <param name="state"></param>
        /// <param name="frames"></param>
        /// <returns></returns>
        protected override IEnumerator EnableCamera(bool state, int frames)
		{
			yield return base.EnableCamera(state, frames);
			if (state)
			{
				_cinemachineCameraController.FollowsAPlayer = true;
				_cinemachineCameraController.StartFollowing();
			}
			else
			{
				_cinemachineCameraController.StopFollowing();
				_cinemachineCameraController.FollowsAPlayer = false;
			}
		}

        /// <summary>
        /// 你可以覆盖的额外测试，用于添加额外的碰撞器条件
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected override bool TestCollidingGameObject(GameObject collider)
		{
			if (RequiresPlayerCharacter)
			{
				_character = collider.MMGetComponentNoAlloc<Character>();
				if (_character == null)
				{
					return false;
				}

				if (_character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}
			
			return true;
		}
		#endif
	}    
}