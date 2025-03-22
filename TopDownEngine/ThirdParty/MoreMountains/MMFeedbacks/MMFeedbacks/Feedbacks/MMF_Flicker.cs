using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈将在播放时使绑定的渲染器按设定的持续时间闪烁（并在停止时恢复其初始颜色）
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈允许您在指定的持续时间内，按指定的次数，以指定的颜色使特定的渲染器（精灵、网格等）闪烁。例如，当角色受到攻击时，这会非常有用（但用途远不止于此！）")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Flicker")]
	public class MMF_Flicker : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
		public override bool EvaluateRequiresSetup() => (BoundRenderer == null);
		public override string RequiredTargetText => BoundRenderer != null ? BoundRenderer.name : "";
		public override string RequiresSetupText => "This feedback requires that a BoundRenderer be set to be able to work properly. You can set one below.";
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => BoundRenderer = FindAutomatedTarget<Renderer>();

        /// 可能的模式
        /// Color : 将控制材质颜色
        /// PropertyName : 将通过名称来定位特定的着色器属性。
        public enum Modes { Color, PropertyName }

		[MMFInspectorGroup("Flicker", true, 61, true)]
		/// the renderer to flicker when played
		[Tooltip("在播放时使渲染器闪烁")]
		public Renderer BoundRenderer;
		/// more renderers to flicker when played
		[Tooltip("在播放时使更多渲染器闪烁")]
		public List<Renderer> ExtraBoundRenderers;
		/// the selected mode to flicker the renderer 
		[Tooltip("选择模式来使渲染器闪烁")]
		public Modes Mode = Modes.Color;
		/// the name of the property to target
		[MMFEnumCondition("Mode", (int)Modes.PropertyName)]
		[Tooltip("要定位的属性名称")]
		public string PropertyName = "_Tint";
		/// the duration of the flicker when getting damage
		[Tooltip("受到伤害时闪烁的持续时间")]
		public float FlickerDuration = 0.2f;
		/// the duration of the period for the flicker
		[Tooltip("闪烁阶段的持续时间")]
		[FormerlySerializedAs("FlickerOctave")]
		public float FlickerPeriod = 0.04f;
		/// the color we should flicker the sprite to 
		[Tooltip("我们应该将精灵闪烁成的颜色")]
		[ColorUsage(true, true)]
		public Color FlickerColor = new Color32(255, 20, 20, 255);
		/// the list of material indexes we want to flicker on the target renderer. If left empty, will only target the material at index 0 
		[Tooltip("我们要在目标渲染器上闪烁的材质索引列表。如果留空，将只针对索引为0的材质")]
		public int[] MaterialIndexes;
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("如果为真，此组件将使用材质属性块，而不是对材质的实例进行操作")] 
		public bool UseMaterialPropertyBlocks = false;
		/// if using material property blocks on a sprite renderer, you'll want to make sure the sprite texture gets passed to the block when updating it. For that, you need to specify your sprite's material's shader's texture property name. If you're not working with a sprite renderer, you can safely ignore this.
		[Tooltip("如果在使用材质属性块时，您需要确保在更新时将精灵纹理传递给该块。为此，您需要在指定您的精灵材质的着色器的纹理属性名称。如果您不是在使用精灵渲染器，则可以安全地忽略这一点。")]
		[MMCondition("UseMaterialPropertyBlocks", true)]
		public string SpriteRendererTextureProperty = "_MainTex";

        /// 此反馈的持续时间就是闪烁的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FlickerDuration); } set { FlickerDuration = value; } }

		protected const string _colorPropertyName = "_Color";
        
		protected int[] _propertyIDs;
		protected bool[] _propertiesFound;
		protected bool _spriteRendererIsNull;
		
		protected Coroutine[] _coroutines;
		protected List<Coroutine[]> _extraCoroutines;
		
		protected Color[] _initialFlickerColors;
		protected List<Color[]> _extraInitialFlickerColors;
		
		protected MaterialPropertyBlock _propertyBlock;
		protected List<MaterialPropertyBlock> _extraPropertyBlocks;
		
		protected SpriteRenderer _spriteRenderer;
		protected List<SpriteRenderer> _spriteRenderers;
		
		protected Texture2D _spriteRendererTexture;
		protected List<Texture2D> _spriteRendererTextures;

        /// <summary>
        /// 在初始化时，我们获取初始颜色和组件
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
            // 初始化材质索引
            if (MaterialIndexes.Length == 0)
			{
				MaterialIndexes = new int[1];
				MaterialIndexes[0] = 0;
			}

			_coroutines = new Coroutine[MaterialIndexes.Length];
			_initialFlickerColors = new Color[MaterialIndexes.Length];
			
			_extraCoroutines = new List<Coroutine[]>();
			_extraInitialFlickerColors = new List<Color[]>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				_extraCoroutines.Add(new Coroutine[MaterialIndexes.Length]);
				_extraInitialFlickerColors.Add(new Color[MaterialIndexes.Length]);
			}
			
			_propertyIDs = new int[MaterialIndexes.Length];
			_propertiesFound = new bool[MaterialIndexes.Length];
			_propertyBlock = new MaterialPropertyBlock();

			AcquireRenderers(owner);
			StoreSpriteRendererTexture();

			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				_propertiesFound[i] = false;
				int index = MaterialIndexes[i];

				if (Active && (BoundRenderer != null))
				{
					if (Mode == Modes.Color)
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(_colorPropertyName) : BoundRenderer.materials[index].HasProperty(_colorPropertyName);
						if (_propertiesFound[i])
						{
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].color : BoundRenderer.materials[index].color;
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].color : renderer.materials[index].color;
							}
						}
					}
					else
					{
						_propertiesFound[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].HasProperty(PropertyName) : BoundRenderer.materials[index].HasProperty(PropertyName); 
						if (_propertiesFound[i])
						{
							_propertyIDs[i] = Shader.PropertyToID(PropertyName);
							_initialFlickerColors[i] = UseMaterialPropertyBlocks ? BoundRenderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : BoundRenderer.materials[index].GetColor(_propertyIDs[i]);
							foreach (Renderer renderer in ExtraBoundRenderers)
							{
								_extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i] = UseMaterialPropertyBlocks ? renderer.sharedMaterials[index].GetColor(_propertyIDs[i]) : renderer.materials[index].GetColor(_propertyIDs[i]);
							}
						}
					}
				}
			}
		}

		protected virtual void AcquireRenderers(MMF_Player owner)
		{
			if (Active && (BoundRenderer == null) && (owner != null))
			{
				if (Owner.gameObject.MMFGetComponentNoAlloc<Renderer>() != null)
				{
					BoundRenderer = owner.GetComponent<Renderer>();
				}
				if (BoundRenderer == null)
				{
					BoundRenderer = owner.GetComponentInChildren<Renderer>();
				}
			}
			if (BoundRenderer == null)
			{
				Debug.LogWarning("[MMFeedbackFlicker] 这个 "+Owner.name+ " 闪烁反馈如果没有绑定的渲染器，它将无法工作。您需要在检查器中指定一个要闪烁的渲染器。");    
			}
			
			_spriteRenderer = BoundRenderer.GetComponent<SpriteRenderer>();
			_spriteRenderers = new List<SpriteRenderer>();
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				if (renderer.GetComponent<SpriteRenderer>() != null)
				{
					_spriteRenderers.Add(renderer.GetComponent<SpriteRenderer>());
				}
			}
			_spriteRendererIsNull = _spriteRenderer == null;
		}

		protected virtual void StoreSpriteRendererTexture()
		{
			if (_spriteRendererIsNull)
			{
				return;
			}
			_spriteRendererTexture = _spriteRenderer.sprite.texture;
			_spriteRendererTextures = new List<Texture2D>();
			for (var index = 0; index < ExtraBoundRenderers.Count; index++)
			{
				_spriteRendererTextures.Add(_spriteRenderers[index].sprite.texture);
			}
		}

        /// <summary>
        /// 在播放时，我们使渲染器闪烁
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (BoundRenderer == null))
			{
				return;
			}
			for (int i = 0; i < MaterialIndexes.Length; i++)
			{
				if (_coroutines[i] != null) { Owner.StopCoroutine(_coroutines[i]); }
				_coroutines[i] = Owner.StartCoroutine(Flicker(BoundRenderer, i, _initialFlickerColors[i], FlickerColor, FlickerPeriod, FeedbackDuration));
				for (var index = 0; index < ExtraBoundRenderers.Count; index++)
				{
					_extraCoroutines[index][i] = Owner.StartCoroutine(Flicker(ExtraBoundRenderers[index], i, _extraInitialFlickerColors[index][i], FlickerColor, FlickerPeriod, FeedbackDuration));
				}
			}
		}

        /// <summary>
        /// 在重置时，我们使渲染器停止闪烁
        /// </summary>
        protected override void CustomReset()
		{
			base.CustomReset();

			if (InCooldown)
			{
				return;
			}

			if (Active && FeedbackTypeAuthorized && (BoundRenderer != null))
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(BoundRenderer, i, _initialFlickerColors[i]);
				}
			}
			
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					SetColor(renderer, i, _extraInitialFlickerColors[ExtraBoundRenderers.IndexOf(renderer)][i]);
				}
			}
		}
		
		protected virtual void SetStoredSpriteRendererTexture(Renderer renderer, MaterialPropertyBlock block)
		{
			if (_spriteRendererIsNull)
			{
				return;
			}

			if (renderer == BoundRenderer)
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTexture);	
			}
			else
			{
				block.SetTexture(SpriteRendererTextureProperty, _spriteRendererTextures[ExtraBoundRenderers.IndexOf(renderer)]);
			}
		}

		public virtual IEnumerator Flicker(Renderer renderer, int materialIndex, Color initialColor, Color flickerColor, float flickerSpeed, float flickerDuration)
		{
			if (renderer == null)
			{
				yield break;
			}

			if (!_propertiesFound[materialIndex])
			{
				yield break;
			}

			if (initialColor == flickerColor)
			{
				yield break;
			}

			float flickerStop = FeedbackTime + flickerDuration;
			IsPlaying = true;
			
			StoreSpriteRendererTexture();
            
			while (FeedbackTime < flickerStop)
			{
				SetColor(renderer, materialIndex, flickerColor);
				yield return WaitFor(flickerSpeed);
				SetColor(renderer, materialIndex, initialColor);
				yield return WaitFor(flickerSpeed);
			}

			SetColor(renderer, materialIndex, initialColor);
			IsPlaying = false;
		}


		protected virtual void SetColor(Renderer renderer, int materialIndex, Color color)
		{
			if (!_propertiesFound[materialIndex])
			{
				return;
			}
            
			if (Mode == Modes.Color)
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_colorPropertyName, color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].color = color;
				}
			}
			else
			{
				if (UseMaterialPropertyBlocks)
				{
					renderer.GetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
					_propertyBlock.SetColor(_propertyIDs[materialIndex], color);
					SetStoredSpriteRendererTexture(renderer, _propertyBlock);
					renderer.SetPropertyBlock(_propertyBlock, MaterialIndexes[materialIndex]);
				}
				else
				{
					renderer.materials[MaterialIndexes[materialIndex]].SetColor(_propertyIDs[materialIndex], color);
				}
			}            
		}
        
		/// <summary>
		/// 停止这个反馈
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			IsPlaying = false;
			for (int i = 0; i < _coroutines.Length; i++)
			{
				if (_coroutines[i] != null)
				{
					Owner.StopCoroutine(_coroutines[i]);    
				}
				_coroutines[i] = null;  
			}
			foreach (Renderer renderer in ExtraBoundRenderers)
			{
				for (int i = 0; i < MaterialIndexes.Length; i++)
				{
					if (_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] != null)
					{
						Owner.StopCoroutine(_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i]);
					}
					_extraCoroutines[ExtraBoundRenderers.IndexOf(renderer)][i] = null;
				}
			}
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			CustomReset();
		}
	}
}