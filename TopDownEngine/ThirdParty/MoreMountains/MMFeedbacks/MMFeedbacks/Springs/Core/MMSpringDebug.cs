using System;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个类用于在使用MMSpring系统的类中显示调试信息。  
    /// </summary>
    [Serializable]
	public class MMSpringDebug
	{
        /// 弹簧的当前值
        public float CurrentValue;
        /// 弹簧的目标值
        public float TargetValue;

        /// 使用传入参数中的值来更新当前值和目标值。 
        public void Update(float value, float target)
		{
			CurrentValue = value;
			TargetValue = target;
		}
	}	
}

