using System;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public Toggler toggler;
    public Renderer greenLight;
    public Renderer redLight;
    public List<Car> cars;

    private void Awake()
    {
        if (toggler)
        {
            toggler.Toggle += OnSwitchToggle;
        }
    }

    private void OnSwitchToggle(bool isOn)
    {
        redLight.enabled = isOn;
        greenLight.enabled = !isOn;
        cars.ForEach(car =>
        {
            car.enabled = !isOn;
        });
    }

    private void OnTriggerEnter(Collider other)
    {
        Car car = other.GetComponent<Car>();
        if (car)
        {
            if (!cars.Contains(car))
            {
                cars.Add(car);
                car.enabled = !toggler.isOn;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Car car = other.GetComponent<Car>();
        if (car)
        {
            cars.Remove(car);
            car.enabled = true;
        }
    }
}