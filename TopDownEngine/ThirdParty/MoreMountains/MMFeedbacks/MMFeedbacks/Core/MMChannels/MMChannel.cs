using System;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 用于识别通道的可能模式，无论是通过整数还是MMChannel可脚本化对象。
    /// </summary>
    public enum MMChannelModes
	{
		Int,
		MMChannel
	}

    /// <summary>
    /// 一种用于传递通道信息的数据结构。
    /// </summary>
    [Serializable]
	public class MMChannelData
	{
		public MMChannelModes MMChannelMode;
		public int Channel;
		public MMChannel MMChannelDefinition;

		public MMChannelData(MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			MMChannelMode = mode;
			Channel = channel;
			MMChannelDefinition = channelDefinition;
		}
	}

    /// <summary>
    /// MMChannelData的扩展类。
    /// </summary>
    public static class MMChannelDataExtensions
	{
		public static MMChannelData Set(this MMChannelData data, MMChannelModes mode, int channel, MMChannel channelDefinition)
		{
			data.MMChannelMode = mode;
			data.Channel = channel;
			data.MMChannelDefinition = channelDefinition;
			return data;
		}
	}

    /// <summary>
    ///
    /// 一种可以从中创建资源的资产脚本化对象，主要用于（但不限于）反馈和震动器中，以确定通信通道，通常介于发射器和接收器之间
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/MMChannel", fileName = "MMChannel")]
	public class MMChannel : ScriptableObject
	{
		public static bool Match(MMChannelData dataA, MMChannelData dataB)
		{
			if (dataA.MMChannelMode != dataB.MMChannelMode)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == dataB.Channel;
			}
			else
			{
				return dataA.MMChannelDefinition == dataB.MMChannelDefinition;
			}
		}
		public static bool Match(MMChannelData dataA, MMChannelModes modeB, int channelB, MMChannel channelDefinitionB)
		{
			if (dataA == null)
			{
				return true;
			}
			
			if (dataA.MMChannelMode != modeB)
			{
				return false;
			}

			if (dataA.MMChannelMode == MMChannelModes.Int)
			{
				return dataA.Channel == channelB;
			}
			else
			{
				return dataA.MMChannelDefinition == channelDefinitionB;
			}
		}
	}
}