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
    public Transform targets;

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

    public void Sequence()
    {
        int ran = Random.Range(0, targets.childCount - 1);

        Sequence s = DOTween.Sequence();
        s.Append(transform.DOLookAt(targets.GetChild(ran).position, .35f));
        s.Join(transform.DOMove(targets.GetChild(ran).position, .6f).SetEase(Ease.InCirc));
    }
}
