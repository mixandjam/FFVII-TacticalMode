using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
using UnityEngine.Rendering;
using DG.Tweening;

public class TestScript : MonoBehaviour
{
    [Header("Targets in radius")]
    public List<Transform> targets;
    public int count;

    [Space]

    public VisualEffect hey;
    public Volume slowMotionVolume;

    public GameObject gameCam;
    public GameObject targetCam;

    public Transform aimObject;


    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Time.timeScale = (Time.timeScale == 1) ? .05f : 1;
        //    DOVirtual.Float(slowMotionVolume.weight, (slowMotionVolume.weight == 0) ? 1 : 0, .2f, SlowmotionPostProcessing).SetUpdate(true);
        //}

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!targetCam.active)
            {
                Time.timeScale = .1f;
                targetCam.SetActive(true);
                DOVirtual.Float(0, 1, .3f, SlowmotionPostProcessing).SetUpdate(true);
                LookAt();
            }
            else
            {
                DOVirtual.Float(1, 0, .3f, SlowmotionPostProcessing).SetUpdate(true);
                Time.timeScale = 1;
                targetCam.SetActive(false);
            }

        }

        if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int direction = (Input.GetKey(KeyCode.RightArrow)) ? 1 : -1;
            count += direction;
            if (count > targets.Count - 1)
                count = 0;
            if (count < 0)
                count = targets.Count - 1;
            LookAt();

        }
    }

    public void LookAt()
    {
        aimObject.DOLookAt(targets[count].position, .3f).SetUpdate(true);
    }

    public void PlayVFX()
    {
        hey.SendEvent("OnPlay");
        Camera.main.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
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
}
