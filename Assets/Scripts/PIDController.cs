using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PIDController
{
    [SerializeField]
    public string name;
    [Range(-50.0f, 50.0f)]
    public float proportionalGain, integralGain, derivativeGain;

    private float _p, _i, _d;
    private float _previousError = 0.0f;

    public float Update(float target, float current, float dt)
    {
        float currentError = target - current;
        _p = currentError;
        _i += _p * dt;
        if (_previousError != 0.0f)
            _d = (_p - _previousError) / dt;
        _previousError = currentError;

        var result = (_p * proportionalGain) + (_i * integralGain) + (_d * derivativeGain);
        result = Mathf.Clamp(result, -1.0f, 1.0f);
        return result;
    }

    public void Reset()
    {
        _p = 0;
        _i = 0;
        _d = 0;
        _previousError = 0.0f;
    }
    public string ToJson()
    {
        string json = "";
        json += $"\"{name}\":{{\n";
        json += $"\"ProportionalGain\":{proportionalGain},\n";
        json += $"\"IntegralGain\":{integralGain},\n";
        json += $"\"DerivativeGain\":{derivativeGain},\n";
        json += "}\n";
        return json;
    }

    public void FromJson(string json)
    {
        var p = JSON.Parse(json);
        proportionalGain = p["ProportionalGain"].AsFloat;
        integralGain = p["IntegralGain"].AsFloat;
        derivativeGain = p["DerivativeGain"].AsFloat;
    }
}
