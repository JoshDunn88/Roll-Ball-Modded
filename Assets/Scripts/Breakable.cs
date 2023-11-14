using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    //wooden objects break when shot collides
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Explosion"))
        {
            print("door collided with shot");
            this.gameObject.SetActive(false);
        }
    }
}
