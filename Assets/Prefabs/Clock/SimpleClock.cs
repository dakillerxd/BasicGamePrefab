using UnityEngine;
using System;

public class SimpleClock : MonoBehaviour
{
    [Header("Clock Hands")]
    [SerializeField] private Transform hourPivotHand;
    [SerializeField] private Transform minutePivotHand;
    [SerializeField] private Transform secondPivotHand;

    [Header("Custom Time Settings")]
    [SerializeField] private int startHour = 10;
    [SerializeField] private int startMinute = 10;
    [SerializeField] private int startSecond = 30;


    [Header("Debug")]
    [ReadOnly] public bool useSystemTime = false;
    [ReadOnly] public string currentTimeString;
    [ReadOnly] public float hourAngle;
    [ReadOnly] public float minuteAngle;
    [ReadOnly] public float secondAngle;

    private DateTime currentTime;
    private float elapsedTime = 0f;

    void Start()
    {
        InitializeClock();
    }

    void InitializeClock()
    {
        if (useSystemTime)
        {
            currentTime = DateTime.Now;
        }
        else
        {
            currentTime = new DateTime(2000, 1, 1, startHour, startMinute, startSecond);
        }
        elapsedTime = 0f;
        UpdateClockHands();
    }

    void Update()
    {
        if (useSystemTime)
        {
            currentTime = DateTime.Now;
        }
        else
        {
            elapsedTime += Time.deltaTime;
            currentTime = currentTime.AddSeconds(Time.deltaTime);
        }
        UpdateClockHands();
    }

    void UpdateClockHands()
    {
        float hour = currentTime.Hour + currentTime.Minute / 60f;
        float minute = currentTime.Minute + currentTime.Second / 60f;
        float second = currentTime.Second + currentTime.Millisecond / 1000f;

        hourAngle = hour * 30f;
        minuteAngle = minute * 6f;
        secondAngle = second * 6f;

        hourPivotHand.localRotation = Quaternion.Euler(0, 0, -hourAngle);
        minutePivotHand.localRotation = Quaternion.Euler(0, 0, -minuteAngle);
        secondPivotHand.localRotation = Quaternion.Euler(0, 0, -secondAngle);

        currentTimeString = currentTime.ToString("HH:mm:ss.fff");
    }


    public void SetCustomRandomTime()
    {
        useSystemTime = false;
        startHour = UnityEngine.Random.Range(0, 24);
        startMinute = UnityEngine.Random.Range(0, 60);
        startSecond = UnityEngine.Random.Range(0, 60);
        InitializeClock();
    }

    public void ToggleTimeMode()
    {
        useSystemTime = !useSystemTime;
        InitializeClock();
    }
}