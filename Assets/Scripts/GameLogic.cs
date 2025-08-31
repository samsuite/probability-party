using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DefaultExecutionOrder(1)]
public class GameLogic : MonoBehaviour {

    public enum GameState {
        ReadyToSpin,
        Spinning,
        DisplayingResults
    }

    [SerializeField] private WheelController wheelController;
    [SerializeField] private new Camera camera;
    [SerializeField] private TextMeshProUGUI resultsTitle;
    [SerializeField] private RectTransform resultsPanelTop;
    [SerializeField] private RectTransform resultsPanelBottom;
    [SerializeField] private AnimationCurve cameraPanCurve;
    [SerializeField] private AnimationCurve resultsMotionCurve;

    public GameState gameState {get; private set;}
    private Vector3 originalCameraPosition;
    private Vector3 originalTopPanelPosition;
    private Vector3 originalBottomPanelPosition;

    [Space]
    public float delayBeforePan = 1f;
    public float cameraPanDuration = 1f;
    public float cameraPanDistance = 1f;
    public float resultsPanDistanceV = 1000f;
    public float resultsPanDistanceH = 1000f;

    private void Start () {
        Initialize();
    }

    private void Initialize () {
        originalCameraPosition = camera.transform.position;
        originalTopPanelPosition = resultsPanelTop.localPosition;
        originalBottomPanelPosition = resultsPanelBottom.localPosition;

        wheelController.CreateSegments(3);
        StartCoroutine(ReadyToSpinCoroutine());

        SetResultsPosition(0);
    }

    private IEnumerator ReadyToSpinCoroutine () {
        gameState = GameState.ReadyToSpin;
        Debug.Log("Ready to spin");

        while (!Input.GetKeyDown(KeyCode.Space)) {
            // wait for spacebar
            yield return null;
        }

        wheelController.StartSpin();
        yield return SpinningCoroutine();
    }

    private IEnumerator SpinningCoroutine () {
        gameState = GameState.Spinning;
        Debug.Log("Spinning");

        while (!wheelController.isSettled) {
            // wait for wheel to stop spinning
            yield return null;
        }

        yield return DisplayingResultsCoroutine();
    }

    private IEnumerator DisplayingResultsCoroutine () {
        gameState = GameState.DisplayingResults;

        resultsTitle.text = wheelController.selectedSegmentName;
        yield return new WaitForSeconds(delayBeforePan);

        float t = 0;
        while (t < 1) {
            t += Time.deltaTime / cameraPanDuration;
            t = Mathf.Clamp01(t);
            float panProgress = cameraPanCurve.Evaluate(t);
            camera.transform.position = originalCameraPosition + Vector3.right * cameraPanDistance * panProgress;
            SetResultsPosition(t);
            yield return null;
        }

        while (!Input.GetKeyDown(KeyCode.Space)) {
            // wait for spacebar
            yield return null;
        }

        // reset the selected segment so the wedge isn't hilighted
        wheelController.selectedSegmentID = -1;

        while (t > 0) {
            t -= Time.deltaTime / cameraPanDuration;
            t = Mathf.Clamp01(t);
            float panProgress = 1-cameraPanCurve.Evaluate(1-t);
            camera.transform.position = originalCameraPosition + Vector3.right * cameraPanDistance * panProgress;
            SetResultsPosition(t);
            yield return null;
        }

        yield return ReadyToSpinCoroutine();
    }

    private void SetResultsPosition (float t) {
        float motionProgress = resultsMotionCurve.Evaluate(1-t);
        resultsPanelTop.localPosition = originalTopPanelPosition + Vector3.up * resultsPanDistanceV * motionProgress + Vector3.right * resultsPanDistanceH * motionProgress;
        resultsPanelBottom.localPosition = originalBottomPanelPosition + Vector3.down * resultsPanDistanceV * motionProgress + Vector3.right * resultsPanDistanceH * motionProgress;
    }

}
