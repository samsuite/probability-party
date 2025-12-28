using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    [SerializeField] private RectTransform tagsLayout;
    [SerializeField] private ActivityTagElement activityTagPrefab;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;
    [SerializeField] private TextMeshProUGUI resultsPlayerCountText;
    [Space]
    [SerializeField] private RectTransform readyPanel;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button changeCountButton;
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI readyButtonText;

    public GameState gameState {get; private set;}
    private Vector3 originalCameraPosition;
    private Vector3 originalTopPanelPosition;
    private Vector3 originalBottomPanelPosition;

    private string[] numbers = new string[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten"};

    [Space]
    public float delayBeforePan = 1f;
    public float cameraPanDuration = 1f;
    public float cameraPanDistance = 1f;
    public float resultsPanDistanceV = 1000f;
    public float resultsPanDistanceH = 1000f;

    private int playerCount = 1;
    private const int minPlayerCount = 1;
    private const int maxPlayerCount = 10;

    private void Start () {
        Initialize();
    }

    private void Update () {
        RefreshPlayerCountText();
    }

    private void Initialize () {
        originalCameraPosition = camera.transform.position;
        originalTopPanelPosition = resultsPanelTop.localPosition;
        originalBottomPanelPosition = resultsPanelBottom.localPosition;

        readyButton.onClick.AddListener(ReadyButtonPressed);
        changeCountButton.onClick.AddListener(ChangeCountButtonPressed);
        plusButton.onClick.AddListener(PlusButtonPressed);
        minusButton.onClick.AddListener(MinusButtonPressed);

        acceptButton.onClick.AddListener(AcceptButtonPressed);
        declineButton.onClick.AddListener(DeclineButtonPressed);

        playerCount = wheelController.GetRandomWeightedPlayerCount();
        wheelController.CreateSegments(playerCount, false);
        StartCoroutine(ReadyToSpinCoroutine());

        SetResultsPosition(0);
    }

    private IEnumerator ReadyToSpinCoroutine () {
        gameState = GameState.ReadyToSpin;
        Debug.Log("Ready to spin");

        ShowReadyPanel();
        yield return null;
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

    bool acceptedResult = false;
    bool declinedResult = false;
    private IEnumerator DisplayingResultsCoroutine () {
        gameState = GameState.DisplayingResults;

        resultsTitle.text = wheelController.selectedSegmentName;
        ClearTags();
        CreateTags(wheelController.selectedActivity);

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

        acceptedResult = false;
        declinedResult = false;
        while (!acceptedResult && !declinedResult) {
            // wait for accept or decline
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

    private void RefreshPlayerCountText () {
        string numString = numbers[playerCount];

        if (playerCount == 1) {
            playerCountText.text = "one person";
            readyButtonText.text = "I'm ready!";
        }
        else {
            playerCountText.text = numString.ToLower() + " people";
            readyButtonText.text = numString + " people are ready!";
        }

        if (gameState == GameState.DisplayingResults) {
            resultsPlayerCountText.text = "an activity for "+GetPlayerCountSummary(wheelController.selectedActivity);
        }

        plusButton.interactable = playerCount < maxPlayerCount;
        minusButton.interactable = playerCount > minPlayerCount;
    }


    public static string GetPlayerCountSummary (ActivityProfile activity) {
        string playerCountSummary = string.Empty;
        if (activity.requireExactPlayerCount) {
            if (activity.exactPlayerCount == 1) {
                playerCountSummary = "1 person";
            }
            else {
                playerCountSummary = $"{activity.exactPlayerCount} people";
            }
        }
        else if (activity.hasMinPlayerCount && !activity.hasMaxPlayerCount) {
            playerCountSummary = $"{activity.minPlayerCount} or more people";
        }
        else if (activity.hasMaxPlayerCount && !activity.hasMinPlayerCount) {
            playerCountSummary = $"1 to {activity.maxPlayerCount} people";
        }
        else if (activity.hasMaxPlayerCount && activity.hasMinPlayerCount) {
            playerCountSummary = $"{activity.minPlayerCount} to {activity.maxPlayerCount} people";
        }

        if (playerCountSummary == string.Empty) {
            if (activity.requireEvenPlayerCount) {
                playerCountSummary += "any even number of people";
            }
            if (activity.requireOddPlayerCount) {
                playerCountSummary += "any odd number of people";
            }
        }
        else {
            if (activity.requireEvenPlayerCount || activity.requireOddPlayerCount) {

                List<int> validPlayerCounts = new List<int>();
                for (int i = 1; i <= 10; i++) {
                    if (activity.CanPlayWithNumPlayers(i)) {
                        validPlayerCounts.Add(i);
                    }
                }

                playerCountSummary = string.Empty;
                for (int i = 0; i < validPlayerCounts.Count; i++) {

                    if (i == validPlayerCounts.Count - 1) {
                        // last entry
                        playerCountSummary += $"or {validPlayerCounts[i]}";
                    }
                    else if (i == validPlayerCounts.Count - 2) {
                        // second to last entry
                        playerCountSummary += $"{validPlayerCounts[i]} ";
                    }
                    else {
                        // some other entry
                        playerCountSummary += $"{validPlayerCounts[i]}, ";
                    }
                }


                playerCountSummary += " people";
            }
        }

        if (playerCountSummary == string.Empty) {
            playerCountSummary += "any number of people";
        }

        return playerCountSummary;
    }


    private void ShowReadyPanel () {
        readyPanel.gameObject.SetActive(true);
        plusButton.gameObject.SetActive(false);
        minusButton.gameObject.SetActive(false);
        changeCountButton.gameObject.SetActive(true);
    }

    private void HideReadyPanel () {
        readyPanel.gameObject.SetActive(false);
    }

    private void ShowPlayerCountOptions () {
        plusButton.gameObject.SetActive(true);
        minusButton.gameObject.SetActive(true);
        changeCountButton.gameObject.SetActive(false);
    }


    private void CreateTags (ActivityProfile activity) {
        foreach (ActivityTag tag in Enum.GetValues(typeof(ActivityTag))) {
            bool hasTag = ((int)activity.tags & (int)tag) == (int)tag;
            if (hasTag) {
                ActivityTagElement newTagElement = Instantiate(activityTagPrefab, tagsLayout);
                newTagElement.label.text = GetTagName(tag);
                LayoutRebuilder.ForceRebuildLayoutImmediate(newTagElement.transform as RectTransform);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tagsLayout);
    }

    public static string GetTagName (ActivityTag tag) {
        if (tag == ActivityTag.What) {
            return "???";
        }
        else {
            string[] words = Regex.Matches(tag.ToString(), "(^[a-z]+|[A-Z]+(?![a-z])|[A-Z][a-z]+)")
                .OfType<Match>()
                .Select(m => m.Value)
                .ToArray();
            return string.Join(" ", words);
        }
    }

    private void ClearTags () {
        for (int i = tagsLayout.childCount - 1; i >= 0; i--) {
            Destroy(tagsLayout.GetChild(i).gameObject);
        }
    }


    private void ReadyButtonPressed () {
        wheelController.StartSpin(playerCount);

        HideReadyPanel();
        StartCoroutine(SpinningCoroutine());
    }

    private void ChangeCountButtonPressed () {
        ShowPlayerCountOptions();
    }

    private void PlusButtonPressed () {
        playerCount += 1;
    }

    private void MinusButtonPressed () {
        playerCount -= 1;
    }

    private void AcceptButtonPressed () {
        acceptedResult = true;
        PrintManager.PrintActivityReceipt(wheelController.selectedActivity);
        wheelController.ConsumeActivity(wheelController.selectedActivity);
        wheelController.ClearDeclinedActivities();
        playerCount = wheelController.GetRandomWeightedPlayerCount();
    }

    private void DeclineButtonPressed () {
        declinedResult = true;
        wheelController.DeclineActivity(wheelController.selectedActivity);
    }


}
