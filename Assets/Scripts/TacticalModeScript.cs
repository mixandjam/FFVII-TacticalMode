using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using Cinemachine;
using UnityEngine.Rendering;
using DG.Tweening;

[System.Serializable] public class GameEvent : UnityEvent { }
[System.Serializable] public class TacticalModeEvent : UnityEvent<bool> { }

public class TacticalModeScript : MonoBehaviour
{
    [HideInInspector]
    public GameEvent OnAttack;
    [HideInInspector]
    public GameEvent OnModificationATB;
    [HideInInspector]
    public TacticalModeEvent OnTacticalTrigger;
    [HideInInspector]
    public TacticalModeEvent OnTargetSelectTrigger;

    private MovementInput movement;
    private Animator anim;
    public WeaponCollision weapon;

    [Header("Time Stats")]
    public float slowMotionTime = .005f;

    [Space]

    public bool tacticalMode;
    public bool isAiming;
    public bool usingAbility;
    public bool dashing;

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

    private CinemachineImpulseSource camImpulseSource;

    private void Start()
    {
        weapon.onHit.AddListener((target) => HitTarget(target));
        movement = GetComponent<MovementInput>();
        anim = GetComponent<Animator>();
        camImpulseSource = Camera.main.GetComponent<CinemachineImpulseSource>();
    }

    void Update()
    {
        if (targets.Count > 0 && !tacticalMode && !usingAbility)
        {
            targetIndex = NearestTargetToCenter();
            aimObject.LookAt(targets[targetIndex]);
        }

        //Attack
        if ((Input.GetMouseButtonDown(0)) && !tacticalMode && !usingAbility)
        {

            OnAttack.Invoke();

            if(!dashing)
                //MoveTowardsTarget(targets[targetIndex]);
            anim.SetTrigger("slash");
        }


        if (Input.GetMouseButtonDown(1) && !usingAbility)
        {
            if (atbCount > 0 && !tacticalMode)
                SetTacticalMode(true);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelAction();
        }
    }

    public void SpinAttack()
    {
        ModifyATB(-100);

        StartCoroutine(AbilityCooldown());

        SetTacticalMode(false);

        MoveTowardsTarget(targets[targetIndex]);

        //Animation
        anim.SetTrigger("ability");

        //Polish
        PlayVFX(abilityVFX, false);
        LightColor(groundLight, abilityColot, .3f);
    }

    public void Heal()
    {
        ModifyATB(-100);

        StartCoroutine(AbilityCooldown());

        SetTacticalMode(false);

        //Animation
        anim.SetTrigger("heal");

        //Polish
        PlayVFX(healVFX, false);
        LightColor(groundLight, healColor, .5f);
    }

    public void MoveTowardsTarget(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > 1 && Vector3.Distance(transform.position, target.position) < 10)
        {
            StartCoroutine(DashCooldown());
            transform.DOMove(TargetOffset(), .5f);
            transform.DOLookAt(targets[targetIndex].position, .2f);
        }
    }

    IEnumerator AbilityCooldown()
    {
        usingAbility = true;
        yield return new WaitForSeconds(1f);
        usingAbility = false;
    }

    IEnumerator DashCooldown()
    {
        dashing = true;
        yield return new WaitForSeconds(1);
        dashing = false;
    }

    public Vector3 TargetOffset()
    {
        Vector3 position;
        position = targets[targetIndex].position;
        return Vector3.MoveTowards(position, transform.position, 1.2f);
    }

    public void HitTarget(Transform x)
    {
        OnModificationATB.Invoke();

        PlayVFX(sparkVFX, true);
        if (usingAbility)
            PlayVFX(abilityHitVFX, true, 4,4, .3f);

        ModifyATB(25);

        LightColor(swordLight, sparkColor, .1f);

        if (x.GetComponent<EnemyScript>() != null)
        {
            x.GetComponent<EnemyScript>().GetHit();
        }
    }

    public void ModifyATB(float amount)
    {
        OnModificationATB.Invoke();

        atbSlider += amount;
        atbSlider = Mathf.Clamp(atbSlider, 0, (filledAtbValue * 2));

        if (amount > 0)
        {
            if (atbSlider >= filledAtbValue && atbCount == 0)
                atbCount = 1;
            if (atbSlider >= (filledAtbValue * 2) && atbCount == 1)
                atbCount = 2;
        }
        else
        {
            if (atbSlider <= filledAtbValue)
                atbCount = 0;
            if (atbSlider >= filledAtbValue && atbCount == 0)
                atbCount = 1;
        }

        OnModificationATB.Invoke();
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
        //movement.enabled = !on;

        if (!on)
        {
            SetAimCamera(false);
        }

        camImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = on ? 0 : 2;

        float time = on ? slowMotionTime : 1;
        Time.timeScale = time;

        //Polish
        DOVirtual.Float(on ? 0 : 1, on ? 1 : 0, .3f, SlowmotionPostProcessing).SetUpdate(true);

        OnTacticalTrigger.Invoke(on);
    }

    public void SelectTarget(int index)
    {
        targetIndex = index;
        aimObject.DOLookAt(targets[targetIndex].position, .3f).SetUpdate(true);
    }

    public void SetAimCamera(bool on)
    {
        if (targets.Count < 1)
            return;

        OnTargetSelectTrigger.Invoke(on);

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

    public void PlayVFX(VisualEffect visualEffect, bool shakeCamera, float shakeAmplitude = 2, float shakeFrequency = 2, float shakeSustain = .2f)
    {
        if (visualEffect == abilityHitVFX)
            LightColor(groundLight, abilityColot, .2f);

        if(visualEffect == sparkVFX)
            visualEffect.SetFloat("PosX", VFXDir);
        visualEffect.SendEvent("OnPlay");

        camImpulseSource.m_ImpulseDefinition.m_AmplitudeGain = shakeAmplitude;
        camImpulseSource.m_ImpulseDefinition.m_FrequencyGain = shakeFrequency;
        camImpulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = shakeSustain;

        if (shakeCamera)
            camImpulseSource.GenerateImpulse();
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
