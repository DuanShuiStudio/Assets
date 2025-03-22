using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.TopDownEngine
{
	public class KillsManager : MMSingleton<KillsManager>, MMEventListener<MMLifeCycleEvent>
	{
		public enum Modes { Layer, List }
		
		public Modes Mode = Modes.Layer;

		[Header("List Mode列表模式")]
		/// a list of Health components on the targets. Once all these targets are dead, OnLastDeath will trigger
		[Tooltip("目标上的生命组件列表。一旦所有这些目标都死亡，OnLastDeath就会触发")]
		public List<Health> TargetsList;
		
		[Header("Layer Mode图层模式")]
		/// the layer(s) on which the dying Health components should be to be counted - typically Enemies
		[Tooltip("应该计算垂死生命组件所在的图层 - 通常是敌人")]
		public LayerMask TargetLayerMask = LayerManager.EnemiesLayerMask;
		/// in Layer mode, if AutoSetKillThreshold is true, the KillThreshold will be automatically computed on start, based on the total number of potential targets in the level matching the target layer mask 
		[Tooltip("在图层模式下，如果AutoSetKillThreshold为true，则在开始时根据与目标图层遮罩匹配的关卡中潜在目标的总数自动计算KillThreshold")]
		public bool AutoSetKillThreshold = true;

		[Header("Counters计数器")]
		/// The maximum amount of kills needed to trigger OnLastDeath
		[Tooltip("触发OnLastDeath所需的最大击杀数")]
		public int DeathThreshold = 5;
		/// The amount of deaths remaining to trigger OnLastDeath. Read only value
		[Tooltip("触发OnLastDeath所需的剩余死亡数。只读值")]
		[MMReadOnly]
		public int RemainingDeaths = 0;
		
		[Header("Events事件")]
		/// An event that gets triggered on every death
		[Tooltip("每次死亡时触发的事件")]
		public UnityEvent OnDeath;
		/// An event that gets triggered when the last death occurs
		[Tooltip("当最后一次死亡发生时触发的事件")]
		public UnityEvent OnLastDeath;

		[Header("Text displays文本显示")] 
		/// An optional text counter displaying the total amount of deaths required before OnLastDeath triggers
		[Tooltip("一个可选的文本计数器，显示在OnLastDeath触发前所需的总死亡数")]
		public Text TotalCounter;
		/// An optional text counter displaying the remaining amount of deaths before OnLastDeath
		[Tooltip("一个可选的文本计数器，显示在OnLastDeath触发前剩余的死亡数")]
		public Text RemainingCounter;
		#if MM_TEXTMESHPRO
		/// An optional text counter displaying the total amount of deaths required before OnLastDeath triggers
		[Tooltip("一个可选的在OnLastDeath触发前显示所需总死亡数的文本计数器")]
		public TMP_Text TotalCounter_TMP;
		/// An optional text counter displaying the remaining amount of deaths before OnLastDeath
		[Tooltip("一个可选的在OnLastDeath触发前显示剩余死亡数的文本计数器")]
		public TMP_Text RemainingCounter_TMP;
#endif

        /// <summary>
        /// 支持进入播放模式的静态初始化
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			_instance = null;
		}

        /// <summary>
        /// 在开始时，我们初始化剩余死亡计数器
        /// </summary>
        protected virtual void Start()
		{
			if (AutoSetKillThreshold)
			{
				ComputeKillThresholdBasedOnTargetLayerMask();
			}
			RefreshRemainingDeaths();
		}

        /// <summary>
        /// 使用此方法更改当前死亡阈值。这也将更新剩余死亡计数器
        /// </summary>
        /// <param name="newThreshold"></param>
        public virtual void RefreshRemainingDeaths()
		{
			if (Mode == Modes.List)
			{
				DeathThreshold = TargetsList.Count;
			}
			
			RemainingDeaths = DeathThreshold;

			UpdateTexts();
		}

        /// <summary>
        /// 基于所选图层遮罩上的对象数量计算所需的死亡阈值
        /// </summary>
        public virtual void ComputeKillThresholdBasedOnTargetLayerMask()
		{
			DeathThreshold = 0;
			Health[] healths = FindObjectsOfType<Health>();
			foreach (Health health in healths)
			{
				if (TargetLayerMask.MMContains(health.gameObject))
				{
					DeathThreshold++;
				}
			}
		}

        /// <summary>
        /// 当我们获得死亡事件时
        /// </summary>
        /// <param name="lifeCycleEvent"></param>
        public virtual void OnMMEvent(MMLifeCycleEvent lifeCycleEvent)
		{
			if (lifeCycleEvent.MMLifeCycleEventType != MMLifeCycleEventTypes.Death)
			{
				return;
			}

            // 我们检查是否仍需要跟踪事件
            if (RemainingDeaths <= 0)
			{
				return;
			}

            // 我们检查图层
            if (!TargetLayerMask.MMContains(lifeCycleEvent.AffectedHealth.gameObject.layer))
			{
				return;
			}

            // 如果在列表模式下，我们确保死亡对象包含在击杀列表中
            if (Mode == Modes.List)
			{
				if (!TargetsList.Contains(lifeCycleEvent.AffectedHealth))
				{
					return;
				}
			}

            // 我们触发OnDeath事件
            OnDeath?.Invoke();
			RemainingDeaths--;

			UpdateTexts();

            // 如果需要，我们触发最后一次死亡事件
            if (RemainingDeaths <= 0)
			{
				OnLastDeath?.Invoke();
			}
		}

        /// <summary>
        /// 如有必要，更新绑定的文本
        /// </summary>
        protected virtual void UpdateTexts()
		{
			if (TotalCounter != null)
			{
				TotalCounter.text = DeathThreshold.ToString();
			}

			if (RemainingCounter != null)
			{
				RemainingCounter.text = RemainingDeaths.ToString();
			}
			
			#if MM_TEXTMESHPRO
				if (TotalCounter_TMP != null)
				{
					TotalCounter_TMP.text = DeathThreshold.ToString();
				}

				if (RemainingCounter_TMP != null)
				{
					RemainingCounter_TMP.text = RemainingDeaths.ToString();
				}
			#endif
		}

        /// <summary>
        /// 在禁用状态下，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMLifeCycleEvent>();
		}

        /// <summary>
        /// 在禁用状态下，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMLifeCycleEvent>();
		}
	}
}
