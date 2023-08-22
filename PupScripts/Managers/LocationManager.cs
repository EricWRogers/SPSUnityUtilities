using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LocationManager : MonoBehaviour
{
    public UnityEvent locationNotEnabledEvent;
    public UnityEvent<Vector2> gpsUpdate; 
    public TMP_Text logText = null;
    public Transform playerTransform;

    private bool m_locationEnabled = false;

    void Log(string _message)
    {
        if (logText == null)
        {
            print(_message);
        }
        else
        {
            logText.text = _message + " " +playerTransform.localPosition.x + " " +playerTransform.localPosition.y;
        }
    }

    IEnumerator Start()
    {
        Log("Start Location Manager");

        if (locationNotEnabledEvent == null)
            locationNotEnabledEvent = new UnityEvent();
        
        if (gpsUpdate == null)
            gpsUpdate = new UnityEvent<Vector2>();
        
        // Check if the user has location service enabled.
        if (!Input.location.isEnabledByUser)
        {
            Log("Location not enabled.");
            locationNotEnabledEvent.Invoke();
            yield break;
        }

        // Starts the location service.
        Input.location.Start();
        m_locationEnabled = true;

        // Waits until the location service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // If the service didn't initialize in 20 seconds this cancels location service use.
        if (maxWait < 1)
        {
            Log("Timed out");
            yield break;
        }

        // If the connection failed this cancels location service use.
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Log("Unable to determine device location");
            yield break;
        }
        else
        {
            // If the connection succeeded, this retrieves the device's current location and displays it in the Console window.
            Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }

        // Stops the location service if there is no need to query location updates continuously.
        
    }

    float GPSChop(float _location)
    {
        string message = _location.ToString();

        if (message.Length < 5)
            return 0.0f;
        
        if (_location > 0)
        {
            message = message.Remove(0,4);
            message = message.Insert(2,".");
        }
        else
        {
            message = message.Remove(1,4);
            message = message.Insert(3,".");
        }        

        return float.Parse(message);
    }

    public void UpdateLocation()
    {
        if (m_locationEnabled)
        {
            gpsUpdate.Invoke(new Vector2(
                GPSChop(Input.location.lastData.longitude),
                GPSChop(Input.location.lastData.latitude)
                ));
            Log("Location: " + Input.location.lastData.latitude + " " + Input.location.lastData.longitude + " " + Input.location.lastData.altitude + " " + Input.location.lastData.horizontalAccuracy + " " + Input.location.lastData.timestamp);
        }
    }

    void OnDestroy()
    {
        Input.location.Stop();
    }
}
