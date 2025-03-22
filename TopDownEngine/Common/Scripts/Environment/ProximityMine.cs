using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到碰撞器（2D或3D）中，它将允许您在持续时间后触发事件，就像地雷一样
    /// 它还提供了在退出时中断或重置计时器的选项
    /// </summary>
    public class ProximityMine : TopDownMonoBehaviour
	{
		[Header("Proximity Mine触碰式延时地雷")]
		/// the layers that will trigger this mine
		[Tooltip("触发这种地雷的层")]
		public LayerMask TargetLayerMask;
		/// whether or not to disable the mine when it triggers/explodes
		[Tooltip("是否在地雷触发/爆炸时将其禁用")]
		public bool DisableMineOnTrigger = true;

		[Header("WarningDuration警告持续时间")] 
		/// the duration of the warning phase, in seconds, betfore the mine triggers
		[Tooltip("在地雷触发之前，警告阶段的持续时间（以秒为单位）")]
		public float WarningDuration = 2f;
		/// whether or not the warning should stop when exiting the zone
		[Tooltip("是否应在离开区域时停止警告")]
		public bool WarningStopsOnExit = false;
		/// whether or not the warning duration should reset when exiting the zone
		[Tooltip("是否应在离开区域时重置警告持续时间")]
		public bool WarningDurationResetsOnExit = false;

		/// a read only display of the current duration before explosion
		[Tooltip("爆炸前当前持续时间的只读显示")]
		[MMReadOnly] 
		public float TimeLeftBeforeTrigger;
        
		[Header("Feedbacks反馈")]
		/// the feedback to play when the warning phase starts
		[Tooltip("警告阶段开始时要播放的反馈")]
		public MMFeedbacks OnWarningStartsFeedbacks;
		/// a feedback to play when the warning phase stops
		[Tooltip("警告阶段停止时要播放的反馈")] 
		public MMFeedbacks OnWarningStopsFeedbacks;
		/// a feedback to play when the warning phase is reset
		[Tooltip("警告阶段重置时要播放的反馈")] 
		public MMFeedbacks OnWarningResetFeedbacks;
		/// a feedback to play when the mine triggers
		[Tooltip("地雷触发时要播放的反馈")]
		public MMFeedbacks OnMineTriggerFeedbacks;
        
		protected bool _inside = false;

        /// <summary>
        /// 开始时，我们初始化地雷
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，我们初始化反馈和持续时间
        /// </summary>
        public virtual void Initialization()
		{
			OnWarningStartsFeedbacks?.Initialization();
			OnWarningStopsFeedbacks?.Initialization();
			OnWarningResetFeedbacks?.Initialization();
			OnMineTriggerFeedbacks?.Initialization();
			TimeLeftBeforeTrigger = WarningDuration;
		}

        /// <summary>
        /// 发生碰撞时，如果需要，我们开始计时器
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			_inside = true;
            
			OnWarningStartsFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 退出时，如果需要，我们停止计时器
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Exiting(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return;
			}

			if (!WarningStopsOnExit)
			{
				return;
			}
            
			OnWarningStopsFeedbacks?.PlayFeedbacks();

			if (WarningDurationResetsOnExit)
			{
				OnWarningResetFeedbacks?.PlayFeedbacks();
				TimeLeftBeforeTrigger = WarningDuration;
			}
            
			_inside = false;
		}

        /// <summary>
        /// 描述地雷爆炸时会发生什么
        /// </summary>
        public virtual void TriggerMine()
		{
			OnMineTriggerFeedbacks?.PlayFeedbacks();
            
			if (DisableMineOnTrigger)
			{
				this.gameObject.SetActive(false);
			}
		}

        /// <summary>
        /// 在更新时，如果目标在区域内，我们更新计时器。
        /// </summary>
        protected virtual void Update()
		{
			if (_inside)
			{
				TimeLeftBeforeTrigger -= Time.deltaTime;
			}

			if (TimeLeftBeforeTrigger <= 0)
			{
				TriggerMine();
			}
		}

        /// <summary>
        /// 当与玩家发生碰撞时，我们会对玩家造成伤害并将其击倒
        /// </summary>
        /// <param name="collider">what's colliding with the object.</param>
        public virtual void OnTriggerStay2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发进入2D时，我们调用碰撞结束点
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发停留时，我们调用碰撞结束点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerStay(Collider collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发进入时，我们调用碰撞结束点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerEnter(Collider collider)
		{
			Colliding(collider.gameObject);
		}

        /// <summary>
        /// 在触发退出时，我们调用碰撞结束点
        /// </summary>
        /// <param name="collider"></param>
        public virtual void OnTriggerExit(Collider collider)
		{
			Exiting(collider.gameObject);
		}

        /// <summary>
        /// 在触发退出2D时，我们调用碰撞结束点。
        /// </summary>
        /// <param name="collider"></param>S
        public virtual void OnTriggerExit2D(Collider2D collider)
		{
			Exiting(collider.gameObject);
		}
	}    
}