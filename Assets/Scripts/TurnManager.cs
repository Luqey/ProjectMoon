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
      var gameState = new GameState { playerPosition = outcome.Position };
      var outcomes = turnCounters.Select((action) => (outcome: action.Increment(gameState), animator: action.GetComponent<GridMovementAnimator>()));

      var tasks = new[] { (outcome, animator: playerMovementAnimator) }.Concat(outcomes).Select((pair) => {
        return pair.outcome switch {
          Turn turn => pair.animator.TurnTask(turn.From, turn.Delta),
          Stride stride => pair.animator.StrideTask(stride.From, stride.Position),
          Bump bump => pair.animator.BumpTask(bump.Position, bump.Facing),
          None => UniTask.CompletedTask,
        };
      });

      await UniTask.WhenAll(tasks);
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
