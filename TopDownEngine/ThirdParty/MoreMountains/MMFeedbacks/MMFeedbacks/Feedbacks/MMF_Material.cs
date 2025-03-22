using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("")]
	[FeedbackHelp("每次播放此反馈时，都将允许你更改目标渲染器的材质")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Material")]
	public class MMF_Material : MMF_Feedback
	{
        /// 在检查器中设置此反馈的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get => MMFeedbacksInspectorColors.RendererColor; }
		public override bool EvaluateRequiresSetup() => (TargetRenderer == null);
		public override string RequiredTargetText => TargetRenderer != null ? TargetRenderer.name : "";
		public override string RequiresSetupText => "此反馈需要设置一个目标渲染器才能正常工作。你可以在下面设置一个";
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetRenderer = FindAutomatedTarget<Renderer>();

        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 此反馈的持续时间就是震动的时长
        public override float FeedbackDuration { get { return (InterpolateTransition) ? TransitionDuration : 0f; } set { if (InterpolateTransition) { TransitionDuration = value; } } }

        /// 切换材质的可能方法
        public enum Methods { Sequential, Random }

		[MMFInspectorGroup("Target Material", true, 61, true)]
		/// the renderer to change material on
		[Tooltip("要更改材质的渲染器")]
		public Renderer TargetRenderer;
		/// the list of material indexes we want to change on the target renderer. If left empty, will only target the material at index 0 
		[FormerlySerializedAs("MaterialIndexes")] [Tooltip("我们要在目标渲染器上更改的材质索引列表。如果留空，将只针对索引 0 处的材质")]
		public int[] RendererMaterialIndexes;
        
		[MMFInspectorGroup("Material Change", true, 33)]
		/// the selected method
		[Tooltip("选择的方法")]
		public Methods Method;
		/// whether or not the sequential order should loop
		[MMFEnumCondition("Method", (int)Methods.Sequential)]
		[Tooltip("是否按顺序循环")]
		public bool Loop = true;
		/// whether or not to always pick a new material in random mode
		[MMFEnumCondition("Method", (int)Methods.Random)]        
		[Tooltip("是否在随机模式下总是选择新材质")]
		public bool AlwaysNewMaterial = true;
		/// the initial index to start with
		[Tooltip("开始的初始索引")]
		public int InitialIndex = 0;
		/// the list of materials to pick from
		[Tooltip("可选择的材质列表")]
		public List<Material> Materials;

		[MMFInspectorGroup("Interpolation", true, 35)]
        /// 是否在两种材质之间插值
        /// 重要提示：这只适用于共享相同着色器和纹理的材质(参考 https://docs.unity3d.com/ScriptReference/Material.Lerp.html)
        public bool InterpolateTransition = false;
        /// 插值的持续时间（以秒为单位）
        public float TransitionDuration = 1f;
        /// 用于插值过渡的动画曲线
        public AnimationCurve TransitionCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

		public virtual float GetTime() { return (ComputedTimescaleMode == TimescaleModes.Scaled) ? Time.time : Time.unscaledTime; }
		public virtual float GetDeltaTime() { return (ComputedTimescaleMode == TimescaleModes.Scaled) ? Time.deltaTime : Time.unscaledDeltaTime; }
        
		protected int _currentIndex;
		protected float _startedAt;
		protected Coroutine[] _coroutines;
		protected Material[] _tempMaterials;
		protected Material[] _initialMaterials;

        /// <summary>
        /// 在初始化时，获取当前索引
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			InitializeMaterials();
		}

		protected virtual void InitializeMaterials()
		{
			if (TargetRenderer == null)
			{
				return;
			}
			_currentIndex = InitialIndex;
			_tempMaterials = new Material[TargetRenderer.materials.Length];
			_initialMaterials = new Material[TargetRenderer.materials.Length];
			for (int i = 0; i < _initialMaterials.Length; i++)
			{
				_initialMaterials[i] = new Material(TargetRenderer.materials[i]);
			}
			
			if (RendererMaterialIndexes == null)
			{
				RendererMaterialIndexes = new int[1];
			}
			if (RendererMaterialIndexes.Length == 0)
			{
				RendererMaterialIndexes = new int[1];
				RendererMaterialIndexes[0] = 0;
			}
			_coroutines = new Coroutine[RendererMaterialIndexes.Length];
		}

        /// <summary>
        /// 在播放反馈时，如果可能的话，我们更改材质
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Materials.Count == 0)
			{
				Debug.LogError("[MMFeedbackMaterial on " + Owner.name + "] 材质数组为空.");
				return;
			}

			int newIndex = DetermineNextIndex();

			if (Materials[newIndex] == null)
			{
				Debug.LogError("[MMFeedbackMaterial on " + Owner.name + "] 尝试切换到空材质.");
				return;
			}

			if (InterpolateTransition)
			{
				for (int i = 0; i < RendererMaterialIndexes.Length; i++)
				{
					if (_coroutines[i] != null) { Owner.StopCoroutine(_coroutines[i]); }
					_coroutines[i] = Owner.StartCoroutine(TransitionMaterial(TargetRenderer.materials[RendererMaterialIndexes[i]], Materials[newIndex], RendererMaterialIndexes[i]));
				}
			}
			else
			{
				ApplyMaterial(Materials[newIndex]);
			}            
		}

        /// <summary>
        /// 将新材质应用于所有索引
        /// </summary>
        /// <param name="material"></param>
        protected virtual void ApplyMaterial(Material material)
		{
			_tempMaterials = TargetRenderer.materials;
			for (int i = 0; i < RendererMaterialIndexes.Length; i++)
			{
				_tempMaterials[RendererMaterialIndexes[i]] = material;
			}
			TargetRenderer.materials = _tempMaterials;
		}

        /// <summary>
        /// 为所有索引进行目标材质的线性插值
        /// </summary>
        /// <param name="fromMaterial"></param>
        /// <param name="toMaterial"></param>
        /// <param name="t"></param>
        /// <param name="materialIndex"></param>
        protected virtual void LerpMaterial(Material fromMaterial, Material toMaterial, float t, int materialIndex)
		{
			_tempMaterials = TargetRenderer.materials;
			for (int i = 0; i < RendererMaterialIndexes.Length; i++)
			{
				_tempMaterials[materialIndex].Lerp(fromMaterial, toMaterial, t);
			}
			TargetRenderer.materials = _tempMaterials;
		}

        /// <summary>
        /// 用于在材质之间插值的协程
        /// </summary>
        /// <param name="originalMaterial"></param>
        /// <param name="newMaterial"></param>
        /// <returns></returns>
        protected virtual IEnumerator TransitionMaterial(Material originalMaterial, Material newMaterial, int materialIndex)
		{
			IsPlaying = true;
			_startedAt = GetTime();
			while (GetTime() - _startedAt < TransitionDuration)
			{
				float time = MMFeedbacksHelpers.Remap(GetTime() - _startedAt, 0f, TransitionDuration, 0f, 1f);
				float t = TransitionCurve.Evaluate(time);
                
				LerpMaterial(originalMaterial, newMaterial, t, materialIndex);
				yield return null;
			}
			float finalt = TransitionCurve.Evaluate(1f);
			LerpMaterial(originalMaterial, newMaterial, finalt, materialIndex);
			IsPlaying = false;
		}

        /// <summary>
        /// 确定要选择的新材质
        /// </summary>
        /// <returns></returns>
        protected virtual int DetermineNextIndex()
		{
			switch(Method)
			{
				case Methods.Random:
					int random = Random.Range(0, Materials.Count);
					if (AlwaysNewMaterial)
					{
						while (_currentIndex == random)
						{
							random = Random.Range(0, Materials.Count);
						}
					}
					_currentIndex = random;
					return _currentIndex;                    

				case Methods.Sequential:
					_currentIndex++;
					if (_currentIndex >= Materials.Count)
					{
						_currentIndex = Loop ? 0 : _currentIndex - 1;
					}
					return _currentIndex;
			}
			return 0;
		}

        /// <summary>
        /// 如果需要，在停止时停止过渡
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
			if (Active && FeedbackTypeAuthorized && (_coroutines != null))
			{
				IsPlaying = false;
				for (int i = 0; i < RendererMaterialIndexes.Length; i++)
				{
					if (_coroutines[i] != null)
					{
						Owner.StopCoroutine(_coroutines[i]);    
					}
					_coroutines[i] = null;    
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

			TargetRenderer.materials = _initialMaterials;
			InitializeMaterials();
		}
	}
}