using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonEvent : MonoBehaviour, ISubmitHandler, ISelectHandler
{

    public UnityEvent Confirm;
    public UnityEvent Select;

    public void OnSelect(BaseEventData eventData)
    {
        Select.Invoke();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        Confirm.Invoke();
    }
}
