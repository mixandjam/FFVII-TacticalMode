using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
using UnityEngine.Rendering;
using DG.Tweening;

public class TestScript : MonoBehaviour
{

    private MovementInput movement;
    private Animator anim;

    public bool tacticalMode;
    public bool isAiming;

    [Space]

    [Header("ATB Data")]
    public float atbSlider;
    public float filledAtbValue = 100;
    public int atbCount;

    [Space]

    [Header("Targets in radius")]
    public List<Transform> targets;
    public int targetIndex;

    [Space]

    public VisualEffect hey;
    public Volume slowMotionVolume;

    public GameObject gameCam;
    public GameObject targetCam;

    public Transform aimObject;


    private void Start()
    {
        movement = GetComponent<MovementInput>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {

        if (tacticalMode)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
                SelectTarget(0);

            if (Input.GetKeyDown(KeyCode.Alpha1))
                SelectTarget(1);
        }


        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("slash");
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ModifyATB(20);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            ModifyATB(-20);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (atbCount > 0 && !tacticalMode)
                SetTacticalMode(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAction();
        }
    }

    public void ModifyATB(float amount)
    {
        atbSlider += amount;
        atbSlider = Mathf.Clamp(atbSlider, 0, (filledAtbValue * 2));

        if (atbSlider >= filledAtbValue && atbCount == 0)
            atbCount = 1;
        if (atbSlider >= (filledAtbValue*2) && atbCount == 1)
            atbCount = 2;
    }

    public void ClearATB()
    {
        float value = (atbCount == 1) ? 0 : 1;
        atbSlider = value;
    }

    public void SetTacticalMode(bool on)
    {
        tacticalMode = on;
        movement.enabled = !on;

        float time = on ? .1f : 1;
        Time.timeScale = time;

        //Polish
        DOVirtual.Float(on ? 0 : 1, on ? 1 : 0, .3f, SlowmotionPostProcessing).SetUpdate(true);
    }

    public void SelectTarget(int index)
    {
        targetIndex = index;
        SetAimCamera(true);
        aimObject.DOLookAt(targets[targetIndex].position, .3f).SetUpdate(true);
    }

    public void SetAimCamera(bool on)
    {
        if (!on)
            StartCoroutine(RecenterCamera());
        targetCam.SetActive(on);
        isAiming = on;
    }

    IEnumerator RecenterCamera()
    {
        gameCam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        gameCam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = false;
    }

    public void PlayVFX()
    {
        hey.SendEvent("OnPlay");
        Camera.main.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    public void CancelAction()
    {
        if (!targetCam.activeSelf && tacticalMode)
            SetTacticalMode(false);

        if (targetCam.activeSelf)
            SetAimCamera(false);
    }

    public void SlowmotionPostProcessing(float x)
    {
        slowMotionVolume.weight = x;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            targets.Add(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (targets.Contains(other.transform))
                targets.Remove(other.transform);
        }
    }
}
