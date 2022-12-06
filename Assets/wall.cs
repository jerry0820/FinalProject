using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wall : MonoBehaviour
{
    public float health;
    void Update()
    {
        Debug.Log(health);
        if(health <= 0){
            Destroy(gameObject);
        }
    }
}
