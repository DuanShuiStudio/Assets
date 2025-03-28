﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 角色可能使用的事件列表
    /// </summary>
    public enum MMCharacterEventTypes
	{
		ButtonActivation,
		Jump
	}

    /// <summary>
    /// 除了由角色状态机触发的事件之外，还使用MMCharacterEvents来指示发生的事情，这些事情不一定与状态的变化相关联
    /// </summary>
    public struct MMCharacterEvent
	{
		public Character TargetCharacter;
		public MMCharacterEventTypes EventType;
		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.MMCharacterEvent"/> struct.
		/// </summary>
		/// <param name="character">Character.</param>
		/// <param name="eventType">Event type.</param>
		public MMCharacterEvent(Character character, MMCharacterEventTypes eventType)
		{
			TargetCharacter = character;
			EventType = eventType;
		}

		static MMCharacterEvent e;
		public static void Trigger(Character character, MMCharacterEventTypes eventType)
		{
			e.TargetCharacter = character;
			e.EventType = eventType;
			MMEventManager.TriggerEvent(e);
		}
	}
	
	public enum MMLifeCycleEventTypes { Death, Revive }

	public struct MMLifeCycleEvent
	{
		public Health AffectedHealth;
		public MMLifeCycleEventTypes MMLifeCycleEventType;
		
		public MMLifeCycleEvent(Health affectedHealth, MMLifeCycleEventTypes lifeCycleEventType)
		{
			AffectedHealth = affectedHealth;
			MMLifeCycleEventType = lifeCycleEventType;
		}

		static MMLifeCycleEvent e;
		public static void Trigger(Health affectedHealth, MMLifeCycleEventTypes lifeCycleEventType)
		{
			e.AffectedHealth = affectedHealth;
			e.MMLifeCycleEventType = lifeCycleEventType;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 当某物受到伤害时触发的事件
    /// </summary>
    public struct MMDamageTakenEvent
	{
		public Health AffectedHealth;
		public GameObject Instigator;
		public float CurrentHealth;
		public float DamageCaused;
		public float PreviousHealth;
		public List<TypedDamage> TypedDamages; 

		/// <summary>
		/// Initializes a new instance of the <see cref="MoreMountains.TopDownEngine.MMDamageTakenEvent"/> struct.
		/// </summary>
		/// <param name="affectedHealth">Affected Health.</param>
		/// <param name="instigator">Instigator.</param>
		/// <param name="currentHealth">Current health.</param>
		/// <param name="damageCaused">Damage caused.</param>
		/// <param name="previousHealth">Previous health.</param>
		public MMDamageTakenEvent(Health affectedHealth, GameObject instigator, float currentHealth, float damageCaused, float previousHealth, List<TypedDamage> typedDamages )
		{
			AffectedHealth = affectedHealth;
			Instigator = instigator;
			CurrentHealth = currentHealth;
			DamageCaused = damageCaused;
			PreviousHealth = previousHealth;
			TypedDamages = typedDamages;
		}

		static MMDamageTakenEvent e;
		public static void Trigger(Health affectedHealth, GameObject instigator, float currentHealth, float damageCaused, float previousHealth, List<TypedDamage> typedDamages )
		{
			e.AffectedHealth = affectedHealth;
			e.Instigator = instigator;
			e.CurrentHealth = currentHealth;
			e.DamageCaused = damageCaused;
			e.PreviousHealth = previousHealth;
			e.TypedDamages = typedDamages;
			MMEventManager.TriggerEvent(e);
		}
	}
}