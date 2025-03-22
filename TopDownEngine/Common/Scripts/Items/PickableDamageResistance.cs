using System;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 使用这个选择器在拾取它的角色上创建新的抗性，或者启用/禁用现有的抗性
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Damage Resistance")]
	public class PickableDamageResistance : PickableItem
	{
		public enum Modes { Create, ActivateByLabel, DisableByLabel }
		
		[Header("Damage Resistance伤害抗性")]
		/// The chosen mode to interact with resistance, either creating one, activating one or disabling one
		[Tooltip("与抗性交互所选的模式，无论是创建、激活还是禁用抗性")]
		public Modes Mode = Modes.ActivateByLabel;
		
		/// If activating or disabling by label, the exact label of the target resistance
		[Tooltip("如果是通过标签激活或禁用，目标抗性的具体标签")]
		[MMEnumCondition("Mode", (int)Modes.ActivateByLabel, (int)Modes.DisableByLabel)]
		public string TargetLabel = "SomeResistance";
		
		/// in create mode, the name of the new game object to create to host the new resistance
		[Tooltip("在创建模式下，要创建的新游戏对象的名称，以承载新的抗性")]
		[MMEnumCondition("Mode", (int)Modes.Create)]
		public string NewResistanceNodeName = "NewResistance";
		/// in create mode, a DamageResistance to copy and give to the new node. Usually you'll want to create a new DamageResistance component on your picker, and drag it in this slot
		[Tooltip("在创建模式下，要复制并提供给新节点的DamageResistance。通常你会希望在拾取器上创建一个新的DamageResistance组件，并将其拖入此槽位")]
		[MMEnumCondition("Mode", (int)Modes.Create)]
		public DamageResistance DamageResistanceToGive;
		
		/// if this is true, only player characters can pick this up
		[Tooltip("如果这是真的，只有玩家角色可以拾取这个")]
		public bool OnlyForPlayerCharacter = true;

        /// <summary>
        /// 当有东西与补给包碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker)
		{
			Character character = picker.gameObject.MMGetComponentNoAlloc<Character>();
			if (OnlyForPlayerCharacter && (character != null) && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return;
			}

			Health characterHealth = picker.gameObject.MMGetComponentNoAlloc<Health>();
            // 否则，我们给予玩家生命值
            if (characterHealth == null)
			{
				return;
			}          
			DamageResistanceProcessor processor = characterHealth.TargetDamageResistanceProcessor;
			if (processor == null)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.ActivateByLabel:
					processor.SetResistanceByLabel(TargetLabel, true);
					break;
				case Modes.DisableByLabel:
					processor.SetResistanceByLabel(TargetLabel, false);
					break;
				case Modes.Create:
					if (DamageResistanceToGive == null) { return; }
					GameObject newResistance = new GameObject();
					newResistance.transform.SetParent(processor.transform);
					newResistance.name = NewResistanceNodeName;
					DamageResistance newResistanceComponent = MMHelpers.CopyComponent<DamageResistance>(DamageResistanceToGive, newResistance);
					processor.DamageResistanceList.Add(newResistanceComponent);
					break;
			}
		}
	}
}