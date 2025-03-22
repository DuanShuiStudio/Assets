using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MoreMountains.Feedbacks
{
	[CustomPropertyDrawer(typeof(MMFEnumConditionAttribute))]
	public class MMFEnumConditionAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			MMFEnumConditionAttribute enumConditionAttribute = (MMFEnumConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
			bool previouslyEnabled = GUI.enabled;
			GUI.enabled = enabled;
			if (!enumConditionAttribute.Hidden || enabled)
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			GUI.enabled = previouslyEnabled;
		}

		private bool GetConditionAttributeResult(MMFEnumConditionAttribute enumConditionAttribute, SerializedProperty property)
		{
			bool enabled = true;
			string propertyPath = property.propertyPath;
			string conditionPath = propertyPath.Replace(property.name, enumConditionAttribute.ConditionEnum);
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

			if ((sourcePropertyValue != null) && (sourcePropertyValue.propertyType == SerializedPropertyType.Enum))
			{
				int currentEnum = sourcePropertyValue.enumValueIndex;
				enabled = enumConditionAttribute.ContainsBitFlag(currentEnum);
			}
			else
			{
				Debug.LogWarning("在对象中未找到与以下条件属性匹配的布尔值:" + enumConditionAttribute.ConditionEnum);
			}

			return enabled;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			MMFEnumConditionAttribute enumConditionAttribute = (MMFEnumConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(enumConditionAttribute, property);
            
			if (!enumConditionAttribute.Hidden || enabled)
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				/*int multiplier = 1; // this multiplier fixes issues in differing property spacing between MMFeedbacks and MMF_Player
				if (property.depth > 0)
				{
					multiplier = property.depth;
				}*/
				return -EditorGUIUtility.standardVerticalSpacing /** multiplier*/;
			}
		}
	}

    // 最初的实现是由 http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/
    [CustomPropertyDrawer(typeof(MMFConditionAttribute))]
	public class MMFConditionAttributeDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			MMFConditionAttribute conditionAttribute = (MMFConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(conditionAttribute, property);
			bool previouslyEnabled = GUI.enabled;
			GUI.enabled = conditionAttribute.Negative ? !enabled : enabled;
			if (ShouldDisplay(conditionAttribute, enabled))
			{
				EditorGUI.PropertyField(position, property, label, true);
			}
			GUI.enabled = previouslyEnabled;
		}

		private bool GetConditionAttributeResult(MMFConditionAttribute conditionAttribute, SerializedProperty property)
		{
			bool enabled = true;
			string propertyPath = property.propertyPath;
			string conditionPath = propertyPath.Replace(property.name, conditionAttribute.ConditionBoolean);
			SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

			if (sourcePropertyValue != null)
			{
				enabled = sourcePropertyValue.boolValue;
			}
			else
			{
				Debug.LogWarning("在对象中未找到与条件属性匹配的布尔值: " + conditionAttribute.ConditionBoolean);
			}

			return enabled;
		}
		
		private bool ShouldDisplay(MMFConditionAttribute conditionAttribute, bool result)
		{
			bool shouldDisplay = !conditionAttribute.Hidden || result;
			if (conditionAttribute.Negative)
			{
				shouldDisplay = !shouldDisplay;
			}
			return shouldDisplay;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			MMFConditionAttribute conditionAttribute = (MMFConditionAttribute)attribute;
			bool enabled = GetConditionAttributeResult(conditionAttribute, property);

			if (ShouldDisplay(conditionAttribute, enabled))
			{
				return EditorGUI.GetPropertyHeight(property, label);
			}
			else
			{
				/*int multiplier = 1; // this multiplier fixes issues in differing property spacing between MMFeedbacks and MMF_Player
				if (property.depth > 0)
				{
					//multiplier = property.depth;
				}*/
				return -EditorGUIUtility.standardVerticalSpacing /** multiplier*/; 
			}
		}
	}

	[CustomPropertyDrawer(typeof(MMFHiddenAttribute))]
	public class MMFHiddenAttributeDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return 0f;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{

		}
	}

	[CustomPropertyDrawer(typeof(MMFInformationAttribute))]
    /// <summary>
    /// 这个类允许在属性旁边（之前或之后）显示消息框（警告、信息、错误等）
    /// </summary>
    public class MMFInformationAttributeDrawer : PropertyDrawer
	{
        // 这个类决定了帮助框后面的空间、文本框前面的空间以及帮助框图标的宽度。
        const int spaceBeforeTheTextBox = 5;
		const int spaceAfterTheTextBox = 10;
		const int iconWidth = 55;

		MMFInformationAttribute informationAttribute { get { return ((MMFInformationAttribute)attribute); } }

        /// <summary>
        /// 在GUI中，按照指定的顺序显示该属性和文本框
        /// </summary>
        /// <param name="rect">Rect.</param>
        /// <param name="prop">Property.</param>
        /// <param name="label">Label.</param>
        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
		{
			if (HelpEnabled())
			{
				EditorStyles.helpBox.richText = true;
				Rect helpPosition = rect;
				Rect textFieldPosition = rect;

				if (!informationAttribute.MessageAfterProperty)
				{
                    // 我们将消息框放置在属性之前
                    helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);

					textFieldPosition.y += helpPosition.height + spaceBeforeTheTextBox;
					textFieldPosition.height = GetPropertyHeight(prop, label);
				}
				else
				{
                    // 我们先放置属性，然后放置消息框
                    textFieldPosition.height = GetPropertyHeight(prop, label);

					helpPosition.height = DetermineTextboxHeight(informationAttribute.Message);
                    // 我们增加整个属性的高度（属性加帮助框，就像在这个脚本中覆盖的那样），然后减去这两个高度，只得到属性本身的高度。
                    helpPosition.y += GetPropertyHeight(prop, label) - DetermineTextboxHeight(informationAttribute.Message) - spaceAfterTheTextBox;
				}

				EditorGUI.HelpBox(helpPosition, informationAttribute.Message, informationAttribute.Type);
				EditorGUI.PropertyField(textFieldPosition, prop, label, true);
			}
			else
			{
				Rect textFieldPosition = rect;
				textFieldPosition.height = GetPropertyHeight(prop, label);
				EditorGUI.PropertyField(textFieldPosition, prop, label, true);
			}
		}

        /// <summary>
        /// 返回整个块（属性+帮助文本）的完整高度
        /// </summary>
        /// <returns>The block height.</returns>
        /// <param name="property">Property.</param>
        /// <param name="label">Label.</param>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			if (HelpEnabled())
			{
				return EditorGUI.GetPropertyHeight(property) + DetermineTextboxHeight(informationAttribute.Message) + spaceAfterTheTextBox + spaceBeforeTheTextBox;
			}
			else
			{
				return EditorGUI.GetPropertyHeight(property);
			}
		}

        /// <summary>
        /// 检查编辑器首选项，以查看是否启用了帮助功能
        /// </summary>
        /// <returns><c>true</c>, if enabled was helped, <c>false</c> otherwise.</returns>
        protected virtual bool HelpEnabled()
		{
			bool helpEnabled = false;
			if (EditorPrefs.HasKey("MMShowHelpInInspectors"))
			{
				if (EditorPrefs.GetBool("MMShowHelpInInspectors"))
				{
					helpEnabled = true;
				}
			}
			return helpEnabled;
		}

        /// <summary>
        /// 确定文本框的高度。
        /// </summary>
        /// <returns>The textbox height.</returns>
        /// <param name="message">Message.</param>
        protected virtual float DetermineTextboxHeight(string message)
		{
			GUIStyle style = new GUIStyle(EditorStyles.helpBox);
			style.richText = true;

			float newHeight = style.CalcHeight(new GUIContent(message), EditorGUIUtility.currentViewWidth - iconWidth);
			return newHeight;
		}
	}

	[CustomPropertyDrawer(typeof(MMFReadOnlyAttribute))]
	public class MMFReadOnlyAttributeDrawer : PropertyDrawer
	{
        // 这是必要的，因为有些属性倾向于折叠得比它们的内容还小
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, true);
		}

        // 绘制一个禁用的属性字段
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			GUI.enabled = false; // Disable fields
			EditorGUI.PropertyField(position, property, label, true);
			GUI.enabled = true; // Enable fields
		}
	}


}