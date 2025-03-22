using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于跟踪层名称的简单静态类，它保存了大多数常用层和层掩码组合的现成可用层掩码
    /// 当然，如果您碰巧更改了层的顺序或编号，您会希望更新此类
    /// </summary>
    public static class LayerManager
	{
		private static int ObstaclesLayer = 8;
		private static int GroundLayer = 9;
		private static int PlayerLayer = 10;
		private static int EnemiesLayer = 13;
		private static int HoleLayer = 15;
		private static int MovingPlatformLayer = 16;
		private static int FallingPlatformLayer = 17;
		private static int ProjectileLayer = 18;
        
		public static int ObstaclesLayerMask = 1 << ObstaclesLayer;
		public static int GroundLayerMask = 1 << GroundLayer;
		public static int PlayerLayerMask = 1 << PlayerLayer;
		public static int EnemiesLayerMask = 1 << EnemiesLayer;
		public static int HoleLayerMask = 1 << HoleLayer;
		public static int MovingPlatformLayerMask = 1 << MovingPlatformLayer;
		public static int FallingPlatformLayerMask = 1 << FallingPlatformLayer;
		public static int ProjectileLayerMask = 1 << ProjectileLayer;
	}
}