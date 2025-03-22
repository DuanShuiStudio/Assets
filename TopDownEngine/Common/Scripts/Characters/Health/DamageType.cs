using UnityEngine;

namespace MoreMountains.TopDownEngine
{
	public enum DamageTypeModes { BaseDamage, TypedDamage }
    /// <summary>
    /// 一个可编写脚本的对象，您可以从中创建资产，以识别伤害类型
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/DamageType", fileName = "DamageType")]
	public class DamageType : ScriptableObject
	{
	}    
}