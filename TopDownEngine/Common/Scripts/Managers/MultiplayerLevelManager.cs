using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于处理多人场景的通用关卡管理器（特别是生成和相机模式）
    /// 建议扩展它以实现您自己的特定游戏规则
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Multiplayer Level Manager")]
	public class MultiplayerLevelManager : LevelManager
	{
		[Header("Multiplayer spawn多人游戏生成")]
		/// the list of checkpoints (in order) to use to spawn characters
		[Tooltip("用于生成角色的检查点列表（按顺序）")]
		public List<CheckPoint> SpawnPoints;
        /// 可供选择的相机类型
        public enum CameraModes { Split, Group }

		[Header("Cameras相机")]
		/// the selected camera mode (either group, all targets in one screen, or split screen)
		[Tooltip("选定的相机模式（无论是组合模式、所有目标在一个屏幕上，还是分屏模式）")]
		public CameraModes CameraMode = CameraModes.Split;
		/// the group camera rig
		[Tooltip("组合相机装备")]
		public GameObject GroupCameraRig;
		/// the split camera rig
		[Tooltip("分屏相机装备")]
		public GameObject SplitCameraRig;

		[Header("GUI ManagerGUI管理器")]
		/// the multiplayer GUI Manager
		[Tooltip("多人游戏GUI管理器")]
		public MultiplayerGUIManager MPGUIManager;

        /// <summary>
        /// 在唤醒时，我们处理不同的相机模式
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			HandleCameraModes();
		}

        /// <summary>
        /// 设置场景以匹配选定的相机模式
        /// </summary>
        protected virtual void HandleCameraModes()
		{
			if (CameraMode == CameraModes.Split)
			{
				if (GroupCameraRig != null) { GroupCameraRig.SetActive(false); }
				if (SplitCameraRig != null) { SplitCameraRig.SetActive(true); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(true);
					MPGUIManager.GroupHUD?.SetActive(false);
					MPGUIManager.SplittersGUI?.SetActive(true);
				}
			}
			if (CameraMode == CameraModes.Group)
			{
				if (GroupCameraRig != null) { GroupCameraRig?.SetActive(true); }
				if (SplitCameraRig != null) { SplitCameraRig?.SetActive(false); }
				if (MPGUIManager != null)
				{
					MPGUIManager.SplitHUD?.SetActive(false);
					MPGUIManager.GroupHUD?.SetActive(true);
					MPGUIManager.SplittersGUI?.SetActive(false);
				}
			}
		}

        /// <summary>
        /// 在指定的生成点生成所有角色
        /// </summary>
        protected override void SpawnMultipleCharacters()
		{
			for (int i = 0; i < Players.Count; i++)
			{
				SpawnPoints[i].SpawnPlayer(Players[i]);
				if (AutoAttributePlayerIDs)
				{
					Players[i].SetPlayerID("Player" + (i + 1));
				}                
			}
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.SpawnComplete, null);
		}

        /// <summary>
        /// 杀死指定的玩家
        /// </summary>
        public override void PlayerDead(Character playerCharacter)
		{
			if (playerCharacter == null)
			{
				return;
			}
			Health characterHealth = playerCharacter.CharacterHealth;
			if (characterHealth == null)
			{
				return;
			}
			else
			{
				OnPlayerDeath(playerCharacter);
			}
		}

        /// <summary>
        /// 覆盖此方法以指定玩家死亡时发生的情况
        /// </summary>
        /// <param name="playerCharacter"></param>
        protected virtual void OnPlayerDeath(Character playerCharacter)
		{

		}
	}
}