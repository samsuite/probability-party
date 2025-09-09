using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Activity", menuName = "Activity Profile", order = -1000)]
public class ActivityProfile : ScriptableObject {

    public ActivityTag tags;
    public int exactPlayerCount = 1;
    public int minPlayerCount = 2;
    public int maxPlayerCount = 5;
    public bool requireExactPlayerCount;
    public bool hasMinPlayerCount;
    public bool hasMaxPlayerCount;
    public bool requireEvenPlayerCount;
    public bool requireOddPlayerCount;


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

}
