using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringClampSettings
	{
		[Header("Min��Сֵ")]
		/// whether or not to clamp the min value of this spring, preventing it from going below a certain value
		[Tooltip("�Ƿ�����������ɵ���Сֵ���Է�ֹ�����ĳ���ض���ֵ�� ")]
		public bool ClampMin = false;
		/// the value below which this spring can't go
		[Tooltip("������ɲ��ܵ��ڵ���ֵ�� ")]
		[MMCondition("ClampMin", true)]
		public float ClampMinValue = 0f;
		/// if ClampMin is true, whether or not to use the initial value as the min value
		[Tooltip("�����������Сֵ��ClampMin����Ϊ�棬�Ƿ񽫳�ʼֵ������Сֵ�� ")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinInitial = false;
		/// whether or not the spring should bounce off the min value or not
		[Tooltip("��������Ƿ�Ӧ���ڴﵽ��Сֵʱ������ ")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinBounce = false;
		
		[Header("Max���ֵ")]
		/// whether or not to clamp the max value of this spring, preventing it from going above a certain value
		[Tooltip("�Ƿ�����������ɵ����ֵ����ֹ������ĳ���ض���ֵ�� ")]
		public bool ClampMax = false;
		/// the value above which this spring can't go
		[Tooltip("������ɲ��ܳ�������ֵ��")]
		[MMCondition("ClampMax", true)]
		public float ClampMaxValue = 10f;
		/// if ClampMax is true, whether or not to use the initial value as the max value
		[Tooltip("������������ֵ��ClampMax����Ϊ�棬�Ƿ񽫳�ʼֵ�������ֵ�� ")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxInitial = false;
		/// whether or not the spring should bounce off the max value or not
		[Tooltip("��������Ƿ�Ӧ���ڴﵽ���ֵʱ������ ")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxBounce = false;

		public bool ClampNeeded => ClampMin || ClampMax || ClampMinBounce || ClampMaxBounce;

		public virtual float GetTargetValue(float value, float initialValue)
		{
			float targetValue = value;
			float clampMinValue = ClampMinInitial ? initialValue : ClampMinValue;
			if (ClampMin && value < clampMinValue)
			{
				targetValue = clampMinValue;
			}
			float clampMaxValue = ClampMaxInitial ? initialValue : ClampMaxValue;
			if (ClampMax && value > clampMaxValue)
			{
				targetValue = clampMaxValue;
			}
			return targetValue;
		}
	}
}

