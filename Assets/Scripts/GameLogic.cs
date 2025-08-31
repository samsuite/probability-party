using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1)]
public class GameLogic : MonoBehaviour {

    public enum GameState {
        ReadyToSpin,
        Spinning,
        DisplayingResults,
        Transitioning
    }

    [SerializeField] private WheelController wheelController;
    public GameState gameState {get; private set;}

    private void Start () {
        Initialize();
    }

    private void Initialize () {
        wheelController.CreateSegments(3);
        StartCoroutine(ReadyToSpinCoroutine());
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

        yield return ReadyToSpinCoroutine();
    }

}
