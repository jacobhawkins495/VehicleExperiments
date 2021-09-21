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
    public CarOdometer odometer;
    
    public Transform centerOfMass;

    public List<AxleInfo> axleInfos; 
    public float maxSteeringAngle;
    
    public float engineRPM = 0.0f;
    public float trueEngineRPM = 0.0f;
    public float engineTorque = 0.0f;
    public float gearboxTorque = 0.0f;
    
    public float engineLoad = 0.0f;
    public float currentSpeed = 0.0f;
    public float engineTemperature = 90.0f;
    
    public float ambientTemperature = 90.0f;
    
    /* Current transmission gear
     *  0  Reverse
     *  1  Neutral
     *  2+ Drive
     */
    public int currentGear = CarTransmission.NEUTRAL;
    
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
    private const float MIN_ENGINE_OVERHEAT = 200.0f;
     
    //Finds the corresponding visual wheel
    //Correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider == null || collider.transform.childCount == 0) 
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
                
                if(leftHeadlight != null)
                    leftHeadlight.Deactivate();
                    
                if(rightHeadlight != null)
                    rightHeadlight.Deactivate();
                    
                if(leftTaillight != null)
                    leftTaillight.DeactivatePrimary();
                    
                if(rightTaillight != null)
                    rightTaillight.DeactivatePrimary();
                    
                if(speedometer != null)
                    speedometer.DeactivateBacklight();
                    
                if(tachometer != null)
                    tachometer.DeactivateBacklight();
                    
                if(gasometer != null)
                    gasometer.DeactivateBacklight();
                    
                if(thermometer != null)
                    thermometer.DeactivateBacklight();
            }

            //If the lights are on but the highbeams are not, turn on the highbeams
            else
            {
                highbeamsOn = true;
                
                if(leftHeadlight != null)
                    leftHeadlight.ActivateSecondary();
                    
                if(rightHeadlight != null)
                    rightHeadlight.ActivateSecondary();
            }
        }

        //If the lights are not on, turn them on
        else
        {
            lightsOn = true;
            
            if(leftHeadlight != null)
                leftHeadlight.ActivatePrimary();
                
            if(rightHeadlight != null)
                rightHeadlight.ActivatePrimary();
                
            if(leftTaillight != null)
                leftTaillight.ActivatePrimary();
                
            if(rightTaillight != null)
                rightTaillight.ActivatePrimary();
            
            if(speedometer != null)
                speedometer.ActivateBacklight();
                
            if(tachometer != null)
                tachometer.ActivateBacklight();
                
            if(gasometer != null)
                gasometer.ActivateBacklight();
                
            if(thermometer != null)
                thermometer.ActivateBacklight();
        }
    }
    
    //Apply brakes to the wheels. Provide 0 to release the brakes
    private void ApplyBrakes(float strength)
    {
        foreach(AxleInfo axle in axleInfos)
        {
            if(axle.leftWheel != null)
                axle.leftWheel.brakeTorque = strength;
                
            if(axle.rightWheel != null)
                axle.rightWheel.brakeTorque = strength;
        }
    }
    
    private bool CanEngineRun()
    {
        if(engine == null)
            return false;
            
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
            
            if(leftTaillight != null)
                leftTaillight.ActivateSecondary();
                
            if(rightTaillight != null)
                rightTaillight.ActivateSecondary();
                
            currentGear = CarTransmission.REVERSE;
        }
        
        //Turn off the reverse lights if the player presses "forwards"
        else if(reverseLightsOn && Input.GetAxis("Vertical") > 0)
        {
            reverseLightsOn = false;
            
            if(leftTaillight != null)
                leftTaillight.DeactivateSecondary();
                
            if(rightTaillight != null)
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
        if(odometer != null)
            odometer.AddMiles(traveledDistance);
        
        if(engine != null && radiator != null)
        {
            gasTank.RemoveFluid((traveledDistance / engine.mileage) * (engineRPM / engine.peakRPM));
            engine.RemoveFluid((traveledDistance / engine.oilMileage) * (engineRPM / engine.maxRPM));
            radiator.RemoveFluid((traveledDistance / engine.coolantMileage) * (engineRPM / engine.maxRPM));
        }
        
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
                        
                        if(leftTaillight != null)
                            leftTaillight.ActivateTertiary();
                            
                        if(rightTaillight != null)
                            rightTaillight.ActivateTertiary();
                    }

                    else
                    {
                        ApplyBrakes(0.0f);
                        
                        if(leftTaillight != null)
                            leftTaillight.DeactivateTertiary();
                            
                        if(rightTaillight != null)
                            rightTaillight.DeactivateTertiary();
                    }
                }

                else if(Input.GetAxis("Vertical") < 0.0f)
                {
                    if(movingForward)
                    {
                        ApplyBrakes(1000.0f * -Input.GetAxis("Vertical"));
                        
                        if(leftTaillight != null)
                            leftTaillight.ActivateTertiary();
                            
                        if(rightTaillight != null)
                            rightTaillight.ActivateTertiary();
                    }

                    else
                    {
                        ApplyBrakes(0.0f);
                        
                        if(leftTaillight != null)
                            leftTaillight.DeactivateTertiary();
                            
                        if(rightTaillight != null)
                            rightTaillight.DeactivateTertiary();   
                            
                        currentGear = CarTransmission.REVERSE;
                    }   
                }
            }

            //Shifting out of neutral
            if(transmission != null && currentGear == CarTransmission.NEUTRAL && Input.GetAxis("Vertical") != 0.0f)
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
            float maxTemp = 1000.0f;
            
            if(transmission != null && radiator != null)
                maxTemp = radiator.GetMinTemperature() - (10 * (currentSpeed / transmission.topSpeeds[transmission.topSpeeds.Length - 1]));
            
            if(engineTemperature < maxTemp)
            {
                //Engine overheats faster if no radiator is present
                if(radiator == null)
                    engineTemperature += 5.0f * (engineRPM / engine.maxRPM) * (1 - engineLoad);
                    
                else
                    engineTemperature += 0.2f * (engineRPM / engine.maxRPM) * (1 - engineLoad);
            }
            
            else
            {
                //Engine cools down to below the radiator's max temperature
                engineTemperature -= 0.1f;
            }

            //Shift up
            if(transmission != null && engineRPM > engine.peakRPM && currentGear < transmission.topGear && currentGear > CarTransmission.NEUTRAL && currentSpeed > transmission.topSpeeds[currentGear] * 0.8f && currentSpeed > prevSpeed)
            {
                engineRPM = engineRPM * (currentSpeed / transmission.topSpeeds[currentGear + 1]);
                currentGear++;
            }

            //Shift down
            if(transmission != null && currentGear > CarTransmission.FIRST && currentSpeed < transmission.topSpeeds[currentGear] * 0.6f && currentSpeed < prevSpeed)
            {
                engineRPM = engineRPM * (currentSpeed / transmission.topSpeeds[currentGear]);
                currentGear--;
            }

            //Reverse
            if(currentGear == CarTransmission.FIRST && movingForward && Input.GetAxis("Vertical") < 0.0f)
                currentGear = CarTransmission.REVERSE;
                
            //Engine RPM affected by overheating
            trueEngineRPM = engineRPM * Mathf.Clamp(1.0f - ((engineTemperature - MAX_ENGINE_TEMP) / 10.0f), 0.0f, 1.0f);
            
            //If the RPM drops too hard, the engine stalls
            if(engine != null && engineTemperature > MAX_ENGINE_TEMP && trueEngineRPM < engine.minRPM)
            {
                engineRunning = false;
                engineRPM = 0.0f;
                Debug.Log("Engine overheated too much");
            }

            engineTorque = engine.GetTorque(trueEngineRPM);

            float engineTorqueNM = engineTorque / 0.73756f;
            float topSpeedCoefficient = 0.0f; 
            float transmissionSpeedCoefficient = 0.0f;
            
            if(engine != null && transmission != null)
            {
                topSpeedCoefficient = 1.5f - (currentGear == CarTransmission.REVERSE ? (currentSpeed / transmission.topSpeeds[CarTransmission.REVERSE]) : (currentSpeed / transmission.topSpeeds[transmission.topGear]));
                transmissionSpeedCoefficient = (currentGear == 1 ? 0 : (((topSpeedCoefficient * 2.2f) - (currentSpeed / transmission.topSpeeds[currentGear]))) * (engineRPM / engine.maxRPM));

                gearboxTorque = transmission.gearRatios[currentGear] * engineTorqueNM * engineLoad * transmissionSpeedCoefficient;
            }
            
            //If engine is overheating, produce steam
            if(radiator != null && engineTemperature > MAX_ENGINE_TEMP)
            { 
                radiator.Overheat();
            }
        }
        
        //Engine not running
        else
        {
            if(engineTemperature > ambientTemperature)
                engineTemperature -= 0.01f * (engineTemperature / ambientTemperature);
                
            if(trueEngineRPM > 0)
            {
                trueEngineRPM -= 65;
            }
        }
        
        if(radiator != null && engineTemperature > MAX_ENGINE_TEMP && engineRunning)
        {
            radiator.SetPercent(Mathf.Clamp((engineTemperature - MAX_ENGINE_TEMP) / 10.0f, 0.0f, 1.0f));
        }
        
        else if(radiator != null && engineTemperature > MIN_ENGINE_OVERHEAT)
        {
            radiator.SetPercent(Mathf.Clamp((engineTemperature - MIN_ENGINE_OVERHEAT) / 40.0f, 0.0f, 1.0f));
        }
        
        //Stop producing steam after a while
        if(radiator != null && engineTemperature < MIN_ENGINE_OVERHEAT)
        {
            radiator.StopOverheating();
        }

        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        //Apply forces to the wheels
        foreach (AxleInfo axleInfo in axleInfos) 
        {
            if (axleInfo.steering)
            {
                if(axleInfo.leftWheel != null)
                    axleInfo.leftWheel.steerAngle = steering;
                    
                if(axleInfo.rightWheel != null)
                    axleInfo.rightWheel.steerAngle = steering;
            }

            if (axleInfo.motor) 
            {
                if(currentGear != CarTransmission.NEUTRAL)
                {
                    if(axleInfo.leftWheel != null)
                        axleInfo.leftWheel.motorTorque = gearboxTorque / 2.0f;
                        
                    if(axleInfo.rightWheel != null)
                        axleInfo.rightWheel.motorTorque = gearboxTorque / 2.0f;
                }

                else if(currentGear == CarTransmission.NEUTRAL)
                {
                    //For some reason the car won't roll downhill if wheel torque is set to 0, so it must be set to a nonzero value
                    if(axleInfo.leftWheel != null && axleInfo.leftWheel.motorTorque != 0.00001f)
                        axleInfo.leftWheel.motorTorque = 0.00001f;
                        
                    if(axleInfo.rightWheel != null && axleInfo.rightWheel.motorTorque != 0.00001f)
                        axleInfo.rightWheel.motorTorque = 0.00001f;
                }
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
        
        //Update gauges
        if(tachometer != null)
            tachometer.SetPercentage(trueEngineRPM / tachometer.maxValue);
            
        if(speedometer != null)
            speedometer.SetPercentage(currentSpeed / speedometer.maxValue);
            
        if(gasometer != null)
            gasometer.SetPercentage(gasTank.currentLevel / gasTank.capacity);
            
        if(thermometer != null)
            thermometer.SetPercentage(engineTemperature / thermometer.maxValue);
    }
}
