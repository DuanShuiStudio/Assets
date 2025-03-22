using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 当到达检查点时要触发的事件
    /// </summary>
    public struct CheckPointEvent
	{
		public int Order;
		public CheckPointEvent(int order)
		{
			Order = order;
		}

		static CheckPointEvent e;
		public static void Trigger(int order)
		{
			e.Order = order;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 检查点类。如果玩家死亡，将使玩家在这个点复活
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Checkpoint")]
	public class CheckPoint : TopDownMonoBehaviour 
	{
		[Header("Spawn生成")]
		[MMInformation("将此脚本添加到一个（最好是空的）GameObject上，它将被添加到关卡的检查点列表中，使你可以从那里复活。如果你将它绑定到LevelManager的起点，那么角色将在关卡开始时从那里生成。在这里你可以选择角色是面向左边还是右边生成。", MMInformationAttribute.InformationType.Info,false)]
		/// the facing direction the character should face when spawning from this checkpoint
		[Tooltip("从这个检查点复活时角色应该面向的方向")]
		public Character.FacingDirections FacingDirection = Character.FacingDirections.East ;
		/// whether or not this checkpoint should override any order and assign itself on entry
		[Tooltip("这个检查点是否应该优先于任何指令，并在进入时自行分配任务")]
		public bool ForceAssignation = false;
		/// the order of the checkpoint
		[Tooltip("检查点的顺序")]
		public int CheckPointOrder;
        
		protected List<Respawnable> _listeners;

        /// <summary>
        /// 初始化监听器列表
        /// </summary>
        protected virtual void Awake () 
		{
			_listeners = new List<Respawnable>();
		}

        /// <summary>
        /// 在检查点生成玩家
        /// </summary>
        /// <param name="player">Player.</param>
        public virtual void SpawnPlayer(Character player)
		{
			player.RespawnAt(transform, FacingDirection);
			
			foreach(Respawnable listener in _listeners)
			{
				listener.OnPlayerRespawn(this,player);
			}
		}

        /// <summary>
        /// 将Respawnable分配给这个检查点
        /// </summary>
        /// <param name="listener"></param>
        public virtual void AssignObjectToCheckPoint (Respawnable listener) 
		{
			_listeners.Add(listener);
		}

        /// <summary>
        /// 描述当有东西进入检查点时会发生什么
        /// </summary>
        /// <param name="collider">与水碰撞的物体</param>
        protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			TriggerEnter(collider.gameObject);            
		}

		protected virtual void OnTriggerEnter(Collider collider)
		{
			TriggerEnter(collider.gameObject);
		}

		protected virtual void TriggerEnter(GameObject collider)
		{
			Character character = collider.GetComponent<Character>();

			if (character == null) { return; }
			if (character.CharacterType != Character.CharacterTypes.Player) { return; }
			if (!LevelManager.HasInstance) { return; }
			LevelManager.Instance.SetCurrentCheckpoint(this);
			CheckPointEvent.Trigger(CheckPointOrder);
		}

        /// <summary>
        /// 在DrawGizmos上，我们绘制线条以显示对象将遵循的路径
        /// </summary>
        protected virtual void OnDrawGizmos()
		{	
			#if UNITY_EDITOR

			if (!LevelManager.HasInstance)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints == null)
			{
				return;
			}

			if (LevelManager.Instance.Checkpoints.Count == 0)
			{
				return;
			}

			for (int i=0; i < LevelManager.Instance.Checkpoints.Count; i++)
			{
                // 我们绘制一条朝向路径中下一个点的线
                if ((i+1) < LevelManager.Instance.Checkpoints.Count)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(LevelManager.Instance.Checkpoints[i].transform.position,LevelManager.Instance.Checkpoints[i+1].transform.position);
				}
			}
			#endif
		}
	}
}