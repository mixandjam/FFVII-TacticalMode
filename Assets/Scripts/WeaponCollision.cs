using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable] public class CollisionEvent : UnityEvent<Transform> { }
public class WeaponCollision : MonoBehaviour
{

    public CollisionEvent onHit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            onHit.Invoke(other.transform);
        }
    }
}
