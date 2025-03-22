using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// This feedback lets you trigger the appearance of a floating text, that will reflect the damage done to the target Health component.
	/// This requires that a MMFloatingTextSpawner be correctly setup in the scene, otherwise nothing will happen.
	/// To do so, create a new empty object, add a MMFloatingTextSpawner to it. Drag (at least) one MMFloatingText prefab into its PooledSimpleMMFloatingText slot.
	/// You'll find such prefabs already made in the MMTools/Tools/MMFloatingText/Prefabs folder, but feel free to create your own.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("UI/TopDown Engine Floating Text")]
	[FeedbackHelp("此反馈允许您触发一个浮动文字的出现，该文字将反映目标生命值组件所受的伤害." +
                  "这需要场景中正确设置一个 MMFloatingTextSpawner，否则将不会发生任何事情." +
                  "为此，请执行以下步骤：1.创建一个新空对象。2.在该对象上添加一个 MMFloatingTextSpawner 组件。3.将至少一个 MMFloatingText 预制件拖入其 PooledSimpleMMFloatingText 插槽中。" +
                  "您可以在 MMTools/Tools/MMFloatingText/Prefabs 文件夹中找到已经制作好的此类预制件，但也可以自由创建您自己的预制件")]
	public class MMF_TopDownEngineFloatingText : MMF_FloatingText
	{
		[MMFInspectorGroup("TopDown Engine Settings", true, 17)]

		/// the Health component where damage data should be read
		[Tooltip("应该读取伤害数据的生命值组件")]
		public Health TargetHealth;
		/// the number formatting of your choice, 
		/// check https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings?redirectedfrom=MSDN#NFormatString for examples
		[Tooltip("您选择的数字格式，也可以留空")]
		public string Formatting = "";

		[MMFInspectorGroup("Direction", true, 18)]
		/// whether or not the direction of the damage should impact the direction of the floating text 
		[Tooltip("伤害的方向是否应该影响浮动文字的方向")]
		public bool DamageDirectionImpactsTextDirection = true;
		/// the multiplier to apply to the damage direction. Usually you'll want it to be less than 1. With a value of 0.5, a character being hit from the left will spawn a floating text at a 45° up/right angle
		[Tooltip("应用于伤害方向的乘数。通常，您会希望它小于 1。如果值为 0.5，那么当角色从左侧被击中时，浮动文字将会以 45°向上/向右的角度生成")]
		public float DamageDirectionMultiplier = 0.5f;

        /// <summary>
        /// 在播放时，我们请求生成一个浮动文字
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (Active)
			{
				if (TargetHealth == null)
				{
					return;
				}

				if (DamageDirectionImpactsTextDirection)
				{
					Direction = TargetHealth.LastDamageDirection.normalized * DamageDirectionMultiplier;
				}

				Value = ApplyRounding(TargetHealth.LastDamage).ToString(Formatting);
				
				_playPosition = (PositionMode == PositionModes.FeedbackPosition) ? Owner.transform.position : TargetTransform.position;
				MMFloatingTextSpawnEvent.Trigger(ChannelData, _playPosition, Value, Direction, Intensity, ForceLifetime, Lifetime, ForceColor, AnimateColorGradient);
			}
		}
	}
}