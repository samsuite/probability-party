using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WheelSegment : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image borderImage;
    [SerializeField] private Image fadeImage;
    [Space]
    [SerializeField] private Color bgColorOdd;
    [SerializeField] private Color fadeColorOdd;
    [SerializeField] private Color textColorOdd;
    [SerializeField] private Color bgColorEven;
    [SerializeField] private Color fadeColorEven;
    [SerializeField] private Color textColorEven;
    [Space]
    [SerializeField] private Color bgColorSelected;
    [SerializeField] private Color fadeColorSelected;
    [SerializeField] private Color textColorSelected;

    [HideInInspector] public bool hasBeenDuplicated = false;
    [HideInInspector] public int spinID;
    [HideInInspector] public int segmentID;
    [HideInInspector] public bool isOdd;

    [HideInInspector] public WheelController wheelController;

    private void Start () {
        Debug.Log("Start");
    }

    public void Initialize (WheelController wheelController, string labelText, bool isOdd, int spinID) {
        label.text = labelText;
        this.isOdd = isOdd;
        this.spinID = spinID;
        this.wheelController = wheelController;

        segmentID = Random.Range(0, int.MaxValue);
    }

    public void Update () {
        UpdateGraphics();
    }

    private void UpdateGraphics () {
        if (wheelController.selectedSegmentID == segmentID && wheelController.isSettled) {
            label.color = textColorSelected;
            borderImage.color = fadeColorSelected;
            backgroundImage.color = bgColorSelected;
        }
        else {
            label.color = isOdd ? textColorOdd : textColorEven;
            borderImage.color = isOdd ? fadeColorOdd : fadeColorEven;
            backgroundImage.color = isOdd ? bgColorOdd : bgColorEven;
        }

        fadeImage.color = isOdd ? fadeColorOdd : fadeColorEven;
    }

}
