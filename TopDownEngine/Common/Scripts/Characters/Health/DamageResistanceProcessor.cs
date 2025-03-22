using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件与生命值组件连接起来，它将能够通过抵抗，处理伤害减少/增加，条件变化，移动倍增器，反馈等来处理传入的伤害。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance Processor")]
	public class DamageResistanceProcessor : TopDownMonoBehaviour
	{
		[Header("Damage Resistance List伤害抗性表")]
		
		/// If this is true, this component will try to auto-fill its list of damage resistances from the ones found in its children 
		[Tooltip("如果这是真的，这个组件将尝试从它的子组件中自动填充它的伤害抗性列表")]
		public bool AutoFillDamageResistanceList = true;
		/// If this is true, disabled resistances will be ignored by the auto fill 
		[Tooltip("如果这是真的，禁用的抗性将被自动填充忽略")]
		public bool IgnoreDisabledResistances = true;
		/// If this is true, damage from damage types that this processor has no resistance for will be ignored
		[Tooltip("如果这是真的，来自这个处理器没有抗性的伤害类型的伤害将被忽略")]
		public bool IgnoreUnknownDamageTypes = false;
		
		/// the list of damage resistances this processor will handle. Auto filled if AutoFillDamageResistanceList is true
		[FormerlySerializedAs("DamageResitanceList")] 
		[Tooltip("该处理器将处理的伤害抗性列表。如果AutoFillDamageResistanceList为true，则自动填充")]
		public List<DamageResistance> DamageResistanceList;

        /// <summary>
        /// 在awake状态下，初始化处理器
        /// </summary>
        protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// 如果需要，自动找到抗性并对其进行分类
		/// </summary>
		protected virtual void Initialization()
		{
			if (AutoFillDamageResistanceList)
			{
				DamageResistance[] foundResistances =
					this.gameObject.GetComponentsInChildren<DamageResistance>(
						includeInactive: !IgnoreDisabledResistances);
				if (foundResistances.Length > 0)
				{
					DamageResistanceList = foundResistances.ToList();	
				}
			}
			SortDamageResistanceList();
		}

        /// <summary>
        /// 一种用于根据默认优先级对抗性列表重新排序的方法。
        /// 如果您希望以不同的顺序处理您的抗性，请毫不犹豫地重写此方法
        /// </summary>
        public virtual void SortDamageResistanceList()
		{
			// we sort the list by priority
			DamageResistanceList.Sort((p1,p2)=>p1.Priority.CompareTo(p2.Priority));
		}

        /// <summary>
        /// 通过抗性列表处理传入伤害值，并输出最终伤害值
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="typedDamages"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        public virtual float ProcessDamage(float damage, List<TypedDamage> typedDamages, bool damageApplied)
		{
			float totalDamage = 0f;
			if (DamageResistanceList.Count == 0) // if we don't have resistances, we output raw damage
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
				if (IgnoreUnknownDamageTypes)
				{
					totalDamage = damage;
				}
				return totalDamage;
			}
            else // 如果有抗性
            {
				totalDamage = damage;
				
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					totalDamage = resistance.ProcessDamage(totalDamage, null, damageApplied);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						float currentDamage = typedDamage.DamageCaused;
						
						bool atLeastOneResistanceFound = false;
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (resistance.TypeResistance == typedDamage.AssociatedDamageType)
							{
								atLeastOneResistanceFound = true;
							}
							currentDamage = resistance.ProcessDamage(currentDamage, typedDamage.AssociatedDamageType, damageApplied);
						}
						if (IgnoreUnknownDamageTypes && !atLeastOneResistanceFound)
						{
                            // 我们不加到总数上
                        }
                        else
						{
							totalDamage += currentDamage;	
						}
						
					}
				}
				
				return totalDamage;
			}
		}

		public virtual void SetResistanceByLabel(string searchedLabel, bool active)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (resistance.Label == searchedLabel)
				{
					resistance.gameObject.SetActive(active);
				}
			}
		}

        /// <summary>
        /// 当打断指定类型的所有伤害时，如果需要，停止它们相关的反馈
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void InterruptDamageOverTime(DamageType damageType)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if ( resistance.gameObject.activeInHierarchy &&
					((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) ||
				        (resistance.TypeResistance == damageType))
				    && resistance.InterruptibleFeedback)
				{
					resistance.OnDamageReceived?.StopFeedbacks();
				}
			}
		}

        /// <summary>
        /// 检查是否有任何抗性阻止角色改变条件，如果是，则返回true，否则返回false
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventCharacterConditionChange(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventCharacterConditionChange))
					{
						return true;
					}
				}
			}
			return false;
		}

        /// <summary>
        /// 检查是否有任何抗性阻止角色改变条件，如果是，则返回true，否则返回false
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventMovementModifier(DamageType typedDamage)
		{
			foreach (DamageResistance resistance in DamageResistanceList)
			{
				if (!resistance.gameObject.activeInHierarchy)
				{
					continue;
				}
				if (typedDamage == null)
				{
					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;	
					}
				}
				else
				{
					if ((resistance.TypeResistance == typedDamage) &&
					    (resistance.PreventMovementModifier))
					{
						return true;
					}
				}
			}
			return false;
		}

        /// <summary>
        /// 如果此处理器上的抗性使其免疫反击，则返回true，否则返回false
        /// </summary>
        /// <param name="typedDamage"></param>
        /// <returns></returns>
        public virtual bool CheckPreventKnockback(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (typedDamages.Count == 0))
			{
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					if (!resistance.gameObject.activeInHierarchy)
					{
						continue;
					}

					if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
					    (resistance.ImmuneToKnockback))
					{
						return true;	
					}
				}
			}
			else
			{
				foreach (TypedDamage typedDamage in typedDamages)
				{
					foreach (DamageResistance resistance in DamageResistanceList)
					{
						if (!resistance.gameObject.activeInHierarchy)
						{
							continue;
						}

						if (typedDamage == null)
						{
							if ((resistance.DamageTypeMode == DamageTypeModes.BaseDamage) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;	
							}
						}
						else
						{
							if ((resistance.TypeResistance == typedDamage.AssociatedDamageType) &&
							    (resistance.ImmuneToKnockback))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

        /// <summary>
        /// 通过各种抗性处理输入的反击
        /// </summary>
        /// <param name="knockback"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ProcessKnockbackForce(Vector3 knockback, List<TypedDamage> typedDamages)
		{
			if (DamageResistanceList.Count == 0) // 如果没有抗性，我们输出原始反伤值
            {
				return knockback;
			}
            else // 如果有抗性
            {
				foreach (DamageResistance resistance in DamageResistanceList)
				{
					knockback = resistance.ProcessKnockback(knockback, null);
				}

				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						foreach (DamageResistance resistance in DamageResistanceList)
						{
							if (IgnoreDisabledResistances && !resistance.isActiveAndEnabled)
							{
								continue;
							}
							knockback = resistance.ProcessKnockback(knockback, typedDamage.AssociatedDamageType);
						}
					}
				}

				return knockback;
			}
		}
	}
}