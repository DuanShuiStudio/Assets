using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	public class MMCinemachineHelpers : MonoBehaviour
	{
		public static GameObject AutomaticCinemachineShakersSetup(MMF_Player owner, string feedbackName)
		{
			GameObject virtualCameraGo = null;
			
			
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			bool newVcam = false;
			string additions = owner.name + " "+feedbackName+ " 反馈自动抖动器设置： ";
#endif

#if MM_CINEMACHINE
				//在场景中查找虚拟相机（Cinemachine）的大脑组件（Cinemachine Brain）。 
				CinemachineBrain cinemachineBrain = (CinemachineBrain)Object.FindObjectOfType(typeof(CinemachineBrain));
				if (cinemachineBrain == null)
				{
					cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
					additions += "在场景中查找虚拟相机（Cinemachine）的大脑组件（Cinemachine Brain） ";
				}
			
				// 在场景中查找一个虚拟相机（vcam，即Virtual Camera）。 
				CinemachineVirtualCamera virtualCamera = (CinemachineVirtualCamera)Object.FindObjectOfType(typeof(CinemachineVirtualCamera));
				if (virtualCamera == null)
				{
					GameObject newVirtualCamera = new GameObject("CinemachineVirtualCamera");
					if (Camera.main != null)
					{
						newVirtualCamera.transform.position = Camera.main.transform.position;	
					}
					virtualCamera = newVirtualCamera.AddComponent<CinemachineVirtualCamera>();
					additions += "在场景中查找一个虚拟相机（vcam，即Virtual Camera） ";
					newVcam = true;
				}
				virtualCameraGo = virtualCamera.gameObject;
				CinemachineImpulseListener impulseListener = virtualCamera.GetComponent<CinemachineImpulseListener>();
				if (impulseListener == null)
				{
					impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
					additions += "添加了一个脉冲监听器。 ";
			}
#elif MM_CINEMACHINE3
            //在场景中查找虚拟相机（Cinemachine）的大脑组件（Cinemachine Brain）。 
            CinemachineBrain cinemachineBrain = (CinemachineBrain)Object.FindObjectOfType(typeof(CinemachineBrain));
				if (cinemachineBrain == null)
				{
					cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
					additions += "在场景中查找虚拟相机（Cinemachine）的大脑组件（Cinemachine Brain） ";
				}
            // 在场景中查找一个虚拟相机（vcam，即Virtual Camera）。 
            CinemachineCamera virtualCamera = (CinemachineCamera)Object.FindObjectOfType(typeof(CinemachineCamera));
				if (virtualCamera == null)
				{
					GameObject newVirtualCamera = new GameObject("CinemachineCamera");
					if (Camera.main != null)
					{
						newVirtualCamera.transform.position = Camera.main.transform.position;	
					}
					virtualCamera = newVirtualCamera.AddComponent<CinemachineCamera>();
					additions += "在场景中查找一个虚拟相机（vcam，即Virtual Camera） ";
					newVcam = true;
				}
				virtualCameraGo = virtualCamera.gameObject;
				CinemachineImpulseListener impulseListener = virtualCamera.GetComponent<CinemachineImpulseListener>();
				if (impulseListener == null)
				{
					impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
					additions += "添加了一个脉冲监听器。 ";
				}
			#endif

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (newVcam)
			{
				virtualCameraGo.MMGetOrAddComponent<MMCinemachineCameraShaker>();
				virtualCameraGo.MMGetOrAddComponent<MMCinemachineZoom>();
				virtualCameraGo.MMGetOrAddComponent<MMCinemachinePriorityListener>();
				virtualCameraGo.MMGetOrAddComponent<MMCinemachineClippingPlanesShaker>();
				virtualCameraGo.MMGetOrAddComponent<MMCinemachineFieldOfViewShaker>();	
				additions += "已向虚拟相机（Cinemachine Camera）添加了相机抖动器、缩放功能、优先级监听器、裁剪平面抖动器和视场抖动器。  ";
			}
			
			MMDebug.DebugLogInfo( additions + "一切都准备好了。 ");
			#endif
			return virtualCameraGo;
		}
	}
}
