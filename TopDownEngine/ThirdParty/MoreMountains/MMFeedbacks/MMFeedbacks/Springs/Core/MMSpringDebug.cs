using System;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// �����������ʹ��MMSpringϵͳ��������ʾ������Ϣ��  
    /// </summary>
    [Serializable]
	public class MMSpringDebug
	{
        /// ���ɵĵ�ǰֵ
        public float CurrentValue;
        /// ���ɵ�Ŀ��ֵ
        public float TargetValue;

        /// ʹ�ô�������е�ֵ�����µ�ǰֵ��Ŀ��ֵ�� 
        public void Update(float value, float target)
		{
			CurrentValue = value;
			TargetValue = target;
		}
	}	
}

