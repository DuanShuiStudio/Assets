using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{	
	[AddComponentMenu("TopDown Engine/Environment/Teleporter")]
    /// <summary>
    /// 将此脚本添加到触发器2D碰撞器或碰撞器，以实现从该对象传送到其目的地的功能。
    /// </summary>
    public class Teleporter : ButtonActivated 
	{
        /// 传送装置在激活时与相机系统交互的可能模式，包括不执行任何操作、将相机传送到新位置或在Cinemachine虚拟相机之间进行混合。
        public enum CameraModes { DoNothing, TeleportCamera, CinemachinePriority }
        /// 可能的传送模式（要么是1帧的瞬间传送，要么是此传送器与其目的地之间的补间动画）。
        public enum TeleportationModes { Instant, Tween }
        /// 可能的时间模式
        public enum TimeModes { Unscaled, Scaled }

		[MMInspectorGroup("Teleporter", true, 18)]

		/// if true, this won't teleport non player characters
		[Tooltip("如果为真，则不会传送非玩家角色")]
		public bool OnlyAffectsPlayer = true;
		/// the offset to apply when exiting this teleporter
		[Tooltip("离开此传送器时应用的偏移量")]
		public Vector3 ExitOffset;
		/// the selected teleportation mode 
		[Tooltip("选定的传送模式 ")]
		public TeleportationModes TeleportationMode = TeleportationModes.Instant;
		/// the curve to apply to the teleportation tween 
		[MMEnumCondition("TeleportationMode", (int)TeleportationModes.Tween)]
		[Tooltip("应用于传送补间动画的曲线")]
		public MMTween.MMTweenCurve TweenCurve = MMTween.MMTweenCurve.EaseInCubic;
		/// whether or not to maintain the x value of the teleported object on exit
		[Tooltip("是否在退出时保持传送对象的x值")]
		public bool MaintainXEntryPositionOnExit = false;
		/// whether or not to maintain the y value of the teleported object on exit
		[Tooltip("是否在退出时保持传送对象的y值")]
		public bool MaintainYEntryPositionOnExit = false;
		/// whether or not to maintain the z value of the teleported object on exit
		[Tooltip("是否在退出时保持传送对象的z值")]
		public bool MaintainZEntryPositionOnExit = false;

		[MMInspectorGroup("Destination", true, 19)]

		/// the teleporter's destination
		[Tooltip("传送器的目的地")]
		public Teleporter Destination;
		/// if this is true, the teleported object will be put on the destination's ignore list, to prevent immediate re-entry. If your 
		/// destination's offset is far enough from its center, you can set that to false
		[Tooltip("如果此项为真，传送的对象将被添加到目的地的忽略列表中，以防止其立即重新进入。如果您的目的地的偏移量足够远离其中心，您可以将其设置为假")]
		public bool AddToDestinationIgnoreList = true;

		[MMInspectorGroup("Rooms", true, 20)]

		/// the chosen camera mode
		[Tooltip("选定的相机模式")]
		public CameraModes CameraMode = CameraModes.TeleportCamera;
		/// the room this teleporter belongs to
		[Tooltip("这个传送器所属的房间")]
		public Room CurrentRoom;
		/// the target room
		[Tooltip("目标房间")]
		public Room TargetRoom;
        
		[MMInspectorGroup("MMFader Transtitions", true, 21)]

		/// if this is true, a fade to black will occur when teleporting
		[Tooltip("如果此项为真，传送时会发生渐隐为黑色的效果")]
		public bool TriggerFade = false;
		/// the ID of the fader to target
		[MMCondition("TriggerFade", true)]
		[Tooltip("要渐隐的目标的ID")]
		public int FaderID = 0;
		/// the curve to use to fade to black
		[Tooltip("用于渐隐为黑色的曲线")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);
		/// if this is true, fade events will ignore timescale
		[Tooltip("如果此项为真，渐隐事件将忽略时间缩放")]
		public bool FadeIgnoresTimescale = false;

		[MMInspectorGroup("Mask", true, 22)]

		/// whether or not we should ask to move a MMSpriteMask on activation
		[Tooltip("是否应在激活时请求移动MMSpriteMask")]
		public bool MoveMask = true;
		/// the curve to move the mask along to
		[MMCondition("MoveMask", true)]
		[Tooltip("用于沿路径移动蒙版的曲线")]
		public MMTween.MMTweenCurve MoveMaskCurve = MMTween.MMTweenCurve.EaseInCubic;
		/// the method to move the mask
		[MMCondition("MoveMask", true)]
		[Tooltip("用于移动蒙版的方法")]
		public MMSpriteMaskEvent.MMSpriteMaskEventTypes MoveMaskMethod = MMSpriteMaskEvent.MMSpriteMaskEventTypes.ExpandAndMoveToNewPosition;
		/// the duration of the mask movement (usually the same as the DelayBetweenFades
		[MMCondition("MoveMask", true)]
		[Tooltip("蒙版移动的持续时间（通常与渐隐之间的延迟时间相同）")]
		public float MoveMaskDuration = 0.2f;

		[MMInspectorGroup("Freeze", true, 23)]
		/// whether or not time should be frozen during the transition
		[Tooltip("是否应在过渡期间冻结时间")]
		public bool FreezeTime = false;
		/// whether or not the character should be frozen (input blocked) for the duration of the transition
		[Tooltip("是否应在过渡期间冻结角色（输入被阻止）")]
		public bool FreezeCharacter = true;

		[MMInspectorGroup("Teleport Sequence", true, 24)]
		/// the timescale to use for the teleport sequence
		[Tooltip("用于传送序列的时间缩放")]
		public TimeModes TimeMode = TimeModes.Unscaled;
		/// the delay (in seconds) to apply before running the sequence
		[Tooltip("在运行序列之前要应用的延迟（以秒为单位）")]
		public float InitialDelay = 0.1f;
		/// the duration (in seconds) after the initial delay covering for the fade out of the scene
		[Tooltip("在初始延迟之后，覆盖场景渐隐的持续时间（以秒为单位）")]
		public float FadeOutDuration = 0.2f;
		/// the duration (in seconds) to wait for after the fade out and before the fade in
		[Tooltip("在渐隐出后和渐隐入前要等待的持续时间（以秒为单位）")]
		public float DelayBetweenFades = 0.3f;
		/// the duration (in seconds) after the initial delay covering for the fade in of the scene
		[Tooltip("在初始延迟之后，覆盖场景渐隐入的持续时间（以秒为单位）")]
		public float FadeInDuration = 0.2f;
		/// the duration (in seconds) to apply after the fade in of the scene
		[Tooltip("在场景渐隐入后要应用的持续时间（以秒为单位）")]
		public float FinalDelay = 0.1f;

		public virtual float LocalTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledTime : Time.time;
		public virtual float LocalDeltaTime => (TimeMode == TimeModes.Unscaled) ? Time.unscaledDeltaTime : Time.deltaTime;

		protected Character _player;
		protected Character _characterTester;
		protected CharacterGridMovement _characterGridMovement;
		protected List<Transform> _ignoreList;

		protected Vector3 _entryPosition;
		protected Vector3 _newPosition;

        /// <summary>
        /// 在开始时，我们初始化忽略列表
        /// </summary>
        protected virtual void Awake()
		{
			InitializeTeleporter();
		}

        /// <summary>
        /// 如果需要，则在父级中抓取当前房间
        /// </summary>
        protected virtual void InitializeTeleporter()
		{
			_ignoreList = new List<Transform>();
			if (CurrentRoom == null)
			{
				CurrentRoom = this.gameObject.GetComponentInParent<Room>();
			}
		}

        /// <summary>
        /// 当有东西进入传送器时触发
        /// </summary>
        /// <param name="collider">Collider.</param>
        protected override void TriggerEnter(GameObject collider)
		{
            // 如果与传送器碰撞的对象在其忽略列表中，则不执行任何操作并退出
            if (_ignoreList.Contains(collider.transform))
			{
				return;
			}

			_characterTester = collider.GetComponent<Character>();

			if (_characterTester != null)
			{
				if (RequiresPlayerType)
				{
					if (_characterTester.CharacterType != Character.CharacterTypes.Player)
					{
						return;
					}
				}

				_player = _characterTester;
				_characterGridMovement = _player.GetComponent<CharacterGridMovement>();
			}

            // 如果传送器应该只影响玩家，则不执行任何操作并退出。
            if (OnlyAffectsPlayer || !AutoActivation)
			{
				base.TriggerEnter(collider);
			}
			else
			{
				base.TriggerButtonAction();
				Teleport(collider);
			}
		}

        /// <summary>
        /// 如果我们是按钮激活的，并且按钮被按下，我们就进行传送
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			Teleport(_player.gameObject);
		}

        /// <summary>
        /// 将进入门户的任何对象传送到新的目的地
        /// </summary>
        protected virtual void Teleport(GameObject collider)
		{
			_entryPosition = collider.transform.position;
            // 如果传送器有目的地，我们将碰撞的对象移动到该目的地
            if (Destination != null)
			{
				StartCoroutine(TeleportSequence(collider));         
			}
		}

        /// <summary>
        /// 处理传送序列（渐隐入、暂停、渐隐出）
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual IEnumerator TeleportSequence(GameObject collider)
		{
			SequenceStart(collider);

			for (float timer = 0, duration = InitialDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }
            
			AfterInitialDelay(collider);

			for (float timer = 0, duration = FadeOutDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeOut(collider);
            
			for (float timer = 0, duration = DelayBetweenFades; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterDelayBetweenFades(collider);

			for (float timer = 0, duration = FadeInDuration; timer < duration; timer += LocalDeltaTime) { yield return null; }

			AfterFadeIn(collider);

			for (float timer = 0, duration = FinalDelay; timer < duration; timer += LocalDeltaTime) { yield return null; }

			SequenceEnd(collider);
		}

        /// <summary>
        /// 描述在初始渐隐入之前发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void SequenceStart(GameObject collider)
		{
			if (CameraMode == CameraModes.TeleportCamera)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.StopFollowing);
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0f, 0f, false, 0f, true);
			}

			if (FreezeCharacter && (_player != null))
			{
				_player.Freeze();
			}
		}

        /// <summary>
        /// 描述在初始延迟过后发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void AfterInitialDelay(GameObject collider)
		{            
			if (TriggerFade)
			{
				MMFadeInEvent.Trigger(FadeOutDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

        /// <summary>
        /// 描述在初始渐隐入完成后发生的事件
        /// </summary>
        protected virtual void AfterFadeOut(GameObject collider)
		{   
			#if MM_CINEMACHINE || MM_CINEMACHINE3         
			TeleportCollider(collider);

			if (AddToDestinationIgnoreList)
			{
				Destination.AddToIgnoreList(collider.transform);
			}            
            
			if (CameraMode == CameraModes.CinemachinePriority)
			{
				MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
				MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, DelayBetweenFades);
			}

			if (CurrentRoom != null)
			{
				CurrentRoom.PlayerExitsRoom();
			}
            
			if (TargetRoom != null)
			{
				TargetRoom.PlayerEntersRoom();
				#if MM_CINEMACHINE || MM_CINEMACHINE3 
				if (TargetRoom.VirtualCamera != null)
				{
					TargetRoom.VirtualCamera.Priority = 10;	
				}
				#endif
				MMSpriteMaskEvent.Trigger(MoveMaskMethod, (Vector2)TargetRoom.RoomColliderCenter, TargetRoom.RoomColliderSize, MoveMaskDuration, MoveMaskCurve);
			}
			#endif
		}

        /// <summary>
        /// 将通过传送器的对象进行传送，可以是立即传送或补间传送。
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void TeleportCollider(GameObject collider)
		{
			_newPosition = Destination.transform.position + Destination.ExitOffset;
			if (MaintainXEntryPositionOnExit)
			{
				_newPosition.x = _entryPosition.x;
			}
			if (MaintainYEntryPositionOnExit)
			{
				_newPosition.y = _entryPosition.y;
			}
			if (MaintainZEntryPositionOnExit)
			{
				_newPosition.z = _entryPosition.z;
			}

			switch (TeleportationMode)
			{
				case TeleportationModes.Instant:
					collider.transform.position = _newPosition;
					_ignoreList.Remove(collider.transform);
					break;
				case TeleportationModes.Tween:
					StartCoroutine(TeleportTweenCo(collider, collider.transform.position, _newPosition));
					break;
			}
		}

        /// <summary>
        /// 将对象从原点传送到目的地
        /// </summary>
        /// <param name="collider"></param>
        /// <param name="origin"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected virtual IEnumerator TeleportTweenCo(GameObject collider, Vector3 origin, Vector3 destination)
		{
			float startedAt = LocalTime;
			while (LocalTime - startedAt < DelayBetweenFades)
			{
				float elapsedTime = LocalTime - startedAt;
				collider.transform.position = MMTween.Tween(elapsedTime, 0f, DelayBetweenFades, origin, destination, TweenCurve);
				yield return null;
			}
			_ignoreList.Remove(collider.transform);
		}

        /// <summary>
        /// 描述在渐隐入和渐隐出之间的暂停时发生的事件
        /// </summary>
        protected virtual void AfterDelayBetweenFades(GameObject collider)
		{
			MMCameraEvent.Trigger(MMCameraEventTypes.StartFollowing);

			if (TriggerFade)
			{
				MMFadeOutEvent.Trigger(FadeInDuration, FadeTween, FaderID, FadeIgnoresTimescale, LevelManager.Instance.Players[0].transform.position);
			}
		}

        /// <summary>
        /// 描述在场景渐隐入后发生的事件
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void AfterFadeIn(GameObject collider)
		{

		}

        /// <summary>
        /// 描述在渐隐出完成后发生的事件，即在传送序列的末
        /// </summary>
        protected virtual void SequenceEnd(GameObject collider)
		{
			if (FreezeCharacter && (_player != null))
			{
				_player.UnFreeze();
			}

			if (_characterGridMovement != null)
			{
				_characterGridMovement.SetCurrentWorldPositionAsNewPosition();
			}

			if (FreezeTime)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

        /// <summary>
        /// 当有物体离开传送器时，如果它在忽略列表中，我们就将其从列表中移除，以便下次它进入时能够被考虑
        /// </summary>
        /// <param name="collider">Collider.</param>
        public override void TriggerExitAction(GameObject collider)
		{
			if (_ignoreList.Contains(collider.transform))
			{
				_ignoreList.Remove(collider.transform);
			}
			base.TriggerExitAction(collider);
		}

        /// <summary>
        /// 将一个物体添加到忽略列表中，这将防止传送器在物体处于该列表中时移动该物体
        /// </summary>
        /// <param name="objectToIgnore">Object to ignore.</param>
        public virtual void AddToIgnoreList(Transform objectToIgnore)
		{
			if (!_ignoreList.Contains(objectToIgnore))
			{
				_ignoreList.Add(objectToIgnore);
			}            
		}

        /// <summary>
        /// 在绘制Gizmos时，如果有目标目的地和目标房间，我们会向其绘制箭头
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (Destination != null)
			{
                // 从这个传送器向其目的地绘制一个箭头
                MMDebug.DrawGizmoArrow(this.transform.position, (Destination.transform.position + Destination.ExitOffset) - this.transform.position, Color.cyan, 1f, 25f);
                // 在退出位置绘制一个点
                MMDebug.DebugDrawCross(this.transform.position + ExitOffset, 0.5f, Color.yellow);
				MMDebug.DrawPoint(this.transform.position + ExitOffset, Color.yellow, 0.5f);
			}

			if (TargetRoom != null)
			{
                // 向目的地房间绘制一个箭头
                MMDebug.DrawGizmoArrow(this.transform.position, TargetRoom.transform.position - this.transform.position, MMColors.Pink, 1f, 25f);
			}
		}
	}
}