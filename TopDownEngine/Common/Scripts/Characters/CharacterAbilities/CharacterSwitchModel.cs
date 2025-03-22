using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到角色中，它将能够切换其模型
    /// 当按SwitchCharacter按钮时
    /// 注意，这只会改变模型，而不是预制件。只有视觉表现，而不是能力和设置。
    /// 如果你想完全改变预制件，看看CharacterSwitchManager类。
    /// 如果你想在场景中的一堆角色之间交换角色，请查看CharacterSwap功能和CharacterSwapManager
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Switch Model")] 
	public class CharacterSwitchModel : CharacterAbility
	{
        /// 可以选择下一个角色的顺序
        public enum NextModelChoices { Sequential, Random }

		[Header("Models模型")]
		[MMInformation("将此组件添加到角色中，当按SwitchCharacter按钮（默认为P）时，它将能够切换其模型。", MMInformationAttribute.InformationType.Info, false)]

		/// the list of possible characters models to switch to
		[Tooltip("可能切换到的角色模型列表")]
		public GameObject[] CharacterModels;
		/// the order in which to pick the next character
		[Tooltip("选择下一个角色的顺序")]
		public NextModelChoices NextCharacterChoice = NextModelChoices.Sequential;
		/// the initial (and at runtime, current) index of the character prefab
		[Tooltip("角色预制的初始（以及在运行时的当前）索引")]
		public int CurrentIndex = 0;
		/// if you set this to true, when switching model, the Character's animator will also be bound. This requires your model's animator is at the top level of the model in the hierarchy.
		/// you can look at the MinimalModelSwitch scene for examples of that
		[Tooltip("如果你将这个设置为真，当切换模型时，角色的动画控制器也会被绑定。这要求你的模型的动画控制器在层次结构中位于模型的顶层。你可以看看MinimalModelSwitch场景的例子")]
		public bool AutoBindAnimator = true;

		[Header("Visual Effects视觉效果")]
		/// a particle system to play when a character gets changed
		[Tooltip("当角色发生变化时的粒子系统")]
		public ParticleSystem CharacterSwitchVFX;

		protected ParticleSystem _instantiatedVFX;
		protected string _bindAnimatorMessage = "BindAnimator";
		protected bool[] _characterModelsFlipped;

        /// <summary>
        /// 在init中，我们禁用模型并激活当前模型
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (CharacterModels.Length == 0)
			{
				return;
			}

			foreach (GameObject model in CharacterModels)
			{
				model.SetActive(false);
			}

			CharacterModels[CurrentIndex].SetActive(true);
			_characterModelsFlipped = new bool[CharacterModels.Length];
			InstantiateVFX();
		}

        /// <summary>
        /// 如果需要，实例化和禁用粒子系统
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
        /// 在每个循环开始时，我们检查是否按下或释放了开关按钮
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchModel();
			}	
		}

        /// <summary>
        /// 另一方面，我们存储当前模型的状态
        /// </summary>
        public override void Flip()
		{
			if (_characterModelsFlipped == null)
			{
				_characterModelsFlipped = new bool[CharacterModels.Length];
			}
			if (_characterModelsFlipped.Length == 0)
			{
				_characterModelsFlipped = new bool[CharacterModels.Length];
			}
			if (_character == null)
			{
				_character = this.gameObject.GetComponentInParent<Character>();
			}
		}

        /// <summary>
        /// 切换到下一个模型
        /// </summary>
        protected virtual void SwitchModel()
		{
			if (CharacterModels.Length <= 1) 
			{
				return;
			}
            
			CharacterModels[CurrentIndex].gameObject.SetActive(false);

            // 我们确定下一个指标
            if (NextCharacterChoice == NextModelChoices.Random)
			{
				CurrentIndex = Random.Range(0, CharacterModels.Length);
			}
			else
			{
				CurrentIndex = CurrentIndex + 1;
				if (CurrentIndex >= CharacterModels.Length)
				{
					CurrentIndex = 0;
				}
			}

            // 我们激活新的当前模型
            CharacterModels[CurrentIndex].gameObject.SetActive(true);
			_character.CharacterModel = CharacterModels[CurrentIndex];

            // 我们绑定动画器
            if (AutoBindAnimator)
			{
				_character.CharacterAnimator = CharacterModels[CurrentIndex].gameObject.MMGetComponentNoAlloc<Animator>();
				_character.AssignAnimator(true);
				SendMessage(_bindAnimatorMessage, SendMessageOptions.DontRequireReceiver);    
				
				List<CharacterHandleWeapon> handleWeapons = _character.FindAbilities<CharacterHandleWeapon>();
				foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
				{
					if ((handleWeapon.AutomaticallyBindAnimator) && (handleWeapon.CurrentWeapon != null))
					{
						handleWeapon.CharacterAnimator = _character.CharacterAnimator;
						handleWeapon.CurrentWeapon.SetOwner(_character, handleWeapon);
					}
				}
			}

            // 我们玩我们的视觉特效
            if (_instantiatedVFX != null)
			{
				_instantiatedVFX.gameObject.SetActive(true);
				_instantiatedVFX.transform.position = this.transform.position;
				_instantiatedVFX.Play();
			}
            // 我们播放我们的反馈
            PlayAbilityStartFeedbacks();
		}
	}
}