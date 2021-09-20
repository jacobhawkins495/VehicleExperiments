using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo 
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    
    public bool motor;
    public bool steering;
}
     
public class CarController : MonoBehaviour 
{
    public CarLight leftHeadlight, rightHeadlight, leftTaillight, rightTaillight;
    public CarEngine engine;
    public CarTransmission transmission;
    public FluidContainer gasTank;
    public CarRadiator radiator;
    public CarGauge speedometer, tachometer, gasometer, thermometer;
    
    public Transform centerOfMass;

    public List<AxleInfo> axleInfos; 
    public float maxSteeringAngle;
    
    public float engineRPM = 0.0f;
    public float engineTorque = 0.0f;
    public float gearboxTorque = 0.0f;
    
    public float engineLoad = 0.0f;
    public float currentSpeed = 0.0f;
    public float engineTemperature = 90.0f;
    
    public float ambientTemperature = 90.0f;
    public float radiatorTemp;
    
    /* Current transmission gear
     *  0  Reverse
     *  1  Neutral
     *  2+ Drive
     */
    public int currentGear = CarTransmission.NEUTRAL;
    
    public float odometer = 0.0f;
    
    private Vector3 forwardVector, prevPos, curPos, movement;
    private float prevSpeed = 0.0f;
    
    private bool lightsOn = false;
    private bool highbeamsOn = false;
    private bool movingForward = false;
    private bool movingBackward = false;
    private bool reverseLightsOn = false;
    private bool handbrakeOn = true;
    private bool engineRunning = false;
    
    //Convert meters per 50th of a second to MPH
    private const float MPFS_TO_MPH = 111.84681460272f;
    private const float MAX_ENGINE_TEMP = 230.0f;
     
    //Finds the corresponding visual wheel
    //Correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0) 
        {
            return;
        }
     
