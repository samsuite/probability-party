using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(0)]
public class WheelController : MonoBehaviour {

    private const int numTotalSegments = 16;
    private float segmentHeight;
    private float segmentHeightWorldSpace;
    private float canvasTopY;
    private float canvasBottomY;
    private float selectedSegmentY;
    public float fullRotationDistance {
        get {
            return canvasTopY - canvasBottomY;
        }
    }

    private float currentTotalDistance;
    private float currentGoalDistance;
    private int currentSpinID;
    private int numCompletedRotations;

    public float totalRotationSoFar {
        get {
            return ((currentTotalDistance / fullRotationDistance)+numCompletedRotations)*360f;
        }
    }

    private const float rampUpDuration = 0.2f;
    private const float windDownSpinCount = 5;
    private const float finalApproachSpinCount = 1/32f;
    private const float finalApproachMaxDuration = 5f;
    private const int numTotalSpinsMin = 6;
    private const int numTotalSpinsMax = 10;

    private const float topSpeed = 15f;
    private float springStrength {
        get {
            return 3 * finalApproachSpeed;
        }
    }

    private const float finalApproachSpeedMin = 0.15f;
    private const float finalApproachSpeedMax = 0.75f;
    private const float springDragMin = 2f;
    private const float springDragMax = 4f;

    private float windDownRange {
        get {
            return fullRotationDistance * windDownSpinCount;
        }
    }
    private float finalApproachCurveRange {
        get {
            return fullRotationDistance * finalApproachSpinCount;
        }
    }

    public float distanceFromTarget {
        get {
            return Mathf.Abs(currentGoalDistance - currentTotalDistance);
        }
    }

    private bool hasSettled;
    public bool isSettled {
        get {
            if (hasSettled) {
                return true;
            }

            const float settledSpeedThreshold = 0.15f;
            const float settledDistanceThreshold = 0.025f;
            const float settledDurationThreshold = 2.5f;
            bool settled = inFinalApproach &&
                           gameLogic.gameState == GameLogic.GameState.Spinning &&
                           currentSpeed < settledSpeedThreshold &&
                           Time.time > lastTimeSpinStarted + rampUpDuration &&
                           ((distanceFromTarget < settledDistanceThreshold)||(Time.time > timeStartedFinalApproach + settledDurationThreshold));

            if (settled) {
                hasSettled = true;
            }
            return settled;
        }
    }

    public float currentSpeed { get; private set; }
    public float distanceThisFrame { get; private set; }
    private bool inFinalApproach = true;
    private float springDrag;
    private float finalApproachSpeed;
    private float lastTimeSpinStarted;
    private float timeStartedFinalApproach;
    public int selectedSegmentID = -1;
    public string selectedSegmentName { get; private set; } = string.Empty;

    [SerializeField] private WheelSegment segmentPrefab;
    [SerializeField] private AnimationCurve speedCurve;
    [SerializeField] private GameLogic gameLogic;

    private ActivityProfile[] allActivities;
    private HashSet<ActivityProfile> alreadyPlayedActivities = new HashSet<ActivityProfile>();
    public ActivityProfile selectedActivity { get; private set; }

    private void Start () {
        allActivities = Resources.LoadAll<ActivityProfile>("Activities");

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
    }

    private void Update () {
        MoveSegments();
        LoopSegments();
    }

    private void FixedUpdate () {
        CalculateCurrentSpeed();
    }

    public void StartSpin (int numPlayers) {
        if (inFinalApproach) {
            lastTimeSpinStarted = Time.time;
        }

        while (currentTotalDistance > fullRotationDistance) {
            currentTotalDistance -= fullRotationDistance;
            numCompletedRotations += 1;
        }

        currentSpinID = Random.Range(100000000, 999999999);
        inFinalApproach = false;
        hasSettled = false;
        springDrag = Random.Range(springDragMin, springDragMax);
        finalApproachSpeed = Random.Range(finalApproachSpeedMin, finalApproachSpeedMax);

        CreateSegments(numPlayers, true);
    }



