using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{

    /// <summary>
    /// 将此组件添加到角色中，它将允许您定义许多表面并将行走和运行声音关联到它们
    /// 它还可以让你在进入或离开这些表面时触发事件
	/// 重要提示：从上到下评估表面。与检测到的当前第一个表面定义相匹配
	/// 地面将被认为是当前的表面。所以确保你的指令是相应的。
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks", "AbilityStartFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Surface Sounds")] 
	public class CharacterSurfaceSounds : CharacterAbility
	{	
		[Serializable]
		public class CharacterSurfaceSoundsItems
		{
			/// an ID to identify this surface in the list. Not used by anything but makes the list more readable
			[Tooltip("在列表中识别此表面的ID。除了使列表更易读外，不用于任何其他用途")]
			public string ID;
			/// the list of layers that identify this surface
			[Tooltip("识别此表面的图层列表")]
			public LayerMask Layers;
			/// whether or not to use a tag to identify this surface or just rely only on the layer
			[Tooltip("是否使用标签来识别此表面，或者仅依赖于图层")]
			public bool UseTag;
			/// if using tags, the Tag that should be on this surface to identify it (on top of the layer)
			[Tooltip("如果使用标签，那么应该在此表面上的标签（在图层之上）来识别它")]
			[MMCondition("UseTag", true)]
			public string Tag;
			/// the sound to use for walking when on this surface
			[Tooltip("当在这个表面上行走时要使用的声音")]
			public AudioClip WalkSound;
			/// the sound to use for running when on this surface
			[Tooltip("当在这个表面上奔跑时要使用的声音")]
			public AudioClip RunSound;
			/// a UnityEvent that will trigger when entering this surface
			[Tooltip("进入此表面时将触发的UnityEvent")]
			public UnityEvent OnEnterSurfaceFeedbacks;
			/// a UnityEvent that will trigger when exiting this surface
			[Tooltip("退出此表面时将触发的UnityEvent")]
			public UnityEvent OnExitSurfaceFeedbacks;
		}

        /// 不同维度的检测可以在（2D或3D物理）上操作
        public enum DimensionModes { TwoD, ThreeD }
        /// 检测应该依赖于定期的光线投射还是由外部脚本驱动（通过SetCurrentSurfaceIndex（int-index）方法）
        public enum SurfaceDetectionModes { Raycast, Script }
        /// 此方法仅用于在能力检查器的开头显示帮助框文本
        public override string HelpBoxText() { return "此组件允许角色，它将允许您定义多个表面，并将行走和跑步声音与它们相关联。 " +
                                                      "它还允许您在进入或退出这些表面时触发事件。" +
                                                      "重要提示：从上到下评估表面。与检测到的当前第一个表面定义相匹配 " +
                                                      "地面将被认为是当前的表面。所以确保你的指令是相应的。"; }

		[Header("List of Surfaces表面列表")] 
		/// a list of surface definitions, defined by a layer, an optional tag, and a walk and run sound. These will be evaluated from top to bottom, first match found becomes the current surface.
		[Tooltip("表面定义的列表，由一个图层、一个可选的标签以及行走和跑步声音定义。这些将从上到下进行评估，找到的第一个匹配项将成为当前表面")]
		public List<CharacterSurfaceSoundsItems> Surfaces;
		
		[Header("Detection检测")]
		/// the different dimensions detection can operate on (either 2D or 3D physics)
		[Tooltip("检测可以操作的不同维度（2D或3D物理）")]
		public DimensionModes DimensionMode = DimensionModes.ThreeD;
		/// whether detection should rely on periodical raycasts or be driven by an external script (via the SetCurrentSurfaceIndex(int index) method)
		[Tooltip("检测是否应依赖于周期性的射线投射或由外部脚本驱动（通过SetCurrentSurfaceIndex(int index)方法）")]
		public SurfaceDetectionModes SurfaceDetectionMode = SurfaceDetectionModes.Raycast;
		/// the length of the raycast to cast to detect surfaces
		[Tooltip("用于检测表面的射线投射的长度")]
		[MMEnumCondition("SurfaceDetectionMode", (int)SurfaceDetectionModes.Raycast)]
		public float RaycastLength = 2f;
		/// the direction of the raycast to cast to detect surfaces
		[Tooltip("用于检测表面的射线投射的方向")] 
		[MMEnumCondition("SurfaceDetectionMode", (int)SurfaceDetectionModes.Raycast)]
		public Vector3 RaycastDirection = Vector3.down;
		/// the frequency (in seconds) at which to cast the raycast to detect surfaces, usually you'll want to space them a bit to save on performance
		[Tooltip("投射射线以检测表面的频率（以秒为单位），通常您会希望将它们间隔一点以节省性能")]
		[MMEnumCondition("SurfaceDetectionMode", (int)SurfaceDetectionModes.Raycast)]
		public float RaycastFrequency = 1f;
		
		[Header("Debug调试")]
		/// The current index of the surface we're on in the Surfaces list
		[Tooltip("我们在“表面”列表中所处表面的当前索引")]
		[MMReadOnly]
		public int CurrentSurfaceIndex = -1;
		
		protected RaycastHit _raycastDownHit;
		protected LayerMask _raycastLayerMask;
		protected float _timeSinceLastCheck = -float.PositiveInfinity;
		protected int _surfaceIndexLastFrame;
		protected CharacterRun _characterRun;
		protected Collider2D _testSurface2D;

        /// <summary>
        /// 在ScriptDriven模式下，可以使用一种方法强制表面索引
        /// </summary>
        /// <param name="index"></param>
        public virtual void SetCurrentSurfaceIndex(int index)
		{
			CurrentSurfaceIndex = index;
		}

        /// <summary>
        /// 在init中，我们获取运行能力并初始化层掩码
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_characterRun = _character.FindAbility<CharacterRun>();
			_surfaceIndexLastFrame = -1;
			foreach (CharacterSurfaceSoundsItems item in Surfaces)
			{
				_raycastLayerMask |= item.Layers;
			}
		}

        /// <summary>
        /// 如果需要，我们会检测每一帧表面，并处理潜在的表面变化
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			DetectSurface();
			HandleSurfaceChange();
		}

        /// <summary>
        /// 如果我们在一个新的表面上，我们交换声音并调用我们的事件
        /// </summary>
        protected virtual void HandleSurfaceChange()
		{
			if (_surfaceIndexLastFrame != CurrentSurfaceIndex)
			{
				if (_surfaceIndexLastFrame >= 0 && _surfaceIndexLastFrame < Surfaces.Count)
				{
					Surfaces[_surfaceIndexLastFrame].OnExitSurfaceFeedbacks?.Invoke();
				}
				Surfaces[CurrentSurfaceIndex].OnEnterSurfaceFeedbacks?.Invoke();
				_characterMovement.AbilityInProgressSfx = Surfaces[CurrentSurfaceIndex].WalkSound;
				_characterMovement.StopAbilityUsedSfx();
				_characterRun.AbilityInProgressSfx = Surfaces[CurrentSurfaceIndex].RunSound;
				_characterRun.StopAbilityUsedSfx();
			}
			_surfaceIndexLastFrame = CurrentSurfaceIndex;
		}

        /// <summary>
        /// 投射光线以检测可能与表面列表的层和标签匹配的表面
        /// </summary>
        protected virtual void DetectSurfaces3D()
		{
			Physics.Raycast(this.transform.position, RaycastDirection, out _raycastDownHit, RaycastLength, _raycastLayerMask);
			if (_raycastDownHit.collider == null)
			{
				return;
			}
			foreach (CharacterSurfaceSoundsItems item in Surfaces)
			{
				if (item.Layers.MMContains(_raycastDownHit.collider.gameObject.layer) && TagsMatch(item.UseTag, item.Tag, _raycastDownHit.collider.gameObject.tag))
				{
					CurrentSurfaceIndex = Surfaces.IndexOf(item);
					return;
				}
			}
		}

        /// <summary>
        ///  测试角色下的点以尝试找到表面，然后将其与表面列表进行比较以找到匹配项 
        /// </summary>
        protected virtual void DetectSurfaces2D()
		{
			_testSurface2D = Physics2D.OverlapPoint((Vector2)_controller2D.ColliderBounds.center, _raycastLayerMask);
			if (_testSurface2D == null)
			{
				return;
			}
			foreach (CharacterSurfaceSoundsItems item in Surfaces)
			{
				if (item.Layers.MMContains(_testSurface2D.gameObject.layer) && TagsMatch(item.UseTag, item.Tag, _testSurface2D.gameObject.tag))
				{
					CurrentSurfaceIndex = Surfaces.IndexOf(item);
					return;
				}
			}
		}

        /// <summary>
        /// 如果标签匹配或我们没有使用标签，则返回true
        /// </summary>
        /// <param name="useTag"></param>
        /// <param name="contactTag"></param>
        /// <param name="surfaceTag"></param>
        /// <returns></returns>
        protected virtual bool TagsMatch(bool useTag, string contactTag, string surfaceTag)
		{
			if (!useTag)
			{
				return true;
			}
			return contactTag == surfaceTag;
		}

        /// <summary>
        /// 检查是否需要表面检测并执行
        /// </summary>
        protected virtual void DetectSurface()
		{
			if (SurfaceDetectionMode == SurfaceDetectionModes.Script)
			{
				return;
			}
			
			if (Time.time - _timeSinceLastCheck < RaycastFrequency)
			{
				return;
			}
			_timeSinceLastCheck = Time.time;

			if (DimensionMode == DimensionModes.ThreeD)
			{
				DetectSurfaces3D();
			}
			else
			{
				DetectSurfaces2D();
			}
		}
	}
}