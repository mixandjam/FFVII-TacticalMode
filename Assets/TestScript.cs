using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Cinemachine;
using UnityEngine.Rendering;
using DG.Tweening;

public class TestScript : MonoBehaviour
{
    public VisualEffect hey;
    public Volume slowMotionVolume;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Time.timeScale = (Time.timeScale == 1) ? .05f : 1;
            DOVirtual.Float(slowMotionVolume.weight, (slowMotionVolume.weight == 0) ? 1 : 0, .2f, SlowmotionPostProcessing).SetUpdate(true);
        }
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
}