        Transform visualWheel = collider.transform.GetChild(0);
     
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);
     
        visualWheel.transform.position = position;
        //Adjusted for using Unity cylinders
        visualWheel.transform.rotation = rotation * Quaternion.Euler(0, 0, 90);
    }
    
    private void ToggleLights()
    {
        if(lightsOn)
        {   
            //If the highbeams are on, turn off all lights
            if(highbeamsOn)
            {
                lightsOn = highbeamsOn = false;
                leftHeadlight.Deactivate();
                rightHeadlight.Deactivate();
                leftTaillight.DeactivatePrimary();
                rightTaillight.DeactivatePrimary();
                speedometer.DeactivateBacklight();
                tachometer.DeactivateBacklight();
                gasometer.DeactivateBacklight();
                thermometer.DeactivateBacklight();
            }

            //If the lights are on but the highbeams are not, turn on the highbeams
            else
            {
                highbeamsOn = true;
                leftHeadlight.ActivateSecondary();
                rightHeadlight.ActivateSecondary();
            }
        }

        //If the lights are not on, turn them on
        else
        {
            lightsOn = true;
            leftHeadlight.ActivatePrimary();
            rightHeadlight.ActivatePrimary();
            leftTaillight.ActivatePrimary();
            rightTaillight.ActivatePrimary();
            speedometer.ActivateBacklight();
            tachometer.ActivateBacklight();
            gasometer.ActivateBacklight();
            thermometer.ActivateBacklight();
        }
    }
    
    //Apply brakes to the wheels. Provide 0 to release the brakes
    private void ApplyBrakes(float strength)
    {
        foreach(AxleInfo axle in axleInfos)
        {
            axle.leftWheel.brakeTorque = strength;
            axle.rightWheel.brakeTorque = strength;
        }
    }
    
    private bool CanEngineRun()
    {
        bool fuelCheck = gasTank.currentLevel > 0.0f && gasTank.containedFluid == FluidType.GAS;
        bool engineCheck = engine.currentLevel > 0.0f && engine.containedFluid == FluidType.OIL;
        
        return fuelCheck && engineCheck;
    }
    
    public void Awake()
    {
        ApplyBrakes(1000.0f);
    }
    
    public void Update()
    {
        //Lights stuff
        if(Input.GetButtonUp("Lights"))
        {
            ToggleLights();
        }
        
        //Turn on the reverse lights if the player presses "reverse" and the car begins to move backwards
        if(Input.GetAxis("Vertical") < 0 && movingBackward && !reverseLightsOn)
        {
            reverseLightsOn = true;
            leftTaillight.ActivateSecondary();
            rightTaillight.ActivateSecondary();
            currentGear = CarTransmission.REVERSE;
        }
        
        //Turn off the reverse lights if the player presses "forwards"
        else if(reverseLightsOn && Input.GetAxis("Vertical") > 0)
        {
            reverseLightsOn = false;
            leftTaillight.DeactivateSecondary();
            rightTaillight.DeactivateSecondary();
            currentGear = CarTransmission.FIRST;
        }
        
        //Handbrake controls
        if(Input.GetButtonUp("Handbrake"))
        {
            if(handbrakeOn)
            {
                ApplyBrakes(0.0f);
                handbrakeOn = false;
            }
            
            else
            {
                ApplyBrakes(1000.0f);
                handbrakeOn = true;
                currentGear = CarTransmission.NEUTRAL;
            }
        }
        
        if(!engineRunning && Input.GetButtonUp("Ignition") && CanEngineRun())
        {
            engineRunning = true;
        }
        
        else if(engineRunning && Input.GetButtonUp("Ignition"))
        {
            engineRunning = false;
            engineRPM = 0;
            currentGear = CarTransmission.NEUTRAL;
        }
    }
     
    public void FixedUpdate()
    {
        //Adjustable center of mass
        GetComponent<Rigidbody>().centerOfMass = centerOfMass.localPosition;
        
        //Tracking the motion vector of the vehicle
        //Used for reverse light functionality
        curPos = this.transform.position;
        movement = curPos - prevPos;
        
        //Figure out of car is moving forward or not
        float dot = Vector3.Dot(forwardVector, movement);
        
        if(dot < -0.01f)
        {
            movingForward = false;
            movingBackward = true;
        }
        
        else if(dot > 0.01f)
        {
            movingForward = true;
            movingBackward = false;
        }
        
        else
        {
            movingForward = false;
            movingBackward = false;
        }
        
        forwardVector = this.transform.forward;
        prevPos = this.transform.position;
        
        //Stupid way to get the car's forward speed in miles per hour
        prevSpeed = currentSpeed;
        currentSpeed = Mathf.Abs(dot * MPFS_TO_MPH);
        
        //Distance traveled in miles
        float traveledDistance = currentSpeed / 60 / 60 / 50;
        
        //Update odometer and gas tank and oil
        odometer += traveledDistance;
        gasTank.RemoveFluid((traveledDistance / engine.mileage) * (engineRPM / engine.peakRPM));
        engine.RemoveFluid((traveledDistance / engine.oilMileage) * (engineRPM / engine.maxRPM));
        radiator.RemoveFluid((traveledDistance / engine.coolantMileage) * (engineRPM / engine.maxRPM));
        
        //Shut off the engine if it runs out of oil or fuel
        if(!CanEngineRun())
        {
            engineRPM = 0;
            currentGear = CarTransmission.NEUTRAL;
            engineRunning = false;
        }
        
        if(engineRunning)
        {
            //Reverse/Forward Stuff
            if(!handbrakeOn)
            {
                if(Input.GetAxis("Vertical") > 0.0f)
                {
                    if(movingBackward)
                    {
                        ApplyBrakes(1000.0f * Input.GetAxis("Vertical"));
                        leftTaillight.ActivateTertiary();
                        rightTaillight.ActivateTertiary();
                    }

                    else
                    {
                        ApplyBrakes(0.0f);
                        leftTaillight.DeactivateTertiary();
                        rightTaillight.DeactivateTertiary();
                    }
                }

                else if(Input.GetAxis("Vertical") < 0.0f)
                {
                    if(movingForward)
                    {
                        ApplyBrakes(1000.0f * -Input.GetAxis("Vertical"));
                        leftTaillight.ActivateTertiary();
                        rightTaillight.ActivateTertiary();
                    }

                    else
                    {
                        ApplyBrakes(0.0f);
                        leftTaillight.DeactivateTertiary();
                        rightTaillight.DeactivateTertiary();    
                        currentGear = CarTransmission.REVERSE;
                    }   
                }
            }

            //Shifting out of neutral
            if(currentGear == CarTransmission.NEUTRAL && Input.GetAxis("Vertical") != 0.0f)
            {
                if(Input.GetAxis("Vertical") > 0.0f)
                    currentGear = CarTransmission.FIRST;

                else
                    currentGear = CarTransmission.REVERSE;
            }

            float targetRPM = (Input.GetAxis("Vertical") == 0.0f ? 1000.0f : engine.maxRPM * Mathf.Abs(Input.GetAxis("Vertical")));

            //Engine load coefficient. Currently just based on the x angle of the car
            engineLoad = 1 - Mathf.Min(30, (transform.eulerAngles.x > 180 ? 360 - transform.eulerAngles.x : transform.eulerAngles.x)) / 30;

            if(engineRPM < targetRPM)
            {
                if(targetRPM == 1000.0f)
                    engineRPM += 100;

                else
                    engineRPM += 5 * engineLoad;
            }

            else if(Input.GetAxis("Vertical") == 0)
            {
                engineRPM -= 65;
            }
            
            //Calculate engine temperature
            float maxTemp = radiator.GetMinTemperature() - (10 * (currentSpeed / transmission.topSpeeds[transmission.topSpeeds.Length - 1]));
            radiatorTemp = maxTemp;
            
            if(engineTemperature < maxTemp)
            {
                engineTemperature += 0.2f * (engineRPM / engine.maxRPM) * (1 - engineLoad);
            }
            
            else
            {
                engineTemperature -= 0.1f;
            }

            //Shift up
            if(engineRPM > engine.peakRPM && currentGear < transmission.topGear && currentGear > CarTransmission.NEUTRAL && currentSpeed > transmission.topSpeeds[currentGear] * 0.8f && currentSpeed > prevSpeed)
            {
                engineRPM = engineRPM * (currentSpeed / transmission.topSpeeds[currentGear + 1]);
                currentGear++;
            }

            //Shift down
            if(currentGear > CarTransmission.FIRST && currentSpeed < transmission.topSpeeds[currentGear] * 0.6f && currentSpeed < prevSpeed)
            {
                engineRPM = engineRPM * (currentSpeed / transmission.topSpeeds[currentGear]);
                currentGear--;
            }

            //Reverse
            if(currentGear == CarTransmission.FIRST && movingForward && Input.GetAxis("Vertical") < 0.0f)
                currentGear = CarTransmission.REVERSE;

            engineTorque = engine.GetTorque(engineRPM);

            float engineTorqueNM = engine.GetTorque(engineRPM) / 0.73756f;
            float topSpeedCoefficient = 1.5f - (currentGear == CarTransmission.REVERSE ? (currentSpeed / transmission.topSpeeds[CarTransmission.REVERSE]) : (currentSpeed / transmission.topSpeeds[transmission.topGear]));
            float transmissionSpeedCoefficient = (currentGear == 1 ? 0 : (((topSpeedCoefficient * 1.5f) - (currentSpeed / transmission.topSpeeds[currentGear]))) * (engineRPM / engine.maxRPM));

            gearboxTorque = transmission.gearRatios[currentGear] * engineTorqueNM * engineLoad * transmissionSpeedCoefficient;
        }
        
        //Engine not running
        else
        {
            if(engineTemperature > ambientTemperature)
                engineTemperature -= 0.1f * (engineTemperature / ambientTemperature);
        }

        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        //Apply forces to the wheels
        foreach (AxleInfo axleInfo in axleInfos) 
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor) 
            {
                if(currentGear != CarTransmission.NEUTRAL)
                {
                    axleInfo.leftWheel.motorTorque = gearboxTorque;
                    axleInfo.rightWheel.motorTorque = gearboxTorque;
                }

                else if(axleInfo.leftWheel.motorTorque != 0.00001f && axleInfo.rightWheel.motorTorque != 0.00001f && currentGear == CarTransmission.NEUTRAL && !engineRunning)
                {
                    axleInfo.leftWheel.motorTorque = 0.00001f;
                    axleInfo.rightWheel.motorTorque = 0.00001f;
                }
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        
        //Update gauges
        tachometer.SetPercentage(engineRPM / tachometer.maxValue);
        speedometer.SetPercentage(currentSpeed / speedometer.maxValue);
        gasometer.SetPercentage(gasTank.currentLevel / gasTank.capacity);
        thermometer.SetPercentage(engineTemperature / thermometer.maxValue);
    }
}
