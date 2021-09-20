using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRadiator : FluidContainer
{
    //Engine operating temperature in fahrenheit
    private const float TOP_TEMPERATURE = 200;
    private const float TEMP_ADJUST = 0.25f;
    
    //Thermal conductivity of gasoline and oil (0.15), relative to water (0.606)
    private const float HYDROCARBON_THERMAL_CONDUCTIVITY = 0.248f;
    
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
                return TOP_TEMPERATURE * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST));
                
            case FluidType.GAS:
                return TOP_TEMPERATURE * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST)) / HYDROCARBON_THERMAL_CONDUCTIVITY;
                
            case FluidType.OIL:
                return TOP_TEMPERATURE * (1 + ((1 - (currentLevel / capacity)) * TEMP_ADJUST)) / HYDROCARBON_THERMAL_CONDUCTIVITY;
                
            default:
                return 900.0f;
        }
    }
}
