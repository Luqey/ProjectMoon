// Handles player and npc turns. In the player's case, wait for input to progress the turn, and npcs wait for the player's turn to start before doing their turn
// Turns happen "simultaneously" except that players technically move first before npcs


// All NPCs have a turn counter that counts down until their turn can happen

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurnManager : MonoBehaviour {

  [SerializeField] private InputActionReference moveAction;
  [SerializeField] private GridMovementController playerMovementController;
  [SerializeField] private GridMovementAnimator playerMovementAnimator;

  // How long do different actions take
  [Header("Durations")]
  [SerializeField] private float turnTime = 0.3f;
  [SerializeField] private float strideTime = 0.3f;
  [SerializeField] private float bumpTime = 0.4f;
  [SerializeField] private float standTime = 0.4f;

  private double lastInputTime;
  private UniTask moveChain;
  private readonly List<TurnCounterAction> turnCounters = new();

  void Start() {

  }

  void OnEnable() {
    moveAction.action.performed += OnPlayerInput;
    moveAction.action.canceled += OnPlayerInput;
  }

  void OnDisable() {
    moveAction.action.performed -= OnPlayerInput;
    moveAction.action.canceled -= OnPlayerInput;
  }

  void OnPlayerInput(InputAction.CallbackContext context) {
    var input = context.ReadValue<Vector2>();
    // Store context's time to a local variable so the anonymous function below captures it rather than the context
    var inputTime = context.time;
    lastInputTime = inputTime;

    moveChain = moveChain.ContinueWith(() => ProcessTurn(input, inputTime));
  }

  async UniTask ProcessTurn(Vector2 direction, double inputTime) {
    if (!enabled) return;

    // Queue up all the things that need to process in order, now that the player has press a move button
    if (direction.sqrMagnitude > 0) {
      var outcome = playerMovementController.OnMove(direction);
      var timer = outcome switch {
        Turn turn => turnTime,
        Stride stride => strideTime,
        Bump bump => bumpTime,
        Stand => standTime,
      };

      var gameState = new GameState { playerPosition = outcome.Position };
      var outcomes = turnCounters.Select((action) => (outcome: action.Increment(gameState), gameObject: action.gameObject));

      var animators = new[] { (outcome, gameObject: playerMovementAnimator.gameObject) }.Concat(outcomes).Select((pair) => {
        if (!pair.gameObject.TryGetComponent<GridMovementAnimator>(out var animator)) {
          return pair.outcome switch {
            Turn turn => DefaultGridMovement.Turn(pair.gameObject.transform, turn.From, turn.Delta),
            Stride stride => DefaultGridMovement.Stride(pair.gameObject.transform, stride.From, stride.Position),
            Bump bump => DefaultGridMovement.Bump(pair.gameObject.transform, bump.Position, bump.Facing),
            Stand => progress => { }
          };
        }
        return pair.outcome switch {
          Turn turn => animator.Turn(turn.From, turn.Delta),
          Stride stride => animator.Stride(stride.From, stride.Position),
          Bump bump => animator.Bump(bump.Position, bump.Facing),
          Stand => progress => { }
        };
      });


      var elapsed = 0f;
      while (elapsed < timer) {
        foreach (var animator in animators) animator(elapsed / timer);
        await UniTask.NextFrame();
        elapsed += Time.deltaTime;
      }
      foreach (var animator in animators) animator(1f);
    }

    // If the input time hasn't changed in the meantime, keep moving (button is held)
    if (direction.sqrMagnitude > 0 && lastInputTime == inputTime) {
      await ProcessTurn(direction, inputTime);
    }
    if (destroyCancellationToken.IsCancellationRequested) return;
  }

  public void Register(TurnCounterAction turnCounter) {
    turnCounters.Add(turnCounter);
  }
}
