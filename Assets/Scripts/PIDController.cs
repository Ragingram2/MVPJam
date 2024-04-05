using NaughtyAttributes;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PIDController
{
    [SerializeField]
    public string name;
    public float proportionalGain, integralGain, derivativeGain;
    public Vector2 range = new Vector2(-10.0f, 10.0f);

    private float p, i, d;
    private Vector3 pVec, iVec, dVec;
    private Vector3 previousErrorVec = Vector3.zero;
    private float previousError = 0.0f;

    public Vector3 Update(Vector3 target, Vector3 current, float dt)
    {
        Vector3 currentError = target - current;
        pVec = currentError;
        iVec = Vector3.Min(Vector3.Max((iVec + pVec) * dt, Vector3.one * range.x), Vector3.one * range.y);

        if (previousErrorVec == Vector3.zero)
            dVec = (pVec - previousErrorVec) / dt;
        previousErrorVec = currentError;

        var result = (pVec * proportionalGain) + (iVec * integralGain) + (dVec * derivativeGain);
        result = Vector3.Min(Vector3.Max(result, Vector3.one * range.x), Vector3.one * range.y);
        return result;
    }
    public float Update(float target, float current, float dt)
    {
        float currentError = target - current;
        p = currentError;
        i = Mathf.Clamp(i + p * dt, range.x, range.y);

        if (previousError != 0.0f)
            d = (p - previousError) / dt;
        previousError = currentError;

        var result = (p * proportionalGain) + (i * integralGain) + (d * derivativeGain);
        result = Mathf.Clamp(result, range.x, range.y);
        return result;
    }

    public void SetRange(float min, float max)
    {
        range = new Vector2(min, max);
    }

    private void SetRange(Vector2 range)
    {
        this.range = range;
    }

    public void Reset()
    {
        p = 0;
        i = 0;
        d = 0;
        pVec = Vector3.zero;
        iVec = Vector3.zero;
        dVec = Vector3.zero;
        previousError = 0.0f;
        range = new Vector2(0, 0);
    }
}
