using System;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
	public class DeadlineStartMenuButton : TopDownMonoBehaviour
	{
		public enum Types { NewGame, Continue }

		public Types Type = Types.NewGame;

		public string CharacterSelectionSceneName = "DeadlineCharacterSelection";
		public string LevelSelectionSceneName = "DeadlineLevelSelection";

        /// 在开始新游戏时用于擦除的InventoryEngine数据文件夹
        public string SaveFolderName = "InventoryEngine"; 
		
		[MMInspectorButton("DeleteProgress")]
		public bool DeleteProgressButton;
		
		protected Button _button;
		
		protected virtual void Start()
		{
			_button = this.gameObject.GetComponent<Button>();

			if (Type == Types.Continue)
			{
				_button.interactable = ProgressExists();
			}
		}
		
		public virtual void GoToLevel()
		{
			switch (Type)
			{
				case Types.NewGame:
					DeleteProgress();
					MMSceneLoadingManager.LoadScene(CharacterSelectionSceneName);
					break;
				case Types.Continue:
					MMSceneLoadingManager.LoadScene(LevelSelectionSceneName);
					break;
			}
		}

		protected virtual bool ProgressExists()
		{
			bool progessExists = false;
			foreach (DeadlineScene scene in DeadlineProgressManager.Instance.Scenes)
			{
				if (scene.LevelComplete)
				{
					progessExists = true;
				}
			}
			return progessExists;
		}

		public virtual void DeleteProgress()
		{
			MMSaveLoadManager.DeleteSaveFolder (SaveFolderName);
			DeadlineProgressManager.Instance.ResetProgress ();
		}
	}
}
