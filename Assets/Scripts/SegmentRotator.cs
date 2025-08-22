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

    [SerializeField] private WheelSegment segmentPrefab;

    public float speed = 1;
    public int currentSpinID;

    private void Start () {
        RectTransform segmentPrefabRect = segmentPrefab.GetComponent<RectTransform>();
        segmentHeight = segmentPrefabRect.rect.height;

        Canvas segmentCanvas = GetComponentInParent<Canvas>();
        Vector3[] canvasCorners = new Vector3[4];
        segmentCanvas.GetComponent<RectTransform>().GetWorldCorners(canvasCorners);
        canvasTopY = canvasCorners[1].y;
        canvasBottomY = canvasCorners[0].y;
        segmentHeightWorldSpace = transform.TransformVector(Vector3.up*segmentHeight).y;

        StartCoroutine(TriggerLateStart());
    }

    private void LateStart () {
        Spin();
    }

    private void Update () {

        transform.position += Vector3.down * speed * Time.deltaTime;
        LoopSegments();

        if (Input.GetKeyDown(KeyCode.Space)) {
            Spin();
        }
    }

    private void Spin () {
        currentSpinID = Random.Range(100000000, 999999999);
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
                label = selectedSegmentLabel;
            }

            newSegment.Initialize(label, oddToggle, currentSpinID);
            oddToggle = !oddToggle;
        }
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

    private IEnumerator TriggerLateStart () {
        yield return null;
        LateStart();
    }

}
