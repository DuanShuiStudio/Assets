using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 你可以使用不同的状态来检查你的角色在当前帧是否正在做某事
    /// </summary>    
    public class CharacterStates 
	{
		/// The possible character conditions
		public enum CharacterConditions
		{
			Normal,
			ControlledMovement,
			Frozen,
			Paused,
			Dead,
			Stunned
		}

        /// 角色可能处于的运动状态。这些通常对应于它们自己的类，
        /// 但这不是强制性的
        public enum MovementStates 
		{
			Null,//无
			Idle,//空闲
			Falling,//下降
			Walking,//行走
			Running,//奔跑
			Crouching,//蹲下
			Crawling, //爬行
			Dashing,//冲刺
			Jetpacking,//喷气背包
			Jumping,//跳跃
			Pushing,//推动
			DoubleJumping,//连跳
			Attacking,//攻击
			FallingDownHole//落入洞穴
		}
	}
}