    public void CreateSegments (int numPlayers, bool setGoal) {
        List<ActivityProfile> activityPool = GetRemainingActivitiesForPlayerCount(numPlayers);
        selectedActivity = SelectActivityFromPool(activityPool);
        HashSet<ActivityProfile> activitiesOnWheel = new HashSet<ActivityProfile>();

        activityPool.Remove(selectedActivity);
        activitiesOnWheel.Add(selectedActivity);

        WheelSegment[] childSegments = GetComponentsInChildren<WheelSegment>();
        bool oddToggle = false;
        if (childSegments.Length > 0) {
            // continue the alternating pattern from the segment currently at the top
            oddToggle = !childSegments[^1].isOdd;
        }

        WheelSegment selectedSegment = null;

        for (int i = 0; i < numTotalSegments; i++) {
            WheelSegment newSegment = Instantiate(segmentPrefab, transform);
            newSegment.gameObject.name = segmentPrefab.gameObject.name;
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            string label;
            if (i == numTotalSegments / 2 && setGoal) {

                label = selectedActivity.name.ToUpper();
                // we want to end on this segment. our goal distance should reflect that
                currentGoalDistance = currentTotalDistance + newSegment.transform.position.y - selectedSegmentY;
                // add a random number of extra rotations
                currentGoalDistance += fullRotationDistance * Random.Range(numTotalSpinsMin, numTotalSpinsMax);

                // set this as the selected segment
                selectedSegment = newSegment;
            }
            else {
                if (activityPool.Count == 0) {
                    // no activities left to populate the pool. just add a bunch of random ones from the big list
                    activityPool.AddRange(allActivities);
                }

                ActivityProfile activity = activityPool[Random.Range(0, activityPool.Count)];
                while (activitiesOnWheel.Contains(activity)) {
                    // iterate until we get an activity that isn't already on the wheel
                    activity = activityPool[Random.Range(0, activityPool.Count)];
                }

                label = activity.name.ToUpper();

                activityPool.Remove(activity);
                activitiesOnWheel.Add(activity);
            }

            newSegment.Initialize(this, label, oddToggle, currentSpinID);
            oddToggle = !oddToggle;
        }

        if (selectedSegment != null) {
            selectedSegmentID = selectedSegment.segmentID;
            selectedSegmentName = selectedActivity.name.ToUpper();
            Debug.Log($"selected {selectedActivity.name}");
        }
    }

    private void MoveSegments () {
        distanceThisFrame = currentSpeed * Time.deltaTime;

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
        float windDownFraction = 1-Mathf.Clamp01((remainingDistance-finalApproachCurveRange) / windDownRange);

        if (remainingDistance < finalApproachCurveRange || inFinalApproach) {
            if (!inFinalApproach) {
                inFinalApproach = true;
                timeStartedFinalApproach = Time.time;
            }

            currentSpeed += (remainingDistance/finalApproachCurveRange) * springStrength * Time.deltaTime;
            currentSpeed *= ( 1 - Time.deltaTime * springDrag);
        }
        else {
            float curveSample = speedCurve.Evaluate(windDownFraction);
            currentSpeed = Mathf.Lerp(finalApproachSpeed, topSpeed, curveSample);
        }

        float rampUpCoefficient = Mathf.Clamp01((Time.time - lastTimeSpinStarted)/rampUpDuration);
        currentSpeed *= rampUpCoefficient;

        if (inFinalApproach) {
            if (Time.time - timeStartedFinalApproach > finalApproachMaxDuration) {
                // stop the wheel completely a few seconds after the final approach starts.
                // this eliminates any tiny drift
                currentSpeed = 0;
            }
        }
    }

    private List<ActivityProfile> GetRemainingActivitiesForPlayerCount (int playerCount) {
        List<ActivityProfile> validActivities = new List<ActivityProfile>();
        foreach (ActivityProfile activity in allActivities) {
            if (activity.CanPlayWithNumPlayers(playerCount) && !alreadyPlayedActivities.Contains(activity)) {
                validActivities.Add(activity);
            }
        }
        return validActivities;
    }

    private ActivityProfile SelectActivityFromPool (List<ActivityProfile> activityPool) {
        ActivityProfile activity = activityPool[Random.Range(0, activityPool.Count)];
        return activity;
    }

}
