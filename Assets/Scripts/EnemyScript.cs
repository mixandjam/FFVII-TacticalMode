using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    private Animator anim;
    public Renderer eyesRenderer;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void GetHit()
    {
        anim.SetTrigger("hit");
        StopCoroutine(EyeHitSprite());
        StartCoroutine(EyeHitSprite());
    }

    IEnumerator EyeHitSprite()
    {
        eyesRenderer.material.SetTextureOffset("_BaseColorMap", new Vector2(0, -.33f));
        yield return new WaitForSeconds(.8f);
        eyesRenderer.material.SetTextureOffset("_BaseColorMap", new Vector2(.66f, 0));

    }

}
