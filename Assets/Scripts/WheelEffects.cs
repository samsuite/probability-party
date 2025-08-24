using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelEffects : MonoBehaviour {

    [SerializeField] private SegmentRotator rotator;
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
    public AnimationCurve blurIntensityCurve;
    public AnimationCurve spiralIntensityCurve;

    private Material material;
    private int angleDifferenceID;
    private int speedBlurID;

    private void Start () {
        material = new Material(renderer.material);
        renderer.material = material;

        angleDifferenceID = Shader.PropertyToID("_CenterAngleDifference");
        speedBlurID = Shader.PropertyToID("_SpeedBlur");
    }

    private void Update () {
        float speed = rotator.currentSpeed;

        float blurVal = Mathf.InverseLerp(blurSpeedMin, blurSpeedMax, speed);
        float spiralVal = Mathf.InverseLerp(spiralSpeedMin, spiralSpeedMax, speed);

        blurVal = blurIntensityCurve.Evaluate(blurVal);
        spiralVal = spiralIntensityCurve.Evaluate(spiralVal);

        SetSpeedBlur(blurVal * maxBlurValue);
        SetSpiralOffset(spiralVal * maxSpiralValue);
    }


    private void SetSpeedBlur (float value) {
        material.SetFloat(speedBlurID, value);
    }

    private void SetSpiralOffset (float value) {
        material.SetFloat(angleDifferenceID, value);
    }

}
