using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class ButtonEvents : MonoBehaviour, ISelectHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent PointerEnter;

	[SerializeField]
	private UnityEvent PointerExit;


	public void OnSelect(BaseEventData eventData)
	{
		PointerEnter.Invoke();
	}

	public void OnDeselect(BaseEventData eventData)
	{
		PointerExit.Invoke();
	}
}
