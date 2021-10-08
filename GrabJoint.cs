using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabJoint : MonoBehaviour
{
    private bool hasJoint = true;
    
    public void OnJointBreak(float force)
    {
        GetComponent<ConfigurableJoint>().connectedBody.useGravity = true;
        gameObject.AddComponent<ConfigurableJoint>();
        hasJoint = false;
    }
    
    public void FixedUpdate()
    {
        if(!hasJoint)
        {
            GetComponent<ConfigurableJoint>().xMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().yMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().zMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().angularXMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().angularYMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().angularZMotion = ConfigurableJointMotion.Locked;
            GetComponent<ConfigurableJoint>().breakForce = 1000.0f;
            hasJoint = true;
        }
    }
}
