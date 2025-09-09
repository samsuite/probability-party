using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Flags]
public enum ActivityTag {
    DrinkingGame              = 1 << 0,
    UsesTV                    = 1 << 1,
    CardGame                  = 1 << 2,
    BoardGame                 = 1 << 3,
    JackboxGame               = 1 << 4,
    PhysicalChallenge         = 1 << 5,
    Creative                  = 1 << 6,
    Annoying                  = 1 << 7,
    What                      = 1 << 8,
    Outside                   = 1 << 9,
    Competitive               = 1 << 10,
    Collaborative             = 1 << 11
}

public class ActivityTagElement : MonoBehaviour {

    public TextMeshProUGUI label;

}
