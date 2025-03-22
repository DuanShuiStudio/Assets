using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于收集目标获取设置的类
    /// </summary>
    [System.Serializable]
	public class MMFeedbackTargetAcquisition
	{
		public enum Modes { None, Self, AnyChild, ChildAtIndex, Parent, FirstReferenceHolder, PreviousReferenceHolder, ClosestReferenceHolder, NextReferenceHolder, LastReferenceHolder }
		
		/// the selected mode for target acquisition
		/// None : nothing will happen
		/// Self : the target will be picked on the MMF Player's game object
		/// AnyChild : the target will be picked on any of the MMF Player's child objects
		/// ChildAtIndex : the target will be picked on the child at index X of the MMF Player
		/// Parent : the target will be picked on the first parent where a matching target is found
		/// Various reference holders : the target will be picked on the specified reference holder in the list (either the first one, previous : first one found before this feedback in the list, closest in any direction from this feedback, the next one found, or the last one in the list)   
		[Tooltip("目标获取的选定模式\n" +
            "None : 什么都不会发生\n" +
            "Self : 目标将在 MMF 播放器（MMF Player）的游戏对象上被选取\n" +
            "AnyChild : 目标将从 MMF 播放器的任意子对象中选取\n" +
            "ChildAtIndex : 目标将从 MMF 播放器的索引为 X 的子对象中选取\n" +
            "Parent : 目标将在找到匹配目标的第一个父级对象上被选取。 \n" +
            "Various reference holders：目标将从列表中指定的引用持有者上选取。" +
            "可以是第一个；之前的 —— 在列表中此反馈之前找到的第一个；从该反馈在任意方向上最近的；下一个找到的；或者是列表中的最后一个）")]
		public Modes Mode = Modes.None;

		[MMFEnumCondition("Mode", (int)Modes.ChildAtIndex)]
		public int ChildIndex = 0;

		private static MMF_ReferenceHolder _referenceHolder;

		public static MMF_ReferenceHolder GetReferenceHolder(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			switch (settings.Mode)
			{
				case Modes.FirstReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.First, currentFeedbackIndex);
				case Modes.PreviousReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Previous, currentFeedbackIndex);
				case Modes.ClosestReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Closest, currentFeedbackIndex);
				case Modes.NextReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Next, currentFeedbackIndex);
				case Modes.LastReferenceHolder:
					return owner.GetFeedbackOfType<MMF_ReferenceHolder>(MMF_Player.AccessMethods.Last, currentFeedbackIndex);
			}
			return null;
		}

		public static GameObject FindAutomatedTargetGameObject(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			if (owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder != null)
			{
				return owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder.GameObjectReference;
			}
			
			_referenceHolder = GetReferenceHolder(settings, owner, currentFeedbackIndex);
			switch (settings.Mode)
			{
				case Modes.Self:
					return owner.gameObject;
				case Modes.ChildAtIndex:
					return owner.transform.GetChild(settings.ChildIndex).gameObject;
				case Modes.AnyChild:
					return owner.transform.GetChild(0).gameObject;
				case Modes.Parent:
					return owner.transform.parent.gameObject;
				case Modes.FirstReferenceHolder: 
				case Modes.PreviousReferenceHolder:
				case Modes.ClosestReferenceHolder:
				case Modes.NextReferenceHolder:
				case Modes.LastReferenceHolder:
					return _referenceHolder?.GameObjectReference;
			}
			return null;
		}

		public static T FindAutomatedTarget<T>(MMFeedbackTargetAcquisition settings, MMF_Player owner, int currentFeedbackIndex)
		{
			if (owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder != null)
			{
				return owner.FeedbacksList[currentFeedbackIndex].ForcedReferenceHolder.GameObjectReference.GetComponent<T>();
			}
			_referenceHolder = GetReferenceHolder(settings, owner, currentFeedbackIndex);
			switch (settings.Mode)
			{
				case Modes.Self:
					return owner.GetComponent<T>();
				case Modes.ChildAtIndex:
					return owner.transform.GetChild(settings.ChildIndex).gameObject.GetComponent<T>();
				case Modes.AnyChild:
					for (int i = 0; i < owner.transform.childCount; i++) 
					{
						if (owner.transform.GetChild(i).GetComponent<T>() != null)
						{
							return owner.transform.GetChild(i).GetComponent<T>();
						}
					}
					return owner.GetComponentInChildren<T>();
				case Modes.Parent:
					return owner.transform.parent.GetComponentInParent<T>();
				case Modes.FirstReferenceHolder: 
				case Modes.PreviousReferenceHolder:
				case Modes.ClosestReferenceHolder:
				case Modes.NextReferenceHolder:
				case Modes.LastReferenceHolder:
					return (_referenceHolder != null)
						? _referenceHolder.GameObjectReference.GetComponent<T>()
						: default(T);
			}
			return default(T);
		}
		
		
		
	}
}