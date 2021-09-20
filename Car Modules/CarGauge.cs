using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarGauge : MonoBehaviour
{
    public float startAngle = 0.0f;
    public float endAngle = 0.0f;
    
    //Used by speedo, tach, battery, temperature, but not fuel
    public float maxValue = 0.0f;
    
    public Material lightsOn, lightsOff, lightsOnNeedle, lightsOffNeedle;
    
    private Transform needle, display;
    private float totalDistance;
    
    // Start is called before the first frame update
    void Awake()
    {
        //Find the needle and display
        foreach(Transform child in transform)
        {
            if(child.name == "Display")
            {
                display = child;
                needle = child.GetChild(0);
            }
        }
                
        //Zero the needle
        needle.localRotation = Quaternion.Euler(0, startAngle, 0);
        
        totalDistance = startAngle - endAngle;
    }

    //Set a gauge to a percentage of its total travel. 0.0-1.0
    public void SetPercentage(float percent)
    {
        float angle;
        
        if(percent < 0.0f)
            angle = startAngle;
            
        else if(percent > 1.0f)
            angle = endAngle;
            
        else
            angle = startAngle - (totalDistance * percent);
            
        needle.localRotation = Quaternion.Euler(0, angle, 0);
    }
    
    //Activate a gauge's backlight (switch from lit to unlit texture)
    public void ActivateBacklight()
    {
        display.GetComponent<MeshRenderer>().material = lightsOn;
        needle.GetChild(0).GetComponent<MeshRenderer>().material = lightsOnNeedle;
    }
    
    //Deactivate a gauge's backlight (switch from unlit to lit texture)
    public void DeactivateBacklight()
    {
        display.GetComponent<MeshRenderer>().material = lightsOff;
        needle.GetChild(0).GetComponent<MeshRenderer>().material = lightsOffNeedle;
    }
}
