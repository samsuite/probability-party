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

    [HideInInspector] public bool hasBeenDuplicated = false;
    [HideInInspector] public int spinID;
    [HideInInspector] public bool isOdd;

    public void Initialize (string labelText, bool isOdd, int spinID) {
        label.text = labelText;
        label.color = isOdd ? textColorOdd : textColorEven;
        fadeImage.color = isOdd ? fadeColorOdd : fadeColorEven;
        borderImage.color = isOdd ? fadeColorOdd : fadeColorEven;
        backgroundImage.color = isOdd ? bgColorOdd : bgColorEven;

        this.isOdd = isOdd;
        this.spinID = spinID;
    }

}
