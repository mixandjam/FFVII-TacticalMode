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
    public WeaponCollision weapon;

    public bool tacticalMode;
    public bool isAiming;
    public bool usingAbility;

    [Space]

    [Header("ATB Data")]
    public float atbSlider;
    public float filledAtbValue = 100;
    public int atbCount;

    [Space]

    [Header("Targets in radius")]
    public List<Transform> targets;
    public int targetIndex;
    public Transform aimObject;

    [Space]
    [Header("VFX")]
    public VisualEffect sparkVFX;
    public VisualEffect abilityVFX;
    public VisualEffect abilityHitVFX;
    public VisualEffect healVFX;
    [Space]
    [Header("Ligts")]
    public Light swordLight;
    public Light groundLight;
    [Header("Ligh Colors")]
    public Color sparkColor;
    public Color healColor;
    public Color abilityColot;
    [Space]
    [Header("Cameras")]
    public GameObject gameCam;
    public CinemachineVirtualCamera targetCam;

    [Space]

    public Volume slowMotionVolume;

    public float VFXDir = 5;


    private void Start()
    {
        weapon.onHit.AddListener((target) => HitTarget(target));
        movement = GetComponent<MovementInput>();
        anim = GetComponent<Animator>();
        ModifyATB(200);
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

        if(targets.Count > 0 && !tacticalMode && !usingAbility)
        {
            targetIndex = NearestTargetToCenter();
            aimObject.LookAt(targets[targetIndex]);
        }

        if (Input.GetKeyDown(KeyCode.Z) && tacticalMode)
        {
            anim.SetTrigger("heal");
            LightColor(groundLight, healColor, .5f);
            usingAbility = true;
            healVFX.SendEvent("OnPlay");
            SetTacticalMode(false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && tacticalMode)
        {
            LightColor(groundLight, abilityColot,.3f);
            usingAbility = true;
            abilityVFX.SendEvent("OnPlay");
            SetTacticalMode(false);
            if (Vector3.Distance(transform.position, targets[targetIndex].position) > 1 && Vector3.Distance(transform.position, targets[targetIndex].position) < 8)
                transform.DOMove(TargetOffset(), .5f);
            transform.DOLookAt(targets[targetIndex].position, .2f);
            anim.SetTrigger("ability");
        }

        //Attack
        if (Input.GetMouseButtonDown(0) && !tacticalMode)
        {
            if (Vector3.Distance(transform.position, targets[targetIndex].position) >2 && Vector3.Distance(transform.position, targets[targetIndex].position) < 8) {
                transform.DOMove(TargetOffset(), .3f).SetEase(Ease.InQuart);
                transform.DOLookAt(targets[targetIndex].position, .2f);
            }
            anim.SetTrigger("slash");
        }


        if (Input.GetMouseButtonDown(1))
        {
            if (atbCount > 0 && !tacticalMode)
                SetTacticalMode(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAction();
        }
    }

    public Vector3 TargetOffset()
    {
        Vector3 position;
        position = targets[targetIndex].position;
        return Vector3.MoveTowards(position, transform.position, 1.2f);
    }

    public void HitTarget(Transform x)
    {
        PlayVFX(sparkVFX, true);
        if (usingAbility)
            PlayVFX(abilityHitVFX, false);

        ModifyATB(10);

        LightColor(swordLight, sparkColor, .1f);

        print(x);
        if (x.GetComponent<Animator>() != null)
            x.GetComponent<Animator>().SetTrigger("hit");
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
        movement.desiredRotationSpeed = on ? 0 : .3f;
        movement.active = !on;

        tacticalMode = on;
        if (on == false)
            SetAimCamera(false);
        //movement.enabled = !on;

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
        targetCam.LookAt = on ? aimObject : null;
        targetCam.Follow = on ? aimObject : null;
        targetCam.gameObject.SetActive(on);
        isAiming = on;
    }

    IEnumerator RecenterCamera()
    {
        gameCam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = true;
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        gameCam.GetComponent<CinemachineFreeLook>().m_RecenterToTargetHeading.m_enabled = false;
    }

    public void PlayVFX(VisualEffect visualEffect, bool shakeCamera)
    {
        if (visualEffect == abilityHitVFX)
            LightColor(groundLight, abilityColot, .2f);

        if(visualEffect == sparkVFX)
            visualEffect.SetFloat("PosX", VFXDir);
        visualEffect.SendEvent("OnPlay");

        if(shakeCamera)
        Camera.main.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
    }

    public void DirRight()
    {
        VFXDir = -5;
    }

    public void DirLeft()
    {
        VFXDir = 5;
    }

    public void CancelAction()
    {
        if (!targetCam.gameObject.activeSelf && tacticalMode)
            SetTacticalMode(false);

        if (targetCam.gameObject.activeSelf)
            SetAimCamera(false);
    }

    int NearestTargetToCenter()
    {
        float[] distances = new float[targets.Count];

        for (int i = 0; i < targets.Count; i++)
        {
            distances[i] = Vector2.Distance(Camera.main.WorldToScreenPoint(targets[i].position), new Vector2(Screen.width / 2, Screen.height / 2));
        }

        float minDistance = Mathf.Min(distances);
        int index = 0;

        for (int i = 0; i < distances.Length; i++)
        {
            if (minDistance == distances[i])
                index = i;
        }
        return index;
    }

    public void LightColor(Light l, Color x, float time)
    {
        l.DOColor(x, time).OnComplete(() => l.DOColor(Color.black, time));   
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
