using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarOdometer : MonoBehaviour
{
    public float miles = 0.0f;
    
    private Transform[] display;
    
    //Zero and Nine points of the discs. New number every 18 degrees
    private const float ZERO = -80.0f;
    private const float NINE = 80.0f;
    private const float ZERO_2 = 100.0f;
    private const float NINE_2 = -100.0f;
    
    public void Awake()
    {
        display = new Transform[7];
        
        foreach(Transform child in transform)
        {
            switch(child.name)
            {
                case "First Digit":
                    display[5] = child;
                    break;
                    
                case "Second Digit":
                    display[4]  = child;
                    break;
                    
                case "Third Digit":
                    display[3]  = child;
                    break;
                    
                case "Fourth Digit":
                    display[2]  = child;
                    break;
                    
                case "Fifth Digit":
                    display[1]  = child;
                    break;
                    
                case "Sixth Digit":
                    display[0]  = child;
                    break;
                    
                case "Decimal Digit":
                    display[6]  = child;
                    break;
            }
        }
        
        //If no milage, ensure odometer is reset
        if(miles == 0.0f)
        {
            foreach(Transform digit in display)
            {
                digit.localRotation = Quaternion.Euler(ZERO, 0.0f, 90.0f);
            }
        }
    }
    
    public void SetMiles(float m)
    {
        miles = m;
        UpdateDisplay();
    }
    
    public float GetMiles()
    {
        return miles;
    }
    
    public void AddMiles(float m)
    {
        miles += m;
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        float decimalPart = miles - (int)miles;
        float firstDigit = miles - ((int)miles / 10 * 10);
        float secondDigit = miles - ((int)miles / 100 * 100) - 10;
        Debug.Log(secondDigit);
        float thirdDigit = 0.0f;
        float fourthDigit = 0.0f;
        float fifthDigit = 0.0f;
        float sixthDigit = 0.0f;
        
        display[6].localRotation = Quaternion.Euler(ZERO + (18.0f * (decimalPart * 10.0f)), 0.0f, 90.0f);
        display[5].localRotation = Quaternion.Euler(ZERO + (18.0f * firstDigit), 0.0f, 90.0f);
        display[4].localRotation = Quaternion.Euler(ZERO + (18.0f * secondDigit), 0.0f, 90.0f);
        display[3].localRotation = Quaternion.Euler(ZERO + (18.0f * thirdDigit), 0.0f, 90.0f);
        display[2].localRotation = Quaternion.Euler(ZERO + (18.0f * fourthDigit), 0.0f, 90.0f);
        display[1].localRotation = Quaternion.Euler(ZERO + (18.0f * fifthDigit), 0.0f, 90.0f);
        display[0].localRotation = Quaternion.Euler(ZERO + (18.0f * sixthDigit), 0.0f, 90.0f);
    }
}
