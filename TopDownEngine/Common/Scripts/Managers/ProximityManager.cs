using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个类，用于查找带有ProximityManaged类的对象，并根据它们的设置启用或禁用它们
    ///这个类旨在展示如何处理包含大量对象的大场景，通过禁用远处的对象来优化性能 
    /// 远离动作以节省性能
    /// 请注意，有很多方法可以做到这点，这个例子简单且通用，对于您的特定使用情况，可能有更好的选择
    /// </summary>
    public class ProximityManager : MMSingleton<ProximityManager>, MMEventListener<TopDownEngineEvent>
    {
        [Header("Target目标")]

        /// whether or not to automatically grab the player from the LevelManager once the scene loads
        [Tooltip("是否在场景加载后自动从关卡管理器中获取玩家")]
        public bool AutomaticallySetPlayerAsTarget = true;
        /// the target to detect proximity with
        [Tooltip("用于检测近距离的目标")]
        public Transform ProximityTarget;
        /// in this mode, if there's no ProximityTarget, proximity managed objects will be disabled  
        [Tooltip("在这种模式下，如果没有ProximityTarget，将禁用近距离管理的对象")]
        public bool RequireProximityTarget = true;

        [Header("EnableDisable启用/禁用")]

        /// whether or not to automatically grab all ProximityManaged objects in the scene
        [Tooltip("是否自动获取场景中所有的ProximityManaged对象")]
        public bool AutomaticallyGrabControlledObjects = true;
        /// the list of objects to check proximity with
        [Tooltip("要检查的对象列表")]
        public List<ProximityManaged> ControlledObjects;
        
        [Header("Tick时钟")]

        /// the frequency, in seconds, at which to evaluate distances and enable/disable stuff
        [Tooltip("评估距离并启用/禁用某些功能的频率（以秒为单位）")]
        public float EvaluationFrequency = 0.5f;

        protected float _lastEvaluationAt = 0f;

        /// <summary>
        /// 支持进入播放模式的静态初始化
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics()
        {
            _instance = null;
        }

        /// <summary>
        /// 在开始的时候，我们获取我们控制的对象
        /// </summary>
        protected virtual void Start()
        {
            GrabControlledObjects();
        }

        /// <summary>
        /// 抓取场景中所有受 proximity（邻近）管理的对象
        /// </summary>
        protected virtual void GrabControlledObjects()
        {
            if (AutomaticallyGrabControlledObjects)
            {
                var items = FindObjectsOfType<ProximityManaged>();
                foreach(ProximityManaged managed in items)
                {
                    managed.Manager = this;
                    ControlledObjects.Add(managed);
                }
            }
        }

        /// <summary>
        /// 一个在运行时用于添加新受控对象的公共方法
        /// </summary>
        /// <param name="newObject"></param>
        public virtual void AddControlledObject(ProximityManaged newObject)
        {
            ControlledObjects.Add(newObject);
        }

        /// <summary>
        /// 从关卡管理器中获取玩家
        /// </summary>
        protected virtual void SetPlayerAsTarget()
        {
            if (AutomaticallySetPlayerAsTarget)
            {
                ProximityTarget = LevelManager.Instance.Players[0].transform;
                _lastEvaluationAt = 0f;
            }            
        }

        /// <summary>
        /// 在更新（On Update）时，我们检查距离
        /// </summary>
        protected virtual void Update()
        {
            EvaluateDistance();
        }

        /// <summary>
        /// 如果需要就检查距离
        /// </summary>
        protected virtual void EvaluateDistance()
        {
            if (ProximityTarget == null)
            {
                if (RequireProximityTarget)
                {
                    foreach (ProximityManaged proxy in ControlledObjects)
                    {
                        if (proxy.gameObject.activeInHierarchy)
                        {
                            proxy.gameObject.SetActive(false);
                            proxy.DisabledByManager = true;
                        }
                    }
                }
                return;
            }
            
            if (Time.time - _lastEvaluationAt > EvaluationFrequency)
            {
                _lastEvaluationAt = Time.time;
            }
            else
            {
                return;
            }
            foreach(ProximityManaged proxy in ControlledObjects)
            {
                float distance = Vector3.Distance(proxy.transform.position, ProximityTarget.position);
                if (proxy.gameObject.activeInHierarchy && (distance > proxy.DisableDistance))
                {
                    proxy.gameObject.SetActive(false);
                    proxy.DisabledByManager = true;
                }
                if (!proxy.gameObject.activeInHierarchy && proxy.DisabledByManager && (distance < proxy.EnableDistance))
                {
                    proxy.gameObject.SetActive(true);
                    proxy.DisabledByManager = false;
                }
            }
        }

        /// <summary>
        /// 当我们收到关卡开始事件时，我们指定玩家作为目标
        /// </summary>
        /// <param name="engineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
        {
            if ((engineEvent.EventType == TopDownEngineEventTypes.SpawnComplete)
                || (engineEvent.EventType == TopDownEngineEventTypes.CharacterSwap))
            {
                SetPlayerAsTarget();
            }
        }

        /// <summary>
        /// 在启用时，我们开始监听事件。
        /// </summary>
        protected virtual void OnEnable()
        {
            this.MMEventStartListening<TopDownEngineEvent>();
        }

        /// <summary>
        /// 在禁用时，我们停止监听事件。
        /// </summary>
        protected virtual void OnDisable()
        {
            this.MMEventStopListening<TopDownEngineEvent>();
        }
    }
}