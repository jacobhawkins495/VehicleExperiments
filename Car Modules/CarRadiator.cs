using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRadiator : FluidContainer
{
    //Engine operating temperature in fahrenheit
    private float topTemperature = 225;
    private float particleRate;
    
    private const float TEMP_ADJUST = 0.25f;
    
    
    //Thermal conductivity of gasoline and oil (0.15), relative to water (0.606)
    private const float HYDROCARBON_THERMAL_CONDUCTIVITY = 0.248f;
    
    //Calculate this radiator's base cooling temperature from its surface area and thickness
    public void Awake()
    {
        Transform child = transform.GetChild(0);
        topTemperature -= 300 * child.lossyScale.x * child.lossyScale.y * child.lossyScale.z;
        
        ParticleSystem.EmissionModule em = GetComponent<ParticleSystem>().emission;
        
        particleRate = em.rateOverTime.constant;
    }
    
    /* Returns the minimum temperature this radiator can cool to
     * Depends on:
     * Amount of fluid in reservoir
     * Type of fluid in reservoir
     * Size of radiator
     */
    public float GetMinTemperature()
    {
        switch(containedFluid)
        {
            case FluidType.WATER:
                return topTemperature * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST));
                
            case FluidType.GAS:
                return topTemperature * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST)) / HYDROCARBON_THERMAL_CONDUCTIVITY;
                
            case FluidType.OIL:
                return topTemperature * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST)) / HYDROCARBON_THERMAL_CONDUCTIVITY;
                
            default:
                return 900.0f;
        }
    }
    
    public void SetPercent(float percent)
    {
        ParticleSystem.EmissionModule em = GetComponent<ParticleSystem>().emission;
        em.rateOverTime = particleRate * percent;
    }
    
    public void Overheat()
    {
        if(currentLevel > 0.0f && !GetComponent<ParticleSystem>().isPlaying)
        {
            GetComponent<ParticleSystem>().Play();
        }
    }
    
    public void StopOverheating()
    {
        if(GetComponent<ParticleSystem>().isPlaying)
            GetComponent<ParticleSystem>().Stop();
    }
}
