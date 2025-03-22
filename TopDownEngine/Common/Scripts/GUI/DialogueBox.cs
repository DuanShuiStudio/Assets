using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 对话框类。不要直接将其添加到你的游戏中，请参考DialogueZone
    /// </summary>
    public class DialogueBox : TopDownMonoBehaviour
	{
		[Header("Dialogue Box对话框")]
		/// the text panel background
		[Tooltip("文本面板的背景")]
		public CanvasGroup TextPanelCanvasGroup;
		/// the text to display
		[Tooltip("要显示的文本")]
		public Text DialogueText;
		/// the Button A prompt
		[Tooltip("按钮A的提示")]
		public CanvasGroup Prompt;
		/// the list of images to colorize
		[Tooltip("要着色的图片列表")]
		public List<Image> ColorImages;

		protected Color _backgroundColor;
		protected Color _textColor;

        /// <summary>
        /// 更改文本。
        /// </summary>
        /// <param name="newText">New text.</param>
        public virtual void ChangeText(string newText)
		{
			DialogueText.text = newText;
		}

        /// <summary>
        /// 激活按钮A的提示
        /// </summary>
        /// <param name="state">If set to <c>true</c> state.</param>
        public virtual void ButtonActive(bool state)
		{
			Prompt.gameObject.SetActive(state);
		}

        /// <summary>
        /// 将对话框的颜色更改为参数中的颜色
        /// </summary>
        /// <param name="backgroundColor">Background color.</param>
        /// <param name="textColor">Text color.</param>
        public virtual void ChangeColor(Color backgroundColor, Color textColor)
		{
			_backgroundColor = backgroundColor;
			_textColor = textColor;

			foreach(Image image in ColorImages)
			{
				image.color = _backgroundColor;
			}
			DialogueText.color = _textColor;
		}

        /// <summary>
        /// 淡入对话框
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void FadeIn(float duration)
		{
			if (TextPanelCanvasGroup != null)
			{
				StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 1f));
			}
			if (DialogueText != null)
			{
				StartCoroutine(MMFade.FadeText(DialogueText, duration, _textColor));
			}
			if (Prompt != null)
			{
				StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 1f));
			}
		}

        /// <summary>
        /// 淡出对话框
        /// </summary>
        /// <param name="duration">Duration.</param>
        public virtual void FadeOut(float duration)
		{
			Color newBackgroundColor = new Color(_backgroundColor.r, _backgroundColor.g, _backgroundColor.b, 0);
			Color newTextColor = new Color(_textColor.r, _textColor.g, _textColor.b, 0);

			StartCoroutine(MMFade.FadeCanvasGroup(TextPanelCanvasGroup, duration, 0f));
			StartCoroutine(MMFade.FadeText(DialogueText, duration, newTextColor));
			StartCoroutine(MMFade.FadeCanvasGroup(Prompt, duration, 0f));
		}
	}
}