using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

public class ButtonEvent : MonoBehaviour, ISubmitHandler, ISelectHandler, IDeselectHandler
{

    public UnityEvent Confirm;
    public UnityEvent Select;

    private Vector3 pos;

    private void Start()
    {
        pos = transform.position;
        StartCoroutine(WaitForFrames());
    }

    IEnumerator WaitForFrames()
    {
        yield return new WaitForSecondsRealtime(.5f);
        pos = transform.position;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.DOMove(pos, .2f).SetEase(Ease.InOutSine).SetUpdate(true);
    }

    public void OnSelect(BaseEventData eventData)
    {
        transform.DOMove(pos+ (Vector3.right*10), .2f).SetEase(Ease.InOutSine).SetUpdate(true);
        Select.Invoke();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        transform.DOPunchPosition(Vector3.right, .2f, 10, 1).SetUpdate(true);
        Confirm.Invoke();
    }
}
