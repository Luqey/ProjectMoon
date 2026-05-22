using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Math = Moon.Math;

public class GridMovementController : MonoBehaviour {
  private Vector2Int gridPosition;
  public float walkTime = 0.3f;
  public float turnTime = 0.2f;
  public float bumpForwardTime = 0.15f;
  public float bumpBackwardTime = 0.1f;
  public float bumpDistance = 0.5f;
  [SerializeField] private LayerMask colliderMask;
  [SerializeField] private InputActionReference moveAction;
  [SerializeField] private TurnManager turnManager;

  private UniTask moveChain = UniTask.CompletedTask;
  private double lastInputTimestamp;

  public Action<float, Vector2Int> walking;
  public Action<float> turning;
  public Action<float, RaycastHit> bumping;

  void Start() {
    // Snap to grid on start
    gridPosition = Grid.ToGrid(transform.position);
    var angle = Mathf.FloorToInt(transform.rotation.eulerAngles.y / 45) * 45;
    transform.SetPositionAndRotation(
      Grid.FromGrid(gridPosition, withY: transform.position.y),
      Quaternion.Euler(0, angle, 0)
    );
  }

  void OnEnable() {
    moveAction.action.performed += OnMove;
    moveAction.action.canceled += OnMove;
  }

  void OnDisable() {
    moveAction.action.performed -= OnMove;
    moveAction.action.canceled -= OnMove;
  }

  void OnMove(InputAction.CallbackContext context) {
    var inputTime = context.time;
    lastInputTimestamp = inputTime;
    var input = context.ReadValue<Vector2>();
    if (input.sqrMagnitude > 0) {
      moveChain = moveChain.ContinueWith(() => Move(input, inputTime));
    }
  }

  async UniTask Move(Vector2 direction, double inputTime) {
    Debug.Log(direction);
    if (!enabled) return;

    if (direction.y != 0) {
      await Walk(Math.Sign(direction.y));
    }
    if (direction.x != 0) {
      await Turn(Math.Sign(direction.x));
    }

    // If the input time hasn't changed in the meantime, keep moving (button is held)
    if (direction.sqrMagnitude > 0 && lastInputTimestamp == inputTime) {
      await Move(direction, inputTime);
    }
    if (destroyCancellationToken.IsCancellationRequested) return;
  }

  async UniTask Walk(int forward) {
    var direction = forward * transform.forward;
    var halfHeightPosition = Vector3.Scale(transform.position, new Vector3(1, 0.5f, 1));
    var gridDelta = new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.z));
    if (Physics.Raycast(halfHeightPosition, direction, out var hit, Grid.size * gridDelta.magnitude, colliderMask)) {
      turnManager.EndPlayerTurn(new() {playerPosition = gridPosition});
      await Bump(forward, hit);
    } else {
      gridPosition += gridDelta;
      turnManager.EndPlayerTurn(new() {playerPosition = gridPosition});
      await Stride(gridDelta);
    }
  }

  async UniTask Stride(Vector2Int delta) {
      var startPosition = transform.position;
      var target = Grid.FromGrid(gridPosition, withY: transform.position.y);
      var elapsed = 0f;
      var scaledWalkTime = walkTime * delta.magnitude;
      while (elapsed < scaledWalkTime) {
        walking?.Invoke(elapsed / walkTime, gridPosition);
        elapsed += Time.deltaTime;
        transform.position = Vector3.Lerp(startPosition, target, elapsed / scaledWalkTime);
        await UniTask.NextFrame();
        if (destroyCancellationToken.IsCancellationRequested) break;
      }
      walking?.Invoke(1f, gridPosition);
  }

  async UniTask Turn(int right) {
    var start = transform.rotation;
    var target = transform.rotation * Quaternion.Euler(0, right * 45, 0);
    var elapsed = 0f;
    while (elapsed < turnTime) {
      turning?.Invoke(elapsed / turnTime);
      elapsed += Time.deltaTime;
      transform.rotation = Quaternion.Lerp(start, target, elapsed / turnTime);
      await UniTask.NextFrame();
      if (destroyCancellationToken.IsCancellationRequested) break;
    }
    turning?.Invoke(1f);
  }

  async UniTask Bump(int forward, RaycastHit hit) {
    var direction = forward * transform.forward;
    var gridDelta = new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.z));
    var bumpDelta = Vector3.Scale(Grid.FromGrid(gridDelta, transform.position.y), new Vector3(bumpDistance, 1, bumpDistance));
    var startPosition = transform.position;
    var target = startPosition + bumpDelta;
    var elapsed = 0f;
    var scaledBumpForwardTime = bumpForwardTime * gridDelta.magnitude;
    var scaledBumpBackwardTime = bumpBackwardTime * gridDelta.magnitude;
    var bumpTime = scaledBumpForwardTime + scaledBumpBackwardTime;
    while (elapsed < bumpTime) {
      var bumpForward = elapsed < scaledBumpForwardTime;
      bumping?.Invoke(elapsed / bumpTime, hit);
      elapsed += Time.deltaTime;
      transform.position = Vector3.Lerp(bumpForward ? startPosition : target, bumpForward ? target : startPosition, elapsed / bumpTime);
      await UniTask.NextFrame();
      if (destroyCancellationToken.IsCancellationRequested) break;
    }
    bumping?.Invoke(1, hit);
  }
}
