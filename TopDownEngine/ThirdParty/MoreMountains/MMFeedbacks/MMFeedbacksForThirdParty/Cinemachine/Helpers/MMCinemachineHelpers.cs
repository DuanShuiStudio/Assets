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
			string additions = owner.name + " "+feedbackName+ " �����Զ����������ã� ";
#endif

#if MM_CINEMACHINE
				//�ڳ����в������������Cinemachine���Ĵ��������Cinemachine Brain���� 
				CinemachineBrain cinemachineBrain = (CinemachineBrain)Object.FindObjectOfType(typeof(CinemachineBrain));
				if (cinemachineBrain == null)
				{
					cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
					additions += "�ڳ����в������������Cinemachine���Ĵ��������Cinemachine Brain�� ";
				}
			
				// �ڳ����в���һ�����������vcam����Virtual Camera���� 
				CinemachineVirtualCamera virtualCamera = (CinemachineVirtualCamera)Object.FindObjectOfType(typeof(CinemachineVirtualCamera));
				if (virtualCamera == null)
				{
					GameObject newVirtualCamera = new GameObject("CinemachineVirtualCamera");
					if (Camera.main != null)
					{
						newVirtualCamera.transform.position = Camera.main.transform.position;	
					}
					virtualCamera = newVirtualCamera.AddComponent<CinemachineVirtualCamera>();
					additions += "�ڳ����в���һ�����������vcam����Virtual Camera�� ";
					newVcam = true;
				}
				virtualCameraGo = virtualCamera.gameObject;
				CinemachineImpulseListener impulseListener = virtualCamera.GetComponent<CinemachineImpulseListener>();
				if (impulseListener == null)
				{
					impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
					additions += "�����һ������������� ";
			}
#elif MM_CINEMACHINE3
            //�ڳ����в������������Cinemachine���Ĵ��������Cinemachine Brain���� 
            CinemachineBrain cinemachineBrain = (CinemachineBrain)Object.FindObjectOfType(typeof(CinemachineBrain));
				if (cinemachineBrain == null)
				{
					cinemachineBrain = Camera.main.gameObject.AddComponent<CinemachineBrain>();
					additions += "�ڳ����в������������Cinemachine���Ĵ��������Cinemachine Brain�� ";
				}
            // �ڳ����в���һ�����������vcam����Virtual Camera���� 
            CinemachineCamera virtualCamera = (CinemachineCamera)Object.FindObjectOfType(typeof(CinemachineCamera));
				if (virtualCamera == null)
				{
					GameObject newVirtualCamera = new GameObject("CinemachineCamera");
					if (Camera.main != null)
					{
						newVirtualCamera.transform.position = Camera.main.transform.position;	
					}
					virtualCamera = newVirtualCamera.AddComponent<CinemachineCamera>();
					additions += "�ڳ����в���һ�����������vcam����Virtual Camera�� ";
					newVcam = true;
				}
				virtualCameraGo = virtualCamera.gameObject;
				CinemachineImpulseListener impulseListener = virtualCamera.GetComponent<CinemachineImpulseListener>();
				if (impulseListener == null)
				{
					impulseListener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
					additions += "�����һ������������� ";
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
				additions += "�������������Cinemachine Camera���������������������Ź��ܡ����ȼ����������ü�ƽ�涶�������ӳ���������  ";
			}
			
			MMDebug.DebugLogInfo( additions + "һ�ж�׼�����ˡ� ");
			#endif
			return virtualCameraGo;
		}
	}
}
