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
    public float maxBandFrequencyValue;
    [Space]
    public AnimationCurve blurIntensityCurve;
    public AnimationCurve spiralIntensityCurve;
    public AnimationCurve wobbleIntensityCurve;
    public AnimationCurve hueShiftCurve;

    private Material material;
    private int angleDifferenceID;
    private int speedBlurID;
    private int currentAngleID;
    private int bandFrequencyID;

    private Vector3 originalPosition;
    private float wobbleIntensity;

    private void Start () {
        material = new Material(renderer.material);
        renderer.material = material;

        angleDifferenceID = Shader.PropertyToID("_CenterAngleDifference");
        speedBlurID = Shader.PropertyToID("_SpeedBlur");
        currentAngleID = Shader.PropertyToID("_CurrentAngle");
        bandFrequencyID = Shader.PropertyToID("_HueShiftBandFrequency");

        originalPosition = transform.position;
    }

    private void Update () {
        float speed = rotator.currentSpeed;

        float blurVal = Mathf.InverseLerp(blurSpeedMin, blurSpeedMax, speed);
        float spiralVal = Mathf.InverseLerp(spiralSpeedMin, spiralSpeedMax, speed);
        float wobbleVal = Mathf.InverseLerp(wobbleSpeedMin, wobbleSpeedMax, speed);
        float bandFrequencyVal = Mathf.InverseLerp(hueShiftSpeedMin, hueShiftSpeedMax, speed);

        blurVal = blurIntensityCurve.Evaluate(blurVal);
        spiralVal = spiralIntensityCurve.Evaluate(spiralVal);
        wobbleVal = wobbleIntensityCurve.Evaluate(spiralVal);
        bandFrequencyVal = hueShiftCurve.Evaluate(bandFrequencyVal);

        SetSpeedBlur(blurVal * maxBlurValue);
        SetSpiralOffset(spiralVal * maxSpiralValue);
        SetWobbleIntensity(wobbleVal * maxWobbleValue);
        SetBandFrequency(bandFrequencyVal * maxBandFrequencyValue);
        SetCurrentAngle(rotator.totalRotationSoFar);

        ApplyWobble();
    }


    private void SetSpeedBlur (float value) {
        material.SetFloat(speedBlurID, value);
    }

    private void SetSpiralOffset (float value) {
        material.SetFloat(angleDifferenceID, value);
    }

    private void SetCurrentAngle (float value) {
        material.SetFloat(currentAngleID, value);
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
