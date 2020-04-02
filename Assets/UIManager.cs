using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{

    private TacticalModeScript gameScript;
    public Image test;
    public CanvasGroup tacticalCanvas;
    public CanvasGroup attackCanvas;

    public Transform commandsGroup;
    public Transform targetGroup;

    public CanvasGroup aimCanvas;
    public bool aimAtTarget;

    void Start()
    {

        gameScript = FindObjectOfType<TacticalModeScript>();
        gameScript.OnAttack.AddListener(() => AttackAction());
        gameScript.OnTacticalTrigger.AddListener((x) => ShowTacticalMenu(x));
        gameScript.OnTargetSelectTrigger.AddListener((x) => ShowTargetOptions(x));
    }

    private void Update()
    {
        if (aimAtTarget)
        {
            aimCanvas.transform.position = Camera.main.WorldToScreenPoint(gameScript.targets[gameScript.targetIndex].position + Vector3.up);
        }
    }

    public void AttackAction()
    {
        test.transform.DOComplete();
        test.transform.DOPunchPosition(Vector3.right/2, .3f, 10, 1);
    }

    public void ShowTacticalMenu(bool on)
    {
        tacticalCanvas.DOFade(on ? 1 : 0, .15f).SetUpdate(true);
        tacticalCanvas.interactable = on;
        attackCanvas.DOFade(on ? 0 : 1, .15f).SetUpdate(true);
        attackCanvas.interactable = !on;

        EventSystem.current.SetSelectedGameObject(null);

        if(on == true)
        {
            EventSystem.current.SetSelectedGameObject(tacticalCanvas.transform.GetChild(0).GetChild(0).gameObject);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(attackCanvas.transform.GetChild(0).gameObject);
            commandsGroup.gameObject.SetActive(!on);
            targetGroup.gameObject.SetActive(on);
        }
    }

    public void ShowTargetOptions(bool on)
    {
        EventSystem.current.SetSelectedGameObject(null);

        aimAtTarget = on;
        aimCanvas.alpha = on ? 1 : 0;

        commandsGroup.gameObject.SetActive(!on);
        targetGroup.gameObject.SetActive(on);

        if (on)
        {
            for (int i = 0; i < targetGroup.childCount; i++)
            {
                if(gameScript.targets.Count - 1 >= i)
                {
                    targetGroup.GetChild(i).GetComponent<CanvasGroup>().alpha = 1;
                    targetGroup.GetChild(i).GetComponent<CanvasGroup>().interactable = true;
                    targetGroup.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text = gameScript.targets[i].name;
                }
                else
                {
                    targetGroup.GetChild(i).GetComponent<CanvasGroup>().alpha = 0;
                    targetGroup.GetChild(i).GetComponent<CanvasGroup>().interactable = false;
                }
            }
        }
        EventSystem.current.SetSelectedGameObject(on ? targetGroup.GetChild(0).gameObject : commandsGroup.GetChild(0).gameObject);
    }



}
