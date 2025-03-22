using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到场景中的一个空对象，当你按下SwitchCharacter交换角色按钮（默认为P键，可以在Unity的InputManager设置中更改），
    /// 您的主角色将被此组件中设置的列表中的预制件之一替换。您可以决定顺序（顺序或随机），并且可以拥有任意数量的角色
    /// 请注意，这会改变整个预制件，而不仅仅是视觉效果
    /// 如果你只是想要改变视觉效果，请查看CharacterSwitchModel能力
    /// 如果你想在场景中的一群角色之间交换角色，请查看CharacterSwap能力和CharacterSwapManager
    /// </summary>
    [AddComponentMenu("TopDown Engine/Managers/Character Switch Manager")]
	public class CharacterSwitchManager : TopDownMonoBehaviour
	{
        /// 下一个角色可以选择的可能顺序
        public enum NextCharacterChoices { Sequential, Random }

		[Header("Character Switch角色交换")]
		[MMInformation("将此组件添加到场景中的一个空对象，当你按下SwitchCharacter角色交换按钮（默认为P键，可以在Unity的InputManager设置中更改），你的主角色将被此组件中设置的列表中的预制件之一替换。你可以决定顺序（顺序或随机），并且可以拥有任意数量的角色", MMInformationAttribute.InformationType.Info, false)]

		/// the list of possible characters prefabs to switch to
		[Tooltip("要切换到的可能角色预制件的列表")]
		public Character[] CharacterPrefabs;
		/// the order in which to pick the next character
		[Tooltip("选择下一个角色的顺序")]
		public NextCharacterChoices NextCharacterChoice = NextCharacterChoices.Sequential;
		/// the initial (and at runtime, current) index of the character prefab
		[Tooltip("角色预制件的初始（以及在运行时，当前的）索引")]
		public int CurrentIndex = 0;
		/// if this is true, current health value will be passed from character to character
		[Tooltip("如果这是真的，当前的生命值将从这个角色传递到另一个角色")]
		public bool CommonHealth;

		[Header("Visual Effects视觉效果")]
		/// a particle system to play when a character gets changed
		[Tooltip("当角色发生改变时播放的粒子系统")]
		public ParticleSystem CharacterSwitchVFX;

		protected Character[] _instantiatedCharacters;
		protected ParticleSystem _instantiatedVFX;
		protected InputManager _inputManager;
		protected TopDownEngineEvent _switchEvent = new TopDownEngineEvent(TopDownEngineEventTypes.CharacterSwitch, null);

        /// <summary>
        /// 在唤醒时，我们抓取输入管理器并实例化角色和VFX视觉效果
        /// </summary>
        protected virtual void Start()
		{
			_inputManager = FindObjectOfType(typeof(InputManager)) as InputManager;
			InstantiateCharacters();
			InstantiateVFX();
		}

        /// <summary>
        /// 实例化并禁用我们列表中的所有角色
        /// </summary>
        protected virtual void InstantiateCharacters()
		{
			_instantiatedCharacters = new Character[CharacterPrefabs.Length];

			for (int i = 0; i < CharacterPrefabs.Length; i++)
			{
				Character newCharacter = Instantiate(CharacterPrefabs[i]);
				newCharacter.name = "CharacterSwitch_" + i;
				newCharacter.gameObject.SetActive(false);
				_instantiatedCharacters[i] = newCharacter;
			}
		}

        /// <summary>
        /// 根据需要实例化并禁用粒子系统
        /// </summary>
        protected virtual void InstantiateVFX()
		{
			if (CharacterSwitchVFX != null)
			{
				_instantiatedVFX = Instantiate(CharacterSwitchVFX);
				_instantiatedVFX.Stop();
				_instantiatedVFX.gameObject.SetActive(false);
			}
		}

        /// <summary>
        /// 在更新时，我们监视输入
        /// </summary>
        protected virtual void Update()
		{
			if (_inputManager == null)
			{
				return;
			}

			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchCharacter();
			}
		}

        /// <summary>
        /// 切换到列表中的下一个角色
        /// </summary>
        protected virtual void SwitchCharacter()
		{
			if (_instantiatedCharacters.Length <= 1)
			{
				return;
			}

            // 我们确定下一个索引
            if (NextCharacterChoice == NextCharacterChoices.Random)
			{
				CurrentIndex = Random.Range(0, _instantiatedCharacters.Length);
			}
			else
			{
				CurrentIndex = CurrentIndex + 1;
				if (CurrentIndex >= _instantiatedCharacters.Length)
				{
					CurrentIndex = 0;
				}
			}

            // 我们禁用旧的主角色，并启用新的角色
            LevelManager.Instance.Players[0].gameObject.SetActive(false);
			_instantiatedCharacters[CurrentIndex].gameObject.SetActive(true);

            // 我们将新角色移动到旧角色的位置
            _instantiatedCharacters[CurrentIndex].transform.position = LevelManager.Instance.Players[0].transform.position;
			_instantiatedCharacters[CurrentIndex].transform.rotation = LevelManager.Instance.Players[0].transform.rotation;

            // 如果需要，我们保持生命值
            if (CommonHealth)
			{
				_instantiatedCharacters[CurrentIndex].CharacterHealth.SetHealth(LevelManager.Instance.Players[0].gameObject.MMGetComponentNoAlloc<Health>().CurrentHealth);
			}

            // 我们使其处于与旧角色相同的状态
            _instantiatedCharacters[CurrentIndex].MovementState.ChangeState(LevelManager.Instance.Players[0].MovementState.CurrentState);
			_instantiatedCharacters[CurrentIndex].ConditionState.ChangeState(LevelManager.Instance.Players[0].ConditionState.CurrentState);

            // 我们使其成为当前角色
            LevelManager.Instance.Players[0] = _instantiatedCharacters[CurrentIndex];

            // 播放我们的vfx视觉效果
            if (_instantiatedVFX != null)
			{
				_instantiatedVFX.gameObject.SetActive(true);
				_instantiatedVFX.transform.position = _instantiatedCharacters[CurrentIndex].transform.position;
				_instantiatedVFX.Play();
			}

            // 我们触发一个切换事件（主要是为了让相机知道）
            MMEventManager.TriggerEvent(_switchEvent);
			MMCameraEvent.Trigger(MMCameraEventTypes.RefreshAutoFocus, LevelManager.Instance.Players[0], null);
		}
	}
}