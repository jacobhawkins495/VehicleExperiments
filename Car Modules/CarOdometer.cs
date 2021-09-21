using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarOdometer : MonoBehaviour
{
    public double miles = 0.0f;
    
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
    
    public double GetMiles()
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
        int milesInt = (int)miles;
        
        //Separate each digit from the float
        float decimalPart = (float)(miles - (int)miles);
        int firstDigitInt = milesInt % 10;
        int secondDigitInt = (milesInt / 10) % 10;
        int thirdDigitInt = (milesInt / 100) % 10;
        int fourthDigitInt = (milesInt / 1000) % 10;
        int fifthDigitInt = (milesInt / 10000) % 10;
        int sixthDigitInt = (milesInt / 100000) % 10;
        
        //Calculate how much further it would be rotated relative to the digit to the right of it
        float firstDigit = firstDigitInt + decimalPart;
        float secondDigit = secondDigitInt + (firstDigit / 10.0f);
        float thirdDigit = thirdDigitInt + (secondDigit / 10.0f);
        float fourthDigit = fourthDigitInt + (thirdDigit / 10.0f);
        float fifthDigit = fifthDigitInt + (fourthDigit / 10.0f);
        float sixthDigit = sixthDigitInt + (fifthDigit / 10.0f);
        
        //Update the rotations
        display[6].localRotation = Quaternion.Euler(ZERO + (18.0f * (decimalPart * 10.0f)), 0.0f, 90.0f);
        display[5].localRotation = Quaternion.Euler(ZERO + (18.0f * firstDigit), 0.0f, 90.0f);
        display[4].localRotation = Quaternion.Euler(ZERO + (18.0f * secondDigit), 0.0f, 90.0f);
        display[3].localRotation = Quaternion.Euler(ZERO + (18.0f * thirdDigit), 0.0f, 90.0f);
        display[2].localRotation = Quaternion.Euler(ZERO + (18.0f * fourthDigit), 0.0f, 90.0f);
        display[1].localRotation = Quaternion.Euler(ZERO + (18.0f * fifthDigit), 0.0f, 90.0f);
        display[0].localRotation = Quaternion.Euler(ZERO + (18.0f * sixthDigit), 0.0f, 90.0f);
    }
}
