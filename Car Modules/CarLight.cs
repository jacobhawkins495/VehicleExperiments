using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Vehicle light module.
 * Allows for independent operation of two light sources and an arbitrary number
 * of "illuminating objects" (Objects that change color based on whether or not a light is on)
 */
public class CarLight : MonoBehaviour
{
    public Material primaryOn, primaryOff, secondaryOn, secondaryOff;
    
    private List<Transform> primaryObjects, secondaryObjects;
    
    //Tertiary is for brake lights when rear taillights are already on
    private Light primary, secondary;
    private bool primaryEnabled = false;
    
    // Start is called before the first frame update
    void Start()
    {
        primaryObjects = new List<Transform>();
        secondaryObjects = new List<Transform>();
        
        //Get all the illuminating objects 
        for(int i = 0; i < transform.childCount; i++)
        {
            Transform trans = transform.GetChild(i);
            
            if(trans.tag == "IlluminatingObject")
            {
				//Fetch their light objects
                if(trans.name == "Primary")
                {
                    if(trans.childCount > 0)
					    primary = trans.GetChild(0).GetComponent<Light>();
                        
                    primaryObjects.Add(trans);
                }
					
				else if(trans.name == "Secondary")
                {
                    if(trans.childCount > 0)
					    secondary = trans.GetChild(0).GetComponent<Light>();
                        
                    secondaryObjects.Add(trans);
                }
            }
        }
    }
    
    private void ChangePrimaryMaterial(Material m)
    {
        foreach(Transform lightObject in primaryObjects)
            lightObject.GetComponent<MeshRenderer>().material = m;
    }
    
    private void ChangeSecondaryMaterial(Material m)
    {
        foreach(Transform lightObject in secondaryObjects)
            lightObject.GetComponent<MeshRenderer>().material = m;
    }

    //Activate the first light source
    public void ActivatePrimary()
    {
        ChangePrimaryMaterial(primaryOn);
        
        primary.enabled = true;
        primaryEnabled = true;
    }
    
    //Activate the second light source
    public void ActivateSecondary()
    {
        ChangeSecondaryMaterial(secondaryOn);
        
        secondary.enabled = true;
    }
    
    //Activate the third light source
    public void ActivateTertiary()
    {
        if(primaryEnabled)
            primary.intensity = 0.6f;
            
        else
        {
            ChangePrimaryMaterial(primaryOn);
            primary.enabled = true;
        }
    }
    
    //Deactivate the first light source
    public void DeactivatePrimary()
    {
        ChangePrimaryMaterial(primaryOff);

        primary.enabled = false;
        primaryEnabled = false;
    }
    
    //Deactivate the second light source
    public void DeactivateSecondary()
    {
        ChangeSecondaryMaterial(secondaryOff);
        
        secondary.enabled = false;
    }
    
    public void DeactivateTertiary()
    {
        if(primaryEnabled)
            primary.intensity = 0.3f;
            
        else
        {
            ChangePrimaryMaterial(primaryOff);
            primary.enabled = false;
        }
    }
    
    //Convenience method. Deactivate both light sources
    public void Deactivate()
    {
        DeactivatePrimary();
        DeactivateSecondary();
    }
}
