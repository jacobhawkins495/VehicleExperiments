using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTransmission : MonoBehaviour
{
    //Array of gear ratios for this transmission. First value is reverse, second is neutral, third is first, and so on
    public float[] gearRatios;
    
    public float[] topSpeeds;
    
    public int topGear;
    
    public const int REVERSE = 0;
    public const int NEUTRAL = 1;
    public const int FIRST = 2;
    
    void Awake()
    {
        //Negate the reverse gear for convenience
        gearRatios[0] = -gearRatios[0];
        
        //Highest gear
        topGear = gearRatios.Length - 1;
    }
}
