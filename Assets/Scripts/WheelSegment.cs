using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WheelSegment : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image backgroundImage;
    [Space]
    [SerializeField] private Color bgColorOdd;
    [SerializeField] private Color bgColorEven;
    [SerializeField] private Color textColorOdd;
    [SerializeField] private Color textColorEven;

    [HideInInspector] public bool hasBeenDuplicated = false;
    [HideInInspector] public int spinID;
    [HideInInspector] public bool isOdd;

    public void Initialize (string labelText, bool isOdd, int spinID) {
        label.text = labelText;
        label.color = isOdd ? textColorOdd : textColorEven;
        backgroundImage.color = isOdd ? bgColorOdd : bgColorEven;

        this.isOdd = isOdd;
        this.spinID = spinID;
    }

}
