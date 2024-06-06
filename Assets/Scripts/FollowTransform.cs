using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followPosX;
    [SerializeField] private bool followPosY;
    [SerializeField] private bool followPosZ;
    [SerializeField] private bool followRotX;
    [SerializeField] private bool followRotY;
    [SerializeField] private bool followRotZ;
    private float posX;
    private float posY;
    private float posZ;
    private float rotX;
    private float rotY;
    private float rotZ;

    // Update is called once per frame
    void Update()
    {
        if (followPosX) posX = target.position.x;
        if (followPosY) posY = target.position.y;
        if (followPosZ) posZ = target.position.z;
        if (followRotX) rotX = target.rotation.x;
        if (followRotY) rotY = target.rotation.y;
        if (followRotZ) rotZ = target.rotation.z;
        transform.position = new Vector3(posX, posY, posZ);
        transform.eulerAngles = new Vector3(rotX, rotY, rotZ);
    }
}
