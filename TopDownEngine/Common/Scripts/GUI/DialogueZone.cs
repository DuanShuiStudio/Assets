using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 用于存储对白信息（dialogue lines info）的类
    /// </summary>
    [Serializable]
	public class DialogueElement
	{
		[Multiline]
		public string DialogueLine;
	}

    /// <summary>
    /// 将这个类添加到一个空组件上。它需要一个Collider或Collider2D，并将其设置为“is trigger”
    /// 然后您可以通过检查器自定义对话区域
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Dialogue Zone")]
	public class DialogueZone : ButtonActivated
	{
		[MMInspectorGroup("Dialogue Look", true, 18)]

		/// the prefab to use to display the dialogue
		[Tooltip("用于显示对话的预制体")]
		public DialogueBox DialogueBoxPrefab;
		/// the color of the text background.
		[Tooltip("文本背景的颜色")]
		public Color TextBackgroundColor = Color.black;
		/// the color of the text
		[Tooltip("文本的颜色")]
		public Color TextColor = Color.white;
		/// the font that should be used to display the text
		[Tooltip("用于显示文本的字体")]
		public Font TextFont;
		/// the size of the font
		[Tooltip("字体的大小")]
		public int TextSize = 40;
		/// the text alignment in the box used to display the text
		[Tooltip("用于显示文本的框中的文本对齐方式")]
		public TextAnchor Alignment = TextAnchor.MiddleCenter;
        
		[MMInspectorGroup("Dialogue Speed (in seconds)", true, 19)]

		/// the duration of the in and out fades
		[Tooltip("淡入和淡出的持续时间")]
		public float FadeDuration = 0.2f;
		/// the time between two dialogues 
		[Tooltip("两个对话之间的时间间隔")]
		public float TransitionTime = 0.2f;

		[MMInspectorGroup("Dialogue Position", true, 20)]

		/// the distance from the top of the box collider the dialogue box should appear at
		[Tooltip("对话框应出现在的框碰撞器顶部的距离")]
		public Vector3 Offset = Vector3.zero;
		/// if this is true, the dialogue boxes will follow the zone's position
		[Tooltip("如果为真，则对话框将遵循区域的位置")]
		public bool BoxesFollowZone = false;

		[MMInspectorGroup("Player Movement", true, 21)]

		/// if this is set to true, the character will be able to move while dialogue is in progress
		[Tooltip("如果将其设置为真，则角色在对话进行时将能够移动")]
		public bool CanMoveWhileTalking = true;

		[MMInspectorGroup("Press button to go from one message to the next ?", true, 22)]

		/// whether or not this zone is handled by a button or not
		[Tooltip("这个区域是否由按钮处理")]
		public bool ButtonHandled = true;
		/// duration of the message. only considered if the box is not button handled
		[Header("Only if the dialogue is not button handled仅当对话不是由按钮处理时 :")]
		[Range(1, 100)]
		[Tooltip("消息应显示的持续时间（以秒为单位）。仅当框不是由按钮处理时才考虑")]
		public float MessageDuration = 3f;
        
		[MMInspectorGroup("Activations", true, 23)]
		/// true if can be activated more than once
		[Tooltip("如果可以被激活多次，则为真。")]
		public bool ActivableMoreThanOnce = true;
		/// if the zone is activable more than once, how long should it remain inactive between up times?
		[Range(1, 100)]
		[Tooltip("如果该区域可以被激活多次，那么在每次激活之间它应该保持活动多长时间？")]
		public float InactiveTime = 2f;

        /// 对白线
        [MMInspectorGroup("Dialogue Lines", true, 24)]
		public DialogueElement[] Dialogue;

        /// 私有变量
        protected DialogueBox _dialogueBox;
		protected bool _activated = false;
		protected bool _playing = false;
		protected int _currentIndex;
		protected bool _activable = true;
		protected WaitForSeconds _transitionTimeWFS;
		protected WaitForSeconds _messageDurationWFS;
		protected WaitForSeconds _inactiveTimeWFS;

        /// <summary>
        /// 初始化对话区域
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			_currentIndex = 0;
			_transitionTimeWFS = new WaitForSeconds(TransitionTime);
			_messageDurationWFS = new WaitForSeconds(MessageDuration);
			_inactiveTimeWFS = new WaitForSeconds(InactiveTime);
		}

        /// <summary>
        /// 当按下按钮时，我们开始对话
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			if (_playing && !ButtonHandled)
			{
				return;
			}
			base.TriggerButtonAction();
			StartDialogue();
		}

        /// <summary>
        /// 当被触发时，无论是通过按下按钮还是仅仅进入区域，都会开始对话
        /// </summary>
        public virtual void StartDialogue()
		{
            // 如果对话区域没有框碰撞器，我们不进行任何操作并退出
            if ((_collider == null) && (_collider2D == null))
			{
				return;
			}

            // 如果该区域已经被激活且不能被激活多次
            if (_activated && !ActivableMoreThanOnce)
			{
				return;
			}

            // 如果该区域不可激活，我们不进行任何操作并退出
            if (!_activable)
			{
				return;
			}

            // 如果玩家在说话时不能移动，我们通知游戏管理器
            if (!CanMoveWhileTalking)
			{
				LevelManager.Instance.FreezeCharacters();
				if (ShouldUpdateState && (_characterButtonActivation != null))
				{
					_characterButtonActivation.GetComponentInParent<Character>().MovementState.ChangeState(CharacterStates.MovementStates.Idle);
				}
			}

            // 如果它还没有播放，我们将初始化对话框
            if (!_playing)
			{
                // 我们实例化对话框
                _dialogueBox = Instantiate(DialogueBoxPrefab);
                // 我们设置它的位置
                if (_collider2D != null)
				{
					_dialogueBox.transform.position = _collider2D.bounds.center + Offset;
				}
				if (_collider != null)
				{
					_dialogueBox.transform.position = _collider.bounds.center + Offset;
				}
                // 我们设置颜色和背景颜色
                _dialogueBox.ChangeColor(TextBackgroundColor, TextColor);
                // 如果是按钮处理的对话，我们打开A提示
                _dialogueBox.ButtonActive(ButtonHandled);

                // 如果指定了字体设置，我们将其设置
                if (BoxesFollowZone)
				{
					_dialogueBox.transform.SetParent(this.gameObject.transform);
				}
				if (TextFont != null)
				{
					_dialogueBox.DialogueText.font = TextFont;
				}
				if (TextSize != 0)
				{
					_dialogueBox.DialogueText.fontSize = TextSize;
				}
				_dialogueBox.DialogueText.alignment = Alignment;

                // 现在正在播放对话
                _playing = true;
			}
            // 我们开始下一个对话
            StartCoroutine(PlayNextDialogue());
		}

        /// <summary>
        /// 打开或关闭碰撞器
        /// </summary>
        /// <param name="status"></param>
        protected virtual void EnableCollider(bool status)
		{
			if (_collider2D != null)
			{
				_collider2D.enabled = status;
			}
			if (_collider != null)
			{
				_collider.enabled = status;
			}
		}

        /// <summary>
        /// 播放队列中的下一个对话
        /// </summary>
        protected virtual IEnumerator PlayNextDialogue()
		{
            // 我们检查对话框是否仍然存在。
            if (_dialogueBox == null)
			{
				yield break;
			}
            // 如果不是第一条消息。
            if (_currentIndex != 0)
			{
                // 我们关闭消息。
                _dialogueBox.FadeOut(FadeDuration);
                // 在播放下一个对话之前，我们等待指定的过渡时间
                yield return _transitionTimeWFS;
			}
            // 如果我们已经到了最后一条对话，我们就退出
            if (_currentIndex >= Dialogue.Length)
			{
				_currentIndex = 0;
				Destroy(_dialogueBox.gameObject);
				EnableCollider(false);
                // 我们将“已激活”设置为真，因为现在对话区域已经被打开
                _activated = true;
                // 我们让玩家再次移动
                if (!CanMoveWhileTalking)
				{
					LevelManager.Instance.UnFreezeCharacters();
				}
				if ((_characterButtonActivation != null))
				{
					_characterButtonActivation.InButtonActivatedZone = false;
					_characterButtonActivation.ButtonActivatedZone = null;
				}
                // 我们将区域激活一段时间
                if (ActivableMoreThanOnce)
				{
					_activable = false;
					_playing = false;
					StartCoroutine(Reactivate());
				}
				else
				{
					gameObject.SetActive(false);
				}
				yield break;
			}

            // 我们检查对话框是否仍然存在
            if (_dialogueBox.DialogueText != null)
			{
                // 每个对话框都以淡入开始
                _dialogueBox.FadeIn(FadeDuration);
                // 然后我们用当前的对话设置框的文本
                _dialogueBox.DialogueText.text = Dialogue[_currentIndex].DialogueLine;
			}

			_currentIndex++;

            // 如果区域不是由按钮处理的，我们启动一个协程来自动播放下一个对话
            if (!ButtonHandled)
			{
				StartCoroutine(AutoNextDialogue());
			}
		}

        /// <summary>
        /// 自动转到下一条对话
        /// </summary>
        /// <returns>The next dialogue.</returns>
        protected virtual IEnumerator AutoNextDialogue()
		{
			// we wait for the duration of the message
			yield return _messageDurationWFS;
			StartCoroutine(PlayNextDialogue());
		}

        /// <summary>
        /// 重新激活对话区域
        /// </summary>
        protected virtual IEnumerator Reactivate()
		{
			yield return _inactiveTimeWFS;
			EnableCollider(true);
			_activable = true;
			_playing = false;
			_currentIndex = 0;
			_promptHiddenForever = false;

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
		}
	}
}