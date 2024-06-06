using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTime : MonoBehaviour
{
    public float timeUntilDestroy;
    private float currTime;

    // Update is called once per frame
    void Update()
    {
        currTime+= Time.deltaTime;
        if(currTime >= timeUntilDestroy)
        {
            Destroy(this.gameObject);
        }
    }
}
