using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour {

    [SerializeField] private WheelController rotator;
    [SerializeField] private new MeshRenderer renderer;
    [Space]
    public float blurSpeedMin;
    public float blurSpeedMax;
    public float maxBlurValue;
    [Space]
    public float spiralSpeedMin;
    public float spiralSpeedMax;
    public float maxSpiralValue;
    [Space]
    public float wobbleSpeedMin;
    public float wobbleSpeedMax;
    public float maxWobbleValue;
    public float perlinSpeed;
    [Space]
    public float hueShiftSpeedMin;
    public float hueShiftSpeedMax;
    [Space]
    public float bandFreqSpeedMin;
    public float bandFreqSpeedMax;
    public float maxBandFrequencyValue;
    [Space]
    public AnimationCurve intensityCurve;

    private Material material;
    private int angleDifferenceID;
    private int speedBlurID;
    private int hueshiftAngleID;
    private int bandFrequencyID;

    private Vector3 originalPosition;
    private float wobbleIntensity;

    private float accumulatedHueshiftAngle;

    private void Start () {
        material = new Material(renderer.material);
        renderer.material = material;

        angleDifferenceID = Shader.PropertyToID("_CenterAngleDifference");
        speedBlurID = Shader.PropertyToID("_SpeedBlur");
        hueshiftAngleID = Shader.PropertyToID("_HueshiftAngle");
        bandFrequencyID = Shader.PropertyToID("_HueShiftBandFrequency");

        originalPosition = transform.position;
    }

    private void Update () {
        float speed = rotator.currentSpeed;

        float blurVal = Mathf.InverseLerp(blurSpeedMin, blurSpeedMax, speed);
        float spiralVal = Mathf.InverseLerp(spiralSpeedMin, spiralSpeedMax, speed);
        float wobbleVal = Mathf.InverseLerp(wobbleSpeedMin, wobbleSpeedMax, speed);
        float bandFreqVal = Mathf.InverseLerp(bandFreqSpeedMin, bandFreqSpeedMax, speed);
        float hueshiftVal = Mathf.InverseLerp(hueShiftSpeedMin, hueShiftSpeedMax, speed);

        blurVal = intensityCurve.Evaluate(blurVal);
        spiralVal = intensityCurve.Evaluate(spiralVal);
        wobbleVal = intensityCurve.Evaluate(spiralVal);
        bandFreqVal = intensityCurve.Evaluate(bandFreqVal);
        hueshiftVal = intensityCurve.Evaluate(hueshiftVal);

        SetSpeedBlur(blurVal * maxBlurValue);
        SetSpiralOffset(spiralVal * maxSpiralValue);
        SetWobbleIntensity(wobbleVal * maxWobbleValue);
        SetBandFrequency(bandFreqVal * maxBandFrequencyValue);

        accumulatedHueshiftAngle += hueshiftVal * (rotator.distanceThisFrame / rotator.fullRotationDistance) * 360f;
        SetHueShiftAngle(accumulatedHueshiftAngle);

        ApplyWobble();
    }


    private void SetSpeedBlur (float value) {
        material.SetFloat(speedBlurID, value);
    }

    private void SetSpiralOffset (float value) {
        material.SetFloat(angleDifferenceID, value);
    }

    private void SetHueShiftAngle (float value) {
        material.SetFloat(hueshiftAngleID, value);
    }

    private void SetBandFrequency (float value) {
        material.SetFloat(bandFrequencyID, value);
    }

    private void SetWobbleIntensity (float value) {
        wobbleIntensity = value;
    }

    private void ApplyWobble () {
        float perlinX = Mathf.PerlinNoise1D(Time.time * perlinSpeed + 12312.45f);
        float perlinY = Mathf.PerlinNoise1D(Time.time * perlinSpeed + 93854.91f);
        Vector3 perlinVector = new Vector3(perlinX, perlinY);

        transform.position = originalPosition + (perlinVector * wobbleIntensity);
    }

}
