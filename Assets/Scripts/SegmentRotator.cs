using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SegmentRotator : MonoBehaviour {

    private const int numTotalSegments = 16;
    private float segmentHeight;
    private float segmentHeightWorldSpace;
    private float canvasTopY;
    private float canvasBottomY;
    private float selectedSegmentY;
    private float fullRotationDistance {
        get {
            return canvasTopY - canvasBottomY;
        }
    }

    private float currentTotalDistance;
    private float currentGoalDistance;
    private int currentSpinID;

    private const float windDownSpinCount = 5;
    private const float finalApproachSpinCount = 1/32f;
    private const int numTotalSpinsMin = 6;
    private const int numTotalSpinsMax = 10;

    private const float topSpeed = 15f;
    private const float springStrength = 3f;
    private const float finalApproachSpeed = 0.75f;
    private const float springDragMin = 1f;
    private const float springDragMax = 5f;

    private float speedCurveRange {
        get {
            return fullRotationDistance * windDownSpinCount;
        }
    }
    private float finalApproachCurveRange {
        get {
            return fullRotationDistance * finalApproachSpinCount;
        }
    }
    private float currentSpeed;
    private bool inFinalApproach;
    private float springDrag;

    [SerializeField] private WheelSegment segmentPrefab;
    [SerializeField] private AnimationCurve speedCurve;


    private void Start () {
        RectTransform segmentPrefabRect = segmentPrefab.GetComponent<RectTransform>();
        segmentHeight = segmentPrefabRect.rect.height;

        Canvas segmentCanvas = GetComponentInParent<Canvas>();
        Vector3[] canvasCorners = new Vector3[4];
        segmentCanvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);
        canvasTopY = canvasCorners[1].y;
        canvasBottomY = canvasCorners[0].y;
        segmentHeightWorldSpace = transform.TransformVector(Vector3.up*segmentHeight).y;

        // ReSharper disable once PossibleLossOfFraction
        selectedSegmentY = canvasTopY - ((numTotalSegments / 2)-1) * segmentHeightWorldSpace;

        StartCoroutine(TriggerLateStart());
    }

    private void LateStart () {
        StartSpin();
    }

    private void Update () {

        MoveSegments();
        LoopSegments();

        if (Input.GetKeyDown(KeyCode.Space)) {
            StartSpin();
        }
    }

    private void FixedUpdate () {
        CalculateCurrentSpeed();
    }

    private void StartSpin () {
        currentSpinID = Random.Range(100000000, 999999999);
        inFinalApproach = false;
        springDrag = Random.Range(springDragMin, springDragMax);

        CreateSegments("YOU WIN");
    }



    private void CreateSegments (string selectedSegmentLabel) {
        WheelSegment[] childSegments = GetComponentsInChildren<WheelSegment>();
        bool oddToggle = false;
        if (childSegments.Length > 0) {
            // continue the alternating pattern from the segment currently at the top
            oddToggle = !childSegments[^1].isOdd;
        }

        for (int i = 0; i < numTotalSegments; i++) {
            WheelSegment newSegment = Instantiate(segmentPrefab, transform);
            newSegment.gameObject.name = segmentPrefab.gameObject.name;
            string label = "NOPE";

            if (i == numTotalSegments/2) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

                label = selectedSegmentLabel;
                // we want to end on this segment. our goal distance should reflect that
                currentGoalDistance = currentTotalDistance + newSegment.transform.position.y - selectedSegmentY;
                // add a random number of extra rotations
                currentGoalDistance += fullRotationDistance * Random.Range(numTotalSpinsMin, numTotalSpinsMax);
            }

            newSegment.Initialize(label, oddToggle, currentSpinID);
            oddToggle = !oddToggle;
        }
    }

    private void MoveSegments () {
        float remainingDistance = currentGoalDistance - currentTotalDistance;
        float distanceThisFrame = currentSpeed * Time.deltaTime;

        Debug.Log("currentSpeed "+currentSpeed);
        Debug.Log("remainingDistance "+remainingDistance);

        currentTotalDistance += distanceThisFrame;
        transform.position += Vector3.down * distanceThisFrame;
    }

    private void LoopSegments () {
        WheelSegment[] childSegments = GetComponentsInChildren<WheelSegment>();
        if (childSegments.Length == 0) {
            return;
        }

        while (childSegments[^1].transform.position.y < canvasTopY) {
            LoopSegmentsOnce();
            childSegments = GetComponentsInChildren<WheelSegment>();
        }
    }

    private void LoopSegmentsOnce () {
        WheelSegment[] childSegments = GetComponentsInChildren<WheelSegment>();

        foreach (WheelSegment segment in childSegments) {
            if (segment.transform.position.y < canvasBottomY+segmentHeightWorldSpace) {
                // this segment is about to start going off the bottom of the screen
                // create a copy of it at the top

                // unless the segment is from the previous spin,
                // in which case we won't respawn it,
                // and it'll get destroyed when it goes off screen

                if (!segment.hasBeenDuplicated && segment.spinID == currentSpinID) {
                    WheelSegment duplicateSegment = Instantiate(segment, transform);
                    duplicateSegment.gameObject.name = segment.gameObject.name;
                    segment.hasBeenDuplicated = true;
                }
            }

            if (segment.transform.position.y < canvasBottomY) {
                // segment is below the bottom of the canvas
                // destroy it
                Destroy(segment.gameObject);
                // compensate for the resulting offset by moving the whole group up
                transform.localPosition += Vector3.up * segmentHeight;
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    private void CalculateCurrentSpeed () {
        float remainingDistance = currentGoalDistance - currentTotalDistance;
        float speedFraction = 1-Mathf.Clamp01((remainingDistance-finalApproachCurveRange) / speedCurveRange);

        if (remainingDistance < finalApproachCurveRange || inFinalApproach) {
            currentSpeed += (remainingDistance/finalApproachCurveRange) * springStrength * Time.deltaTime;
            currentSpeed *= ( 1 - Time.deltaTime * springDrag);
            inFinalApproach = true;
        }
        else {
            float curveSample = speedCurve.Evaluate(speedFraction);
            currentSpeed = Mathf.Lerp(finalApproachSpeed, topSpeed, curveSample);
        }
    }

    private IEnumerator TriggerLateStart () {
        yield return null;
        LateStart();
    }

}
