using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个抽象类，用于构建弹簧组件以操控各种属性（浮点数、二维向量、三维向量、颜色等等）
    /// </summary>
    [MMRequiresConstantRepaintOnlyWhenPlaying]
	public abstract class MMSpringComponentBase : MMMonoBehaviour
	{
        /// 弹簧不同的可能时间尺度模式。 
        public enum TimeScaleModes { Unscaled, Scaled }
        /// 这个弹簧是否已达到足够低的速度从而自行停止工作。 
        public virtual bool LowVelocity => false;

		[MMInspectorGroup("Events", true, 16, true)] 
		public UnityEvent OnEquilibriumReached;
		
		protected float _velocityLowThreshold = 0.001f;

        /// <summary>
        /// 设置一个阈值，当弹簧的速度低于该阈值时，弹簧会认为其速度过低，并将自行停止工作。 
        /// </summary>
        /// <param name="threshold"></param>
        public virtual void SetVelocityLowThreshold(float threshold)
		{
			_velocityLowThreshold = threshold;
		}

        /// <summary>
        /// 在唤醒时，我们自行停止工作。  
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
			this.enabled = false;
		}

        /// <summary>
        /// 在每次更新时，我们会更新弹簧的值，并且在有需要的情况下自行停止工作。  
        /// </summary>
        protected virtual void Update()
		{
			UpdateSpringValue();
			SelfDisable();
		}

        /// <summary>
        /// 激活此组件
        /// </summary>
        protected virtual void Activate()
		{
			this.enabled = true;
		}

        /// <summary>
        /// 禁用此组件
        /// </summary>
        protected virtual void SelfDisable()
		{
			if (LowVelocity)
			{
				if (OnEquilibriumReached != null)
				{
					OnEquilibriumReached.Invoke();
				}
				Finish();
				this.enabled = false;
			}
		}

        /// <summary>
        /// 停止此弹簧上的所有数值变化（移动）。 
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        /// 将此弹簧移动到其目标位置，然后禁用它。 
        /// </summary>
        public virtual void Finish() { }

        /// <summary>
        /// 恢复这个弹簧的初始值。
        /// </summary>
        public virtual void RestoreInitialValue() { }

        /// <summary>
        /// 将此弹簧的当前值设置为其新的初始值，覆盖先前的初始值。 
        /// </summary>
        public virtual void ResetInitialValue() { }

        /// <summary>
        /// 对此弹簧执行初始化操作。
        /// </summary>
        protected virtual void Initialization() { }

        /// <summary>
        /// 获取弹簧目标的当前值。
        /// </summary>
        protected virtual void GrabCurrentValue() { }

        /// <summary>
        /// 更新弹簧的目标值。
        /// </summary>
        protected virtual void UpdateSpringValue() { }


		#region TEST_METHODS

		protected virtual void TestMoveTo() { }
		protected virtual void TestMoveToAdditive() { }
		protected virtual void TestMoveToSubtractive() { }
		protected virtual void TestMoveToRandom() { }
		protected virtual void TestMoveToInstant() { }
		protected virtual void TestBump() { }
		protected virtual void TestBumpRandom() { }

		#endregion
		
	}
}
