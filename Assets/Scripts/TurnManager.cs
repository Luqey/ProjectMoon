using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
  private CancellationTokenSource cts;
  private readonly List<TurnCounterAction> turnCounters = new();

  void OnEnable() {
    cts = new CancellationTokenSource();
    moveChain = UniTask.CompletedTask;
    moveAction.action.performed += OnPlayerInput;
    moveAction.action.canceled += OnPlayerInput;
  }

  void OnDisable() {
    moveAction.action.performed -= OnPlayerInput;
    moveAction.action.canceled -= OnPlayerInput;
    cts.Cancel();
    cts.Dispose();
  }

  void OnPlayerInput(InputAction.CallbackContext context) {
    var input = context.ReadValue<Vector2>();
    var inputTime = context.time;
    lastInputTime = inputTime;

    var token = cts.Token;
    moveChain = moveChain.ContinueWith(() => ProcessTurn(input, inputTime, token));
  }

  async UniTask ProcessTurn(Vector2 direction, double inputTime, CancellationToken token) {
    token.ThrowIfCancellationRequested();

    if (direction.sqrMagnitude > 0) {
      var outcome = playerMovementController.OnMove(direction);
      var timer = outcome switch {
        Turn turn => turnTime,
        Stride stride => strideTime,
        Bump bump => bumpTime,
        Stand => standTime,
        _ => throw new System.Exception("Fucking explode")
      };

      var gameState = new GameState { playerPosition = outcome.Position };
      var outcomes = turnCounters.Select((action) => (outcome: action.Increment(gameState), action.gameObject)).ToList();

      var animators = new[] { (outcome, playerMovementAnimator.gameObject) }.Concat(outcomes).Select((pair) => {
        IGridMovementAnimator animator = pair.gameObject.GetComponent<GridMovementAnimator>();
        animator ??= new DefaultGridMovementAnimator(pair.gameObject.transform);

        return pair.outcome switch {
          Turn turn => animator.Turn(turn.From, turn.Delta),
          Stride stride => animator.Stride(stride.From, stride.Position),
          Bump bump => animator.Bump(bump.Position, bump.Facing),
          Stand => progress => { },
          _ => throw new System.Exception("Fucking explode")
        };
      });

      var elapsed = 0f;
      while (elapsed < timer) {
        foreach (var animator in animators) animator(elapsed / timer);
        await UniTask.NextFrame(token);
        elapsed += Time.deltaTime;
      }
      foreach (var animator in animators) animator(1f);
    }

    if (direction.sqrMagnitude > 0 && lastInputTime == inputTime) {
      await ProcessTurn(direction, inputTime, token);
    }
  }

  public void Register(TurnCounterAction turnCounter) {
    turnCounters.Add(turnCounter);
  }
}
