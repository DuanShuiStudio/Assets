using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 在考拉演示中使用的一个类，用于处理地牢门
    /// </summary>
    [ExecuteInEditMode]
	public class DungeonDoor : TopDownMonoBehaviour
	{
        /// 门的可能状态
        public enum DoorStates { Open, Closed }

		[Header("demo-Bindings绑定")]
		/// the top part of the door
		[Tooltip("demo-门的顶部")]
		public GameObject OpenDoorTop;
		/// the bottom part of the door
		[Tooltip("demo-门的底部")]
		public GameObject OpenDoorBottom;
		/// the object to show when the door is closed
		[Tooltip("demo-当门关闭时要显示的对象")]
		public GameObject ClosedDoor;

		[Header("demo-State状态")]
		/// the current state of the door
		[Tooltip("demo-门的当前状态")]
		public DoorStates DoorState = DoorStates.Open;

        /// 一个用于切换门开或关的测试按钮
        [MMInspectorButton("ToggleDoor")]
		public bool ToogleDoorButton;
        /// 一个用于打开门的测试按钮
        [MMInspectorButton("OpenDoor")]
		public bool OpenDoorButton;
        /// 一个用于关闭门的测试按钮
        [MMInspectorButton("CloseDoor")]
		public bool CloseDoorButton;

        /// <summary>
        /// 在开始时，我们根据门的初始状态对其进行初始化
        /// </summary>
        protected virtual void Start()
		{
			if (DoorState == DoorStates.Open)
			{
				SetDoorOpen();                
			}
			else
			{
				SetDoorClosed();
			}
		}

		/// <summary>
		/// 打开门
		/// </summary>
		public virtual void OpenDoor()
		{
			DoorState = DoorStates.Open;
		}

		/// <summary>
		/// 关闭门
		/// </summary>
		public virtual void CloseDoor()
		{
			DoorState = DoorStates.Closed;
		}

        /// <summary>
        /// 根据门的当前状态打开或关闭门
        /// </summary>
        public virtual void ToggleDoor()
		{
			if (DoorState == DoorStates.Open)
			{
				DoorState = DoorStates.Closed;
			}
			else
			{
				DoorState = DoorStates.Open;
			}
		}

        /// <summary>
        /// 在更新时，如果需要的话，我们打开或关闭门
        /// </summary>
        protected virtual void Update()
		{
			if ((OpenDoorBottom == null) || (OpenDoorTop == null) || (ClosedDoor == null))
			{
				return;
			}

			if (DoorState == DoorStates.Open)
			{
				if (!OpenDoorBottom.activeInHierarchy)
				{
					SetDoorOpen();
				}                
			}
			else
			{
				if (!ClosedDoor.activeInHierarchy)
				{
					SetDoorClosed();
				}
			}
		}

        /// <summary>
        /// 关闭门
        /// </summary>
        protected virtual void SetDoorClosed()
		{
			ClosedDoor.SetActive(true);
			OpenDoorBottom.SetActive(false);
			OpenDoorTop.SetActive(false);
		}

		/// <summary>
		/// 打开门
		/// </summary>
		protected virtual void SetDoorOpen()
		{
			OpenDoorBottom.SetActive(true);
			OpenDoorTop.SetActive(true);
			ClosedDoor.SetActive(false);
		}
	}
}