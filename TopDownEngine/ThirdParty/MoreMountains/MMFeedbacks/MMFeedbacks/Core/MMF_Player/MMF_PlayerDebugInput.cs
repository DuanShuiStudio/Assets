using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将此调试组件添加到MMF播放器中，您将能够在运行时通过按下（可自定义的）键来播放它，这在调整或调试您的反馈时非常有用。
    /// </summary>
    public class MMF_PlayerDebugInput : MonoBehaviour
	{
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        /// 用于触发此反馈的调试播放的按钮
        public Key PlayKey = Key.P;
#else
        /// 用于触发此反馈的调试播放的按钮
        public KeyCode PlayButton = KeyCode.P;
		#endif

		protected MMF_Player _player;

        /// <summary>
        /// 在Awake时，我们存储我们的MMF播放器
        /// </summary>
        protected virtual void Awake()
		{
			_player = this.gameObject.GetComponent<MMF_Player>();
		}

        /// <summary>
        /// 在Update时，如果按下了正确的按钮，我们就播放我们的反馈
        /// </summary>
        protected virtual void Update()
		{
			bool keyPressed = false;
		
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			keyPressed = Keyboard.current[PlayKey].wasPressedThisFrame;
			#else
			keyPressed = Input.GetKeyDown(PlayButton);
			#endif
		
			if (keyPressed)
			{
				_player.PlayFeedbacks();
			}
		}
	}
}

