using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoreMountains.Tools;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个自定义编辑器，显示可折叠的MMFeedbacks列表，一个下拉菜单用于添加更多反馈，以及在运行时测试反馈的测试按钮
    /// </summary>
    [CustomEditor(typeof(MMFeedbacks))]
	public class MMFeedbacksEditor : Editor
	{
        /// <summary>
        /// 一种用于存储类型和名称的数据结构
        /// </summary>
        public class FeedbackTypePair
		{
			public System.Type FeedbackType;
			public string FeedbackName;
		}

        /// <summary>
        /// 一个辅助类，用于复制和粘贴反馈属性
        /// </summary>
        static class FeedbackCopy
		{
			// Single Copy --------------------------------------------------------------------

			static public System.Type Type { get; private set; }
			static List<SerializedProperty> Properties = new List<SerializedProperty>();
            
			public static string[] IgnoreList = new string[]
			{
				"m_ObjectHideFlags",
				"m_CorrespondingSourceObject",
				"m_PrefabInstance",
				"m_PrefabAsset",
				"m_GameObject",
				"m_Enabled",
				"m_EditorHideFlags",
				"m_Script",
				"m_Name",
				"m_EditorClassIdentifier"
			};

			static public void Copy(SerializedObject serializedObject)
			{
				Type = serializedObject.targetObject.GetType();
				Properties.Clear();

				SerializedProperty property = serializedObject.GetIterator();
				property.Next(true);
				do
				{
					if (!IgnoreList.Contains(property.name))
					{
						Properties.Add(property.Copy());
					}
				} while (property.Next(false));
			}

			static public void Paste(SerializedObject target)
			{
				if (target.targetObject.GetType() == Type)
				{
					for (int i = 0; i < Properties.Count; i++)
					{
						target.CopyFromSerializedProperty(Properties[i]);
					}
				}
			}

			static public bool HasCopy()
			{
				return Properties != null && Properties.Count > 0;
			}

			// Multiple Copy ----------------------------------------------------------

			static public void CopyAll(MMFeedbacks sourceFeedbacks)
			{
				MMFeedbacksConfiguration.Instance._mmFeedbacks = sourceFeedbacks;
			}

			static public bool HasMultipleCopies()
			{
				return (MMFeedbacksConfiguration.Instance._mmFeedbacks != null);
			}

			static public void PasteAll(MMFeedbacksEditor targetEditor)
			{
				var sourceFeedbacks = new SerializedObject(MMFeedbacksConfiguration.Instance._mmFeedbacks);
				SerializedProperty feedbacks = sourceFeedbacks.FindProperty("Feedbacks");

				for (int i = 0; i < feedbacks.arraySize; i++)
				{
					MMFeedback arrayFeedback = (feedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);

					FeedbackCopy.Copy(new SerializedObject(arrayFeedback));
					MMFeedback newFeedback = targetEditor.AddFeedback(arrayFeedback.GetType());
					SerializedObject serialized = new SerializedObject(newFeedback);
					serialized.Update();
					FeedbackCopy.Paste(serialized);
					serialized.ApplyModifiedProperties();
				}
				MMFeedbacksConfiguration.Instance._mmFeedbacks = null;
			}
		}

		protected MMFeedbacks _targetMMFeedbacks;
		protected SerializedProperty _mmfeedbacks;
		protected SerializedProperty _mmfeedbacksInitializationMode;
		protected SerializedProperty _mmfeedbacksSafeMode;
		protected SerializedProperty _mmfeedbacksAutoPlayOnStart;
		protected SerializedProperty _mmfeedbacksAutoPlayOnEnable;
		protected SerializedProperty _mmfeedbacksDirection;
		protected SerializedProperty _mmfeedbacksFeedbacksIntensity;
		protected SerializedProperty _mmfeedbacksAutoChangeDirectionOnEnd;
		protected SerializedProperty _mmfeedbacksDurationMultiplier;
		protected SerializedProperty _mmfeedbacksDisplayFullDurationDetails;
		protected SerializedProperty _mmfeedbacksCooldownDuration;
		protected SerializedProperty _mmfeedbacksInitialDelay;
		protected SerializedProperty _mmfeedbacksCanPlay;
		protected SerializedProperty _mmfeedbacksCanPlayWhileAlreadyPlaying;
		protected SerializedProperty _mmfeedbacksEvents;
		protected SerializedProperty _mmfeedbacksChanceToPlay;
		protected bool _canDisplayInspector = true;
        
		protected Dictionary<MMFeedback, Editor> _editors;
		protected List<FeedbackTypePair> _typesAndNames = new List<FeedbackTypePair>();
		protected string[] _typeDisplays;
		protected int _draggedStartID = -1;
		protected int _draggedEndID = -1;
		private static bool _debugView = false;
		protected Color _originalBackgroundColor;
		protected Color _scriptDrivenBoxColor;
		protected Texture2D _scriptDrivenBoxBackgroundTexture;
		protected Color _scriptDrivenBoxColorFrom = new Color(1f,0f,0f,1f);
		protected Color _scriptDrivenBoxColorTo = new Color(0.7f,0.1f,0.1f,1f);
		protected Color _playButtonColor = new Color32(193, 255, 2, 255);
		private static bool _settingsMenuDropdown;
		protected GUIStyle _directionButtonStyle;
		protected GUIStyle _playingStyle;

        /// <summary>
        /// 启用时，获取属性并初始化“添加反馈”下拉菜单的内容
        /// </summary>
        void OnEnable()
		{
            // 获取属性
            _targetMMFeedbacks = target as MMFeedbacks;
			_mmfeedbacks = serializedObject.FindProperty("Feedbacks");
			_mmfeedbacksInitializationMode = serializedObject.FindProperty("InitializationMode");
			_mmfeedbacksSafeMode = serializedObject.FindProperty("SafeMode");
			_mmfeedbacksAutoPlayOnStart = serializedObject.FindProperty("AutoPlayOnStart");
			_mmfeedbacksAutoPlayOnEnable = serializedObject.FindProperty("AutoPlayOnEnable");
			_mmfeedbacksDirection = serializedObject.FindProperty("Direction");
			_mmfeedbacksAutoChangeDirectionOnEnd = serializedObject.FindProperty("AutoChangeDirectionOnEnd");
			_mmfeedbacksDurationMultiplier = serializedObject.FindProperty("DurationMultiplier");
			_mmfeedbacksDisplayFullDurationDetails = serializedObject.FindProperty("DisplayFullDurationDetails");
			_mmfeedbacksCooldownDuration = serializedObject.FindProperty("CooldownDuration");
			_mmfeedbacksInitialDelay = serializedObject.FindProperty("InitialDelay");
			_mmfeedbacksCanPlay = serializedObject.FindProperty("CanPlay");
			_mmfeedbacksCanPlayWhileAlreadyPlaying = serializedObject.FindProperty("CanPlayWhileAlreadyPlaying");
			_mmfeedbacksFeedbacksIntensity = serializedObject.FindProperty("FeedbacksIntensity");
			_mmfeedbacksChanceToPlay = serializedObject.FindProperty("ChanceToPlay");

			_mmfeedbacksEvents = serializedObject.FindProperty("Events");

            // 存储GUI背景颜色
            _originalBackgroundColor = GUI.backgroundColor;

            // 修复例程，用于捕获由于Unity序列化问题而可能遗漏的反馈。
            RepairRoutine();

            // 创建编辑器
            _editors = new Dictionary<MMFeedback, Editor>();
			for (int i = 0; i < _mmfeedbacks.arraySize; i++)
			{
				AddEditor(_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);
			}

            // 检索可用的反馈
            List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where assemblyType.IsSubclassOf(typeof(MMFeedback))
				select assemblyType).ToList();

            // 从类型创建显示列表
            List<string> typeNames = new List<string>();
			for (int i = 0; i < types.Count; i++)
			{
				FeedbackTypePair newType = new FeedbackTypePair();
				newType.FeedbackType = types[i];
				newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
				if (newType.FeedbackName == "MMFeedbackBase")
				{
					continue;
				}
				_typesAndNames.Add(newType);
			}

			_typesAndNames = _typesAndNames.OrderBy(t => t.FeedbackName).ToList();
            
			typeNames.Add("Add new feedback...");
			for (int i = 0; i < _typesAndNames.Count; i++)
			{
				typeNames.Add(_typesAndNames[i].FeedbackName);
			}

			_typeDisplays = typeNames.ToArray();

			_directionButtonStyle = new GUIStyle();
			_directionButtonStyle.border.left = 0;
			_directionButtonStyle.border.right = 0;
			_directionButtonStyle.border.top = 0;
			_directionButtonStyle.border.bottom = 0;

			_playingStyle = new GUIStyle();
			_playingStyle.normal.textColor = Color.yellow; 

		}

        /// <summary>
        /// 如果需要，调用修复例程
        /// </summary>
        protected virtual void RepairRoutine()
		{
			_targetMMFeedbacks = target as MMFeedbacks;
			if ((_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.EditorOnly) || (_targetMMFeedbacks.SafeMode == MMFeedbacks.SafeModes.Full))
			{
				_targetMMFeedbacks.AutoRepair();
			}
			serializedObject.ApplyModifiedProperties();
		}

        /// <summary>
        /// 绘制检查器，包括帮助框、初始化模式选择、反馈列表、反馈选择和测试按钮
        /// </summary>
        public override void OnInspectorGUI()
		{
			if (!_canDisplayInspector)
			{
				return;
			}
            
			var e = Event.current;
			serializedObject.Update();
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.Space();

			if (!MMFeedbacks.GlobalMMFeedbacksActive)
			{
				Color baseColor = GUI.color;
				GUI.color = Color.red;
				EditorGUILayout.HelpBox("所有MMFeedbacks，包括这个，目前都被禁用了。这是通过脚本完成的，通过更改MMFeedbacks.GlobalMMFeedbacksActive布尔值来实现的。目前这个值被设置为false。将其设置回true将允许MMFeedbacks再次播放。", MessageType.Warning);
				EditorGUILayout.Space();
				GUI.color = baseColor;
			}

			if (MMFeedbacksConfiguration.Instance.ShowInspectorTips)
			{
				EditorGUILayout.HelpBox("MMFeedbacks组件随着v3.0中MMF Player的引入已被弃用" +
                                        "MMF播放器提高了性能，让你可以保留运行时的更改，并且还有更多功能！而且它的工作原理就像MMFeedbacks一样 " +
                                        "随着v4.0的发布，MF播放器现在已从Feel中完全移除并逐步淘汰。", MessageType.Warning);    
			}

			Rect helpBoxRect = GUILayoutUtility.GetLastRect();

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Modified Feedback Manager");
			}
            
			// Settings dropdown -------------------------------------------------------------------------------------

			_settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, "Settings", true, EditorStyles.foldout);
			if (_settingsMenuDropdown)
			{
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Initialization", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksSafeMode);
				EditorGUILayout.PropertyField(_mmfeedbacksInitializationMode);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnStart);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoPlayOnEnable);
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Direction", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksDirection);
				EditorGUILayout.PropertyField(_mmfeedbacksAutoChangeDirectionOnEnd);
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Intensity", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksFeedbacksIntensity);    
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksDurationMultiplier);
				EditorGUILayout.PropertyField(_mmfeedbacksDisplayFullDurationDetails);
				EditorGUILayout.PropertyField(_mmfeedbacksCooldownDuration);
				EditorGUILayout.PropertyField(_mmfeedbacksInitialDelay);
				EditorGUILayout.PropertyField(_mmfeedbacksChanceToPlay);
                
				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Play Conditions", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksCanPlay);
				EditorGUILayout.PropertyField(_mmfeedbacksCanPlayWhileAlreadyPlaying);

				EditorGUILayout.Space(10);
				EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
				EditorGUILayout.PropertyField(_mmfeedbacksEvents);

				if (!Application.isPlaying)
				{
					EditorGUILayout.Space(10);
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("Generate MMF_Player"))
					{
						this.ConvertToMMF_Player(true);
					}
					if (GUILayout.Button("Convert to MMF_Player"))
					{
						this.ConvertToMMF_Player(false);
					}
					EditorGUILayout.EndHorizontal();    
				}
			}

			// Duration ----------------------------------------------------------------------------------------------
            
			float durationRectWidth = 70f;
			Rect durationRect = new Rect(helpBoxRect.xMax - durationRectWidth, helpBoxRect.yMax + 6, durationRectWidth, 17f);
			durationRect.xMin = helpBoxRect.xMax - durationRectWidth;
			durationRect.xMax = helpBoxRect.xMax;
            
			float playingRectWidth = 70f;
			Rect playingRect = new Rect(helpBoxRect.xMax - playingRectWidth - durationRectWidth, helpBoxRect.yMax + 6, playingRectWidth, 17f);
			playingRect.xMin = helpBoxRect.xMax - durationRectWidth- playingRectWidth;
			playingRect.xMax = helpBoxRect.xMax;

			// Direction ----------------------------------------------------------------------------------------------

			float directionRectWidth = 16f;
			Rect directionRect = new Rect(helpBoxRect.xMax - directionRectWidth, helpBoxRect.yMax + 5, directionRectWidth, 17f);
			directionRect.xMin = helpBoxRect.xMax - directionRectWidth;
			directionRect.xMax = helpBoxRect.xMax;

			if ((target as MMFeedbacks).IsPlaying)
			{
				GUI.Label(playingRect, "[PLAYING] ", _playingStyle);    
			}
            
			GUI.Label(durationRect, "["+_targetMMFeedbacks.TotalDuration.ToString("F2")+"s]");

			if (_targetMMFeedbacks.Direction == MMFeedbacks.Directions.BottomToTop)
			{
				Texture arrowUpIcon = Resources.Load("FeelArrowUp") as Texture;
				GUIContent directionIcon = new GUIContent(arrowUpIcon);

				if (GUI.Button(directionRect, directionIcon, _directionButtonStyle))
				{
					_targetMMFeedbacks.Revert();
				}
			}
			else
			{
				Texture arrowDownIcon = Resources.Load("FeelArrowDown") as Texture;
				GUIContent directionIcon = new GUIContent(arrowDownIcon);

				if (GUI.Button(directionRect, directionIcon, _directionButtonStyle))
				{
					_targetMMFeedbacks.Revert();
				}
			}

			// Draw list ------------------------------------------------------------------------------------------

			MMFeedbackStyling.DrawSection("Feedbacks");

			if (!_canDisplayInspector)
			{
				return;
			}
            
			for (int i = 0; i < _mmfeedbacks.arraySize; i++)
			{
				if (!_canDisplayInspector)
				{
					return;
				}
                
				MMFeedbackStyling.DrawSplitter();

				SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(i);

				// Failsafe but should not happen
				if (property.objectReferenceValue == null)
				{
					continue;
				}                    

				// Retrieve feedback

				MMFeedback feedback = property.objectReferenceValue as MMFeedback;
				feedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;
                
				Undo.RecordObject(feedback, "Modified Feedback");

				// Draw header

				int id = i;
				bool isExpanded = property.isExpanded;
				string label = feedback.Label;
				bool pause = false;

				if (feedback.Pause != null)
				{
					pause = true;
				}

				Rect headerRect = MMFeedbackStyling.DrawHeader(
					ref isExpanded,
					ref feedback.Active,
					label,
					feedback.FeedbackColor,
					(GenericMenu menu) =>
					{
						if (Application.isPlaying)
							menu.AddItem(new GUIContent("Play"), false, () => PlayFeedback(id));
						else
							menu.AddDisabledItem(new GUIContent("Play"));
						menu.AddSeparator(null);
						//menu.AddItem(new GUIContent("Reset"), false, () => ResetFeedback(id));
						menu.AddItem(new GUIContent("Remove"), false, () => RemoveFeedback(id));
						menu.AddSeparator(null);
						menu.AddItem(new GUIContent("Copy"), false, () => CopyFeedback(id));
						if (FeedbackCopy.HasCopy() && FeedbackCopy.Type == feedback.GetType())
							menu.AddItem(new GUIContent("Paste"), false, () => PasteFeedback(id));
						else
							menu.AddDisabledItem(new GUIContent("Paste"));
					},
					feedback.FeedbackStartedAt,
					feedback.FeedbackDuration,
					feedback.TotalDuration,
					feedback.Timing,
					pause,
					_targetMMFeedbacks 
				);

                // 检查我们是否开始拖动此反馈

                switch (e.type)
				{
					case EventType.MouseDown:
						if (headerRect.Contains(e.mousePosition))
						{
							_draggedStartID = i;
							e.Use();
						}
						break;
					default:
						break;
				}

                // 如果反馈正在被拖动，则绘制蓝色矩形

                if (_draggedStartID == i && headerRect != Rect.zero)
				{
					Color color = new Color(0, 1, 1, 0.2f);
					EditorGUI.DrawRect(headerRect, color);
				}

                // 如果在拖动一个反馈时悬停在其顶部，请检查该反馈应放置在顶部还是底部。

                if (headerRect.Contains(e.mousePosition))
				{
					if (_draggedStartID >= 0)
					{
						_draggedEndID = i;

						Rect headerSplit = headerRect;
						headerSplit.height *= 0.5f;
						headerSplit.y += headerSplit.height;
						if (headerSplit.Contains(e.mousePosition))
							_draggedEndID = i + 1;
					}
				}

                // 如果展开，绘制反馈编辑器

                property.isExpanded = isExpanded;
				if (isExpanded)
				{
					EditorGUI.BeginDisabledGroup(!feedback.Active);

					string helpText = FeedbackHelpAttribute.GetFeedbackHelpText(feedback.GetType());
                    
					if ( (!string.IsNullOrEmpty(helpText)) && (MMFeedbacksConfiguration.Instance.ShowInspectorTips))
					{
						GUIStyle style = new GUIStyle(EditorStyles.helpBox);
						style.richText = true;
						float newHeight = style.CalcHeight(new GUIContent(helpText), EditorGUIUtility.currentViewWidth);
						EditorGUILayout.LabelField(helpText, style);
					}                    

					EditorGUILayout.Space();

					if (!_editors.ContainsKey(feedback))
					{
						AddEditor(feedback);
					}

					Editor editor = _editors[feedback];
					CreateCachedEditor(feedback, feedback.GetType(), ref editor);

					editor.OnInspectorGUI();

					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();

					EditorGUI.BeginDisabledGroup(!Application.isPlaying);
					EditorGUILayout.BeginHorizontal();
					{
						if (GUILayout.Button("Play", EditorStyles.miniButtonMid))
						{
							PlayFeedback(id);
						}
						if (GUILayout.Button("Stop", EditorStyles.miniButtonMid))
						{
							StopFeedback(id);
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUI.EndDisabledGroup();

					EditorGUILayout.Space();
					EditorGUILayout.Space();
				}
			}

            // 绘制添加新项目

            if (_mmfeedbacks.arraySize > 0)
			{
				MMFeedbackStyling.DrawSplitter();
			}

			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			{
                // 反馈列表

                int newItem = EditorGUILayout.Popup(0, _typeDisplays) - 1;
				if (newItem >= 0)
				{
					AddFeedback(_typesAndNames[newItem].FeedbackType);
				}

                // 粘贴反馈副本作为新项目

                if (FeedbackCopy.HasCopy())
				{
					if (GUILayout.Button("Paste as new", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x)))
					{
						PasteAsNew();
					}                        
				}

				if (FeedbackCopy.HasMultipleCopies())
				{
					if (GUILayout.Button("Paste all as new", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste all as new")).x)))
					{
						PasteAllAsNew();
					}                        
				}
			}

			if (!FeedbackCopy.HasMultipleCopies())
			{
				if (GUILayout.Button("Copy all", EditorStyles.miniButton, GUILayout.Width(EditorStyles.miniButton.CalcSize(new GUIContent("Paste as new")).x)))
				{
					CopyAll();
				}
			}                

			EditorGUILayout.EndHorizontal();

            // 重新排序

            if (_draggedStartID >= 0 && _draggedEndID >= 0)
			{
				if (_draggedEndID != _draggedStartID)
				{
					if (_draggedEndID > _draggedStartID)
						_draggedEndID--;
					_mmfeedbacks.MoveArrayElement(_draggedStartID, _draggedEndID);
					_draggedStartID = _draggedEndID;
				}
			}

			if (_draggedStartID >= 0 || _draggedEndID >= 0)
			{
				switch (e.type)
				{
					case EventType.MouseUp:
						_draggedStartID = -1;
						_draggedEndID = -1;
						e.Use();
						break;
					default:
						break;
				}
			}

            // 清理

            bool wasRemoved = false;
			for (int i = _mmfeedbacks.arraySize - 1; i >= 0; i--)
			{
				if (_mmfeedbacks.GetArrayElementAtIndex(i).objectReferenceValue == null)
				{
					wasRemoved = true;
					_mmfeedbacks.DeleteArrayElementAtIndex(i);
				}
			}

			if (wasRemoved)
			{
				GameObject gameObject = (target as MMFeedbacks).gameObject;
				foreach (var c in gameObject.GetComponents<Component>())
				{
					if (c != null)
					{
						c.hideFlags = HideFlags.None;    
					}
				}
			}

            // 应用更改

            serializedObject.ApplyModifiedProperties();

            // 绘制调试


            MMFeedbackStyling.DrawSection("All Feedbacks Debug");

            // 测试按钮

            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
			EditorGUILayout.BeginHorizontal();
			{
				// initialize button
				if (GUILayout.Button("Initialize", EditorStyles.miniButtonLeft))
				{
					(target as MMFeedbacks).Initialization();
				}

				// play button
				_originalBackgroundColor = GUI.backgroundColor;
				GUI.backgroundColor = _playButtonColor;
				if (GUILayout.Button("Play", EditorStyles.miniButtonMid))
				{
					(target as MMFeedbacks).PlayFeedbacks();
				}
				GUI.backgroundColor = _originalBackgroundColor;
                
				// pause button
				if ((target as MMFeedbacks).ContainsLoop)
				{
					if (GUILayout.Button("Pause", EditorStyles.miniButtonMid))
					{
						(target as MMFeedbacks).PauseFeedbacks();
					}   
				}
                
				// stop button
				if (GUILayout.Button("Stop", EditorStyles.miniButtonMid))
				{
					(target as MMFeedbacks).StopFeedbacks();
				}
                
				// reset button
				if (GUILayout.Button("Reset", EditorStyles.miniButtonMid))
				{
					(target as MMFeedbacks).ResetFeedbacks();
				}
				EditorGUI.EndDisabledGroup();
                
				// reverse button
				if (GUILayout.Button("Revert", EditorStyles.miniButtonMid))
				{
					(target as MMFeedbacks).Revert();
				}

				// debug button
				EditorGUI.BeginChangeCheck();
				{
					_debugView = GUILayout.Toggle(_debugView, "Debug View", EditorStyles.miniButtonRight);

					if (EditorGUI.EndChangeCheck())
					{
						foreach (var f in (target as MMFeedbacks).Feedbacks)
							f.hideFlags = _debugView ? HideFlags.HideInInspector : HideFlags.None;
						UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
					}
				}
			}
			EditorGUILayout.EndHorizontal();


			float pingPong = Mathf.PingPong(Time.unscaledTime, 0.25f);
            
			// if in pause, we display additional controls
			if (_targetMMFeedbacks.InScriptDrivenPause)
			{
				// draws a warning box
				_scriptDrivenBoxColor = Color.Lerp(_scriptDrivenBoxColorFrom, _scriptDrivenBoxColorTo, pingPong);
				GUI.skin.box.normal.background = Texture2D.whiteTexture;
				GUI.backgroundColor = _scriptDrivenBoxColor;
				GUI.skin.box.normal.textColor = Color.black;
				GUILayout.Box("Script driven pause in progress, call Resume() to exit pause", GUILayout.ExpandWidth(true));
				GUI.backgroundColor = _originalBackgroundColor;
				GUI.skin.box.normal.background = _scriptDrivenBoxBackgroundTexture; 
                
				// draws resume button
				if (GUILayout.Button("Resume"))
				{
					_targetMMFeedbacks.ResumeFeedbacks();
				}
			}

			// Debug draw
			if (_debugView)
			{
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(_mmfeedbacks, true);
				EditorGUI.EndDisabledGroup();
			}
		}

        /// <summary>
        /// 如果拖动反馈，我们需要不断重新绘制
        /// </summary>
        public override bool RequiresConstantRepaint()
		{
			return true;
		}

        /// <summary>
        /// 将反馈添加到列表中
        /// </summary>
        protected virtual MMFeedback AddFeedback(System.Type type)
		{
			/*GameObject gameObject = (target as MMFeedbacks).gameObject;

			MMFeedback newFeedback = Undo.AddComponent(gameObject, type) as MMFeedback;
			newFeedback.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;
			newFeedback.Label = FeedbackPathAttribute.GetFeedbackDefaultName(type);

			AddEditor(newFeedback);

			_mmfeedbacks.arraySize++;
			_mmfeedbacks.GetArrayElementAtIndex(_mmfeedbacks.arraySize - 1).objectReferenceValue = newFeedback;

			return newFeedback;*/
			return (target as MMFeedbacks).AddFeedback(type);
		}

        /// <summary>
        /// 移除所选反馈
        /// </summary>
        protected virtual void RemoveFeedback(int id)
		{
			/*SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;

			(target as MMFeedbacks).Feedbacks.Remove(feedback);

			_editors.Remove(feedback);
			Undo.DestroyObjectImmediate(feedback);*/
            
			(target as MMFeedbacks).RemoveFeedback(id);
		}

        //
        // Editors management
        //

        /// <summary>
        /// 创建反馈的编辑器
        /// </summary>
        protected virtual void AddEditor(MMFeedback feedback)
		{
			if (feedback == null)
				return;

			if (!_editors.ContainsKey(feedback))
			{
				Editor editor = null;
				CreateCachedEditor(feedback, null, ref editor);

				_editors.Add(feedback, editor as Editor);
			}
		}

        /// <summary>
        /// 销毁反馈的编辑器
        /// </summary>
        protected virtual void RemoveEditor(MMFeedback feedback)
		{
			if (feedback == null)
				return;

			if (_editors.ContainsKey(feedback))
			{
				DestroyImmediate(_editors[feedback]);
				_editors.Remove(feedback);
			}
		}

        //
        // Feedback generic menus
        //

        /// <summary>
        /// 初始化所选反馈
        /// </summary>
        protected virtual void InitializeFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;
			feedback.Initialization(feedback.gameObject);
		}

        /// <summary>
        /// 播放所选反馈
        /// </summary>
        protected virtual void PlayFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;
			feedback.Play(feedback.transform.position, _targetMMFeedbacks.FeedbacksIntensity);
		}

        /// <summary>
        /// 停止所选反馈
        /// </summary>
        protected virtual void StopFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;
			feedback.Stop(feedback.transform.position);
		}

        /// <summary>
        /// 重启所选反馈
        /// </summary>
        /// <param name="id"></param>
        protected virtual void ResetFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;
			feedback.ResetFeedback();
		}

        /// <summary>
        /// 复制所选反馈
        /// </summary>
        protected virtual void CopyFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;

			FeedbackCopy.Copy(new SerializedObject(feedback));
		}

        /// <summary>
        /// 请求源的完整副本。
        /// </summary>
        protected virtual void CopyAll()
		{
			FeedbackCopy.CopyAll(target as MMFeedbacks);
		}

        /// <summary>
        /// 将之前复制的反馈值粘贴到所选反馈中
        /// </summary>
        protected virtual void PasteFeedback(int id)
		{
			SerializedProperty property = _mmfeedbacks.GetArrayElementAtIndex(id);
			MMFeedback feedback = property.objectReferenceValue as MMFeedback;

			SerializedObject serialized = new SerializedObject(feedback);

			FeedbackCopy.Paste(serialized);
			serialized.ApplyModifiedProperties();
		}

        /// <summary>
        /// 创建一个新的反馈并应用之前复制的反馈值
        /// </summary>
        protected virtual void PasteAsNew()
		{
			MMFeedback newFeedback = AddFeedback(FeedbackCopy.Type);
			SerializedObject serialized = new SerializedObject(newFeedback);

			serialized.Update();
			FeedbackCopy.Paste(serialized);
			serialized.ApplyModifiedProperties();
		}

        /// <summary>
        /// 请求源中所有反馈的粘贴
        /// </summary>
        protected virtual void PasteAllAsNew()
		{
			serializedObject.Update();
			Undo.RecordObject(target, "Paste all MMFeedbacks");
			FeedbackCopy.PasteAll(this);
			serializedObject.ApplyModifiedProperties();
		}

        /// <summary>
        /// 将MMFeedbacks及其所有内容转换为新的改进版MMF_Player
        /// 从MMFeedbacks转换为MMF_Player，你有以下几种选择：
        /// - 在MMFeedbacks设置面板上，点击“生成MMF_Player”按钮，这将在同一对象上创建一个新的MMF_Player组件，并将反馈复制到其中，以便使用
        ///     旧的MMFeedbacks将不会受到影响
        /// - 按下“转换反馈”按钮，这将尝试用具有相同设置的新MMF_Player替换当前的MMFeedbacks。
        /// - 如果你在一个预制实例内部，普通的替换将不起作用，你需要在预制体级别添加一个空的MMF_Player，它将作为转换的接收者
        /// 
        /// </summary>
        public virtual void ConvertToMMF_Player(bool generateOnly)
		{
			GameObject targetObject = _targetMMFeedbacks.gameObject;
			MMF_Player oldMMFPlayer = targetObject.GetComponent<MMF_Player>();

            // 我们会移除该对象上可能存在的任何MMF_Player
            if ((oldMMFPlayer != null) && (oldMMFPlayer.FeedbacksList.Count > 0)) 
			{
				DestroyImmediate(oldMMFPlayer);
			}

            // 如果我们没有旧的播放器可用，并且处于预制实例内部，我们就会放弃操作，以免破坏现有设置
            if (!generateOnly && (oldMMFPlayer == null))
			{
                // 不幸的是，在预制实例上无法进行转换
                if (PrefabUtility.IsPartOfPrefabAsset(targetObject)
				    || PrefabUtility.IsPartOfPrefabInstance(targetObject)
				    || PrefabUtility.IsPartOfNonAssetPrefabInstance(targetObject))
				{
					Debug.LogWarning("不幸的是，你不能在预制实例上使用转换功能");
					return;
				}
			}

			_canDisplayInspector = false;
			serializedObject.Update();
			Undo.RegisterCompleteObjectUndo(target, "Convert to MMF_Player");
			MMDebug.DebugLogInfo("开始转换为MMF_Player --------");

			if (generateOnly)
			{
				// we create a new player
				if (oldMMFPlayer == null)
				{
					MMF_Player newPlayer = targetObject.AddComponent<MMF_Player>();
					CopyFromMMFeedbacksToMMF_Player(newPlayer);
				}
				else
				{
					CopyFromMMFeedbacksToMMF_Player(oldMMFPlayer);
				}
				serializedObject.ApplyModifiedProperties();
				return;
			}

			GameObject temporaryHost = null;
			// we create a new player
			if (oldMMFPlayer == null)
			{
				temporaryHost = new GameObject("TemporaryHost");   
				MMF_Player newPlayer = temporaryHost.AddComponent<MMF_Player>();
				CopyFromMMFeedbacksToMMF_Player(newPlayer);
                
				MonoScript yourReplacementScript = MonoScript.FromMonoBehaviour(newPlayer);
				SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
				serializedObject.Update();
				scriptProperty.objectReferenceValue = yourReplacementScript;
				serializedObject.ApplyModifiedProperties();
                
				// we copy back from our temp object
				MMF_Player finalPlayer = targetObject.GetComponent<MMF_Player>();
				finalPlayer.InitializationMode = newPlayer.InitializationMode;
				finalPlayer.SafeMode = newPlayer.SafeMode;
				finalPlayer.Direction = newPlayer.Direction;
				finalPlayer.AutoChangeDirectionOnEnd = newPlayer.AutoChangeDirectionOnEnd;
				finalPlayer.AutoPlayOnStart = newPlayer.AutoPlayOnStart;
				finalPlayer.AutoPlayOnEnable = newPlayer.AutoPlayOnEnable;
				finalPlayer.DurationMultiplier = newPlayer.DurationMultiplier;
				finalPlayer.DisplayFullDurationDetails = newPlayer.DisplayFullDurationDetails;
				finalPlayer.CooldownDuration = newPlayer.CooldownDuration;
				finalPlayer.InitialDelay = newPlayer.InitialDelay;
				finalPlayer.CanPlay = newPlayer.CanPlay;
				finalPlayer.CanPlayWhileAlreadyPlaying = newPlayer.CanPlayWhileAlreadyPlaying;
				finalPlayer.FeedbacksIntensity = newPlayer.FeedbacksIntensity;
				finalPlayer.Events = newPlayer.Events;
				finalPlayer.FeedbacksList = newPlayer.FeedbacksList;
				if (finalPlayer.FeedbacksList != null && finalPlayer.FeedbacksList.Count > 0)
				{
					foreach (MMF_Feedback feedback in finalPlayer.FeedbacksList)
					{
						feedback.Owner = finalPlayer;
						feedback.UniqueID = Guid.NewGuid().GetHashCode();
					}
				}
			}
			else
			{
				CopyFromMMFeedbacksToMMF_Player(oldMMFPlayer);
				PrefabUtility.RecordPrefabInstancePropertyModifications(oldMMFPlayer);
				serializedObject.Update();
				serializedObject.ApplyModifiedProperties();
				DestroyImmediate(_targetMMFeedbacks);
			}

			// we remove all remaining feedbacks
			Component[] feedbackArray = targetObject.GetComponents(typeof(MMFeedback));
			foreach (Component comp in feedbackArray)
			{
				DestroyImmediate(comp);    
			}

			if (temporaryHost != null)
			{
				DestroyImmediate(temporaryHost);    
			}

			MMDebug.DebugLogInfo("Conversion complete --------");
		}

		protected virtual void CopyFromMMFeedbacksToMMF_Player(MMF_Player newPlayer)
		{
			// we copy all its settings
			newPlayer.InitializationMode = _targetMMFeedbacks.InitializationMode;
			newPlayer.SafeMode = _targetMMFeedbacks.SafeMode;
			newPlayer.Direction = _targetMMFeedbacks.Direction;
			newPlayer.AutoChangeDirectionOnEnd = _targetMMFeedbacks.AutoChangeDirectionOnEnd;
			newPlayer.AutoPlayOnStart = _targetMMFeedbacks.AutoPlayOnStart;
			newPlayer.AutoPlayOnEnable = _targetMMFeedbacks.AutoPlayOnEnable;
			newPlayer.DurationMultiplier = _targetMMFeedbacks.DurationMultiplier;
			newPlayer.DisplayFullDurationDetails = _targetMMFeedbacks.DisplayFullDurationDetails;
			newPlayer.CooldownDuration = _targetMMFeedbacks.CooldownDuration;
			newPlayer.InitialDelay = _targetMMFeedbacks.InitialDelay;
			newPlayer.CanPlay = _targetMMFeedbacks.CanPlay;
			newPlayer.CanPlayWhileAlreadyPlaying = _targetMMFeedbacks.CanPlayWhileAlreadyPlaying;
			newPlayer.FeedbacksIntensity = _targetMMFeedbacks.FeedbacksIntensity;
			newPlayer.Events = _targetMMFeedbacks.Events;
            
			// we copy all its feedbacks
			SerializedProperty feedbacks = serializedObject.FindProperty("Feedbacks");
			for (int i = 0; i < feedbacks.arraySize; i++)
			{
				MMFeedback oldFeedback = (feedbacks.GetArrayElementAtIndex(i).objectReferenceValue as MMFeedback);
                
				// we look for a match in the new classes
				Type oldType = oldFeedback.GetType();
				string oldTypeName = oldType.Name.ToString();
				string newTypeName = oldTypeName.Replace("MMFeedback", "MMF_");
				Type newType = MMFeedbackStaticMethods.MMFGetTypeByName(newTypeName);
                
				if (newType == null)
				{
					MMDebug.DebugLogInfo("<color=red>Couldn't find any MMF_Feedback matching "+oldTypeName+"</color>");
				}
				else
				{
					MMF_Feedback newFeedback = newPlayer.AddFeedback(newType);
                    
					List<FieldInfo> oldFieldsList;
					int oldFieldsListLength = MMF_FieldInfo.GetFieldInfo(oldFeedback, out oldFieldsList);

					for (int j = 0; j < oldFieldsListLength; j++)
					{
						string searchedField = oldFieldsList[j].Name;

						if (!FeedbackCopy.IgnoreList.Contains(searchedField))
						{
							FieldInfo newField = newType.GetField(searchedField);
							FieldInfo oldField = oldType.GetField(searchedField);
                            
							if (newField != null)
							{
								if (newField.FieldType == oldField.FieldType)
								{
									newField.SetValue(newFeedback, oldField.GetValue(oldFeedback));    
								}
								else
								{
									if (oldField.FieldType.IsEnum)
									{
										newField.SetValue(newFeedback, (int)oldField.GetValue(oldFeedback));    
									}
								}
							}    
						}
					}
					MMDebug.DebugLogInfo("已添加新的反馈类型 " + newTypeName);
				}
			}
			newPlayer.RefreshCache();
		}
	}
}