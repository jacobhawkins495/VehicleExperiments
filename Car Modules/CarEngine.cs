using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : FluidContainer
{
    public FluidType fuelToBurn = FluidType.GAS;
    
    //Engine torque in ft-lbs
    //Discretized as torque provided from 1200RPM to 4800RPM in increments of 400
    public float[] torqueOutput = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    
    //Engine torque in newton-meters. Calculated at start
    private float[] torqueOutputNm = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    
    public int maxRPM, minRPM, peakRPM;
    
    void Awake()
    {
        //Calculate torque in newton-meters because engine torque is always specified in foot-pounds
        for(int i = 0; i < torqueOutput.Length; i++)
        {
            torqueOutputNm[i] = torqueOutput[i] * 0.73756f;
        }
    }
    
    //Returns engine torque
    public float GetTorque(float RPM)
    {
        float lowerBound, fraction;
        
        int index = ((int)RPM - 1200) / 400;
        
        if(RPM < 1200)
        {
            index = 0;
            lowerBound = 0.0f;
        }
        
        else
            lowerBound = torqueOutputNm[index];
        
        if(RPM >= 1200)
        {
            fraction = (RPM - ((index * 400) + 1200)) / 400;
            return lowerBound + (fraction * 400);
        }
            
        else
        {
            fraction = RPM / 1200;
            return lowerBound + (fraction * torqueOutputNm[index]);
        }
    }
}
