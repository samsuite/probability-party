using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Activity", menuName = "Activity Profile", order = -1000)]
public class ActivityProfile : ScriptableObject {

    [TextArea(10,20)]
    public string description;
    public string qrLink;
    public bool hasQR { get { return !string.IsNullOrWhiteSpace(qrLink); } }

    public ActivityTag tags;
    public int exactPlayerCount = 1;
    public int minPlayerCount = 2;
    public int maxPlayerCount = 5;
    public bool requireExactPlayerCount;
    public bool hasMinPlayerCount;
    public bool hasMaxPlayerCount;
    public bool requireEvenPlayerCount;
    public bool requireOddPlayerCount;
    public int weight = 1;

    public bool unavailableBeforeTime;
    public int hours = 0;
    public int minutes = 0;


    public bool CanPlayWithNumPlayers (int numPlayers) {
        if (requireExactPlayerCount && (exactPlayerCount != numPlayers)) {
            return false;
        }

        if (hasMinPlayerCount && numPlayers < minPlayerCount) {
            return false;
        }

        if (hasMaxPlayerCount && numPlayers > maxPlayerCount) {
            return false;
        }

        if (requireEvenPlayerCount && numPlayers%2 != 0) {
            return false;
        }

        if (requireOddPlayerCount && numPlayers%2 != 1) {
            return false;
        }

        return true;
    }

    public bool CanPlayYet () {
        if (!unavailableBeforeTime) {
            return true;
        }

        DateTime readyTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, hours, minutes, 0);
        return DateTime.Now.CompareTo(readyTime) > 0;
    }

}
