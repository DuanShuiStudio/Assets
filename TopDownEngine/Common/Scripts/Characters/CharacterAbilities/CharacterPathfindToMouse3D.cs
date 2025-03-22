using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个技能，在玩家角色上使用，可以让你点击地面，让角色移动到点击的位置
    /// 您将在LoftSuspendersMouseDriven演示角色中找到此功能的演示。你可以把它拖到Loft3D演示场景的LevelManager的PlayerPrefabs插槽中进行尝试。
    /// 对于ai，请查看MousePathfinderAI3D脚本，以及它在MinimalPathfinding3D演示场景中的演示
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Pathfind To Mouse")]
	[RequireComponent(typeof(CharacterPathfinder3D))]
	public class CharacterPathfindToMouse3D : CharacterAbility
	{
		[Header("Mouse鼠标")]
		/// the index of the mouse button to read input on
		[Tooltip("要读取输入的鼠标按钮索引")]
		public int MouseButtonIndex = 1;
        
		[Header("OnClick点击")] 
		/// a feedback to play at the position of the click
		[Tooltip("在点击位置播放的反馈")]
		public MMFeedbacks OnClickFeedbacks;

		/// if this is true, a click or tap on a UI element will block the click and won't cause the character to move
		[Tooltip("如果这是真的，点击或点击UI元素将阻止点击，不会导致角色移动")]
		public bool UIShouldBlockInput = true;
        
		public virtual GameObject Destination { get; set; }

		protected CharacterPathfinder3D _characterPathfinder3D;
		protected Plane _playerPlane;
		protected bool _destinationSet = false;
		protected Camera _mainCamera;

        /// <summary>
        /// 唤醒的时候，我们创建一个平面来捕捉光线
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_mainCamera = Camera.main;
			_characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
			_character.FindAbility<CharacterMovement>().ScriptDrivenInput = true;
            
			OnClickFeedbacks?.Initialization();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			if (Destination == null)
			{
				Destination = new GameObject();
				Destination.name = this.name + "PathfindToMouseDestination";
			}
		}

        /// <summary>
        /// 每一帧我们都要确保不退出运行状态
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (!AbilityAuthorized)
			{
				return;
			}
			DetectMouse();
		}

        /// <summary>
        /// 如果点击鼠标，我们将投射光线，如果光线击中平面，我们将其作为寻路目标
        /// </summary>
        protected virtual void DetectMouse()
		{
			bool testUI = false;

			if (UIShouldBlockInput)
			{
				testUI = MMGUI.PointOrTouchBlockedByUI();
			}
            
			if (Input.GetMouseButtonDown(MouseButtonIndex) && !testUI)
			{
				Ray ray = _mainCamera.ScreenPointToRay(InputManager.Instance.MousePosition);
				Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
				float distance;
				if (_playerPlane.Raycast(ray, out distance))
				{
					Vector3 target = ray.GetPoint(distance);
					Destination.transform.position = target;
					_destinationSet = true;
					_characterPathfinder3D.SetNewDestination(Destination.transform);
					OnClickFeedbacks?.PlayFeedbacks(Destination.transform.position);
				}
			}
		}
	}
}