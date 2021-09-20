using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidContainer : MonoBehaviour
{
    //Capacity and level in gallons
    public float capacity = 5.0f;
    public float currentLevel = 0.0f;
    
    public FluidType containedFluid = FluidType.NONE;
    
    public void AddFluid(float amount)
    {
        if(currentLevel + amount < capacity)
            currentLevel += amount;
            
        else
            currentLevel = capacity;
    }
    
    public void RemoveFluid(float amount)
    {
        if(currentLevel - amount > 0.0f)
            currentLevel -= amount;
            
        else
            currentLevel = 0.0f;
    }
}
