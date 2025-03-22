using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{	
	[RequireComponent(typeof(RectTransform))]
    /// <summary>
    /// 这个类处理选择标记，用于标记当前选中的槽位
    /// </summary>
    public class InventorySelectionMarker : MonoBehaviour 
	{
		[MMInformation("选择标记将高亮显示当前选中的项目。在此，您可以定义其过渡速度及最小距离阈值（通常保留默认设置即可）", MMInformationAttribute.InformationType.Info,false)]
        /// 选择标记从一个槽位移动到另一个槽位的速度
        public float TransitionSpeed=5f;
        /// 标记停止移动的阈值距离
        public float MinimalTransitionDistance=0.01f;

		protected RectTransform _rectTransform;
		protected GameObject _currentSelection;
		protected Vector3 _originPosition;
		protected Vector3 _originLocalScale;
		protected Vector3 _originSizeDelta;
		protected float _originTime;
		protected bool _originIsNull=true;
		protected float _deltaTime;

        /// <summary>
        /// 在启动时，我们获取相关联的矩形变换组件
        /// </summary>
        protected virtual void Start () 
		{
			_rectTransform = GetComponent<RectTransform>();
		}

        /// <summary>
        /// 在更新时，我们获取当前选中的对象，并在必要时将标记移动到该对象的位置
        /// </summary>
        protected virtual void Update () 
		{			
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}

			if (_currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>() == null)
			{
				return;
			}

			if (Vector3.Distance(transform.position,_currentSelection.transform.position) > MinimalTransitionDistance)
			{
				if (_originIsNull)
				{
					_originIsNull=false;
					_originPosition = transform.position;
					_originLocalScale = _rectTransform.localScale;
					_originSizeDelta = _rectTransform.sizeDelta;
					_originTime = Time.unscaledTime;
				} 
				_deltaTime =  (Time.unscaledTime - _originTime)*TransitionSpeed;
				transform.position= Vector3.Lerp(_originPosition,_currentSelection.transform.position,_deltaTime);
				_rectTransform.localScale = Vector3.Lerp(_originLocalScale, _currentSelection.GetComponent<RectTransform>().localScale,_deltaTime);
				_rectTransform.sizeDelta = Vector3.Lerp(_originSizeDelta, _currentSelection.GetComponent<RectTransform>().sizeDelta, _deltaTime);
			}
			else
			{
				_originIsNull=true;
			}
		}
	}
}