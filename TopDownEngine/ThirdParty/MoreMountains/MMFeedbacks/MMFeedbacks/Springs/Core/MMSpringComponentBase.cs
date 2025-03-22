using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// һ�������࣬���ڹ�����������Բٿظ������ԣ�����������ά��������ά��������ɫ�ȵȣ�
    /// </summary>
    [MMRequiresConstantRepaintOnlyWhenPlaying]
	public abstract class MMSpringComponentBase : MMMonoBehaviour
	{
        /// ���ɲ�ͬ�Ŀ���ʱ��߶�ģʽ�� 
        public enum TimeScaleModes { Unscaled, Scaled }
        /// ��������Ƿ��Ѵﵽ�㹻�͵��ٶȴӶ�����ֹͣ������ 
        public virtual bool LowVelocity => false;

		[MMInspectorGroup("Events", true, 16, true)] 
		public UnityEvent OnEquilibriumReached;
		
		protected float _velocityLowThreshold = 0.001f;

        /// <summary>
        /// ����һ����ֵ�������ɵ��ٶȵ��ڸ���ֵʱ�����ɻ���Ϊ���ٶȹ��ͣ���������ֹͣ������ 
        /// </summary>
        /// <param name="threshold"></param>
        public virtual void SetVelocityLowThreshold(float threshold)
		{
			_velocityLowThreshold = threshold;
		}

        /// <summary>
        /// �ڻ���ʱ����������ֹͣ������  
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
			this.enabled = false;
		}

        /// <summary>
        /// ��ÿ�θ���ʱ�����ǻ���µ��ɵ�ֵ������������Ҫ�����������ֹͣ������  
        /// </summary>
        protected virtual void Update()
		{
			UpdateSpringValue();
			SelfDisable();
		}

        /// <summary>
        /// ��������
        /// </summary>
        protected virtual void Activate()
		{
			this.enabled = true;
		}

        /// <summary>
        /// ���ô����
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
        /// ֹͣ�˵����ϵ�������ֵ�仯���ƶ����� 
        /// </summary>
        public virtual void Stop() { }

        /// <summary>
        /// ���˵����ƶ�����Ŀ��λ�ã�Ȼ��������� 
        /// </summary>
        public virtual void Finish() { }

        /// <summary>
        /// �ָ�������ɵĳ�ʼֵ��
        /// </summary>
        public virtual void RestoreInitialValue() { }

        /// <summary>
        /// ���˵��ɵĵ�ǰֵ����Ϊ���µĳ�ʼֵ��������ǰ�ĳ�ʼֵ�� 
        /// </summary>
        public virtual void ResetInitialValue() { }

        /// <summary>
        /// �Դ˵���ִ�г�ʼ��������
        /// </summary>
        protected virtual void Initialization() { }

        /// <summary>
        /// ��ȡ����Ŀ��ĵ�ǰֵ��
        /// </summary>
        protected virtual void GrabCurrentValue() { }

        /// <summary>
        /// ���µ��ɵ�Ŀ��ֵ��
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
