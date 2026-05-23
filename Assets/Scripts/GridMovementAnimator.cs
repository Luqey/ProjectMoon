using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GridMovementAnimator : MonoBehaviour {

  public float walkTime = 0.3f;
  public float turnTime = 0.2f;
  public float bumpForwardTime = 0.15f;
  public float bumpBackwardTime = 0.1f;
  public float bumpDistance = 0.5f;

  public async UniTask StrideTask(Vector2Int from, Vector2Int to, Action<float> progress = null) {
    var start = Grid.FromGrid(from, withY: transform.position.y);
    var target = Grid.FromGrid(to, withY: transform.position.y);
    var elapsed = 0f;
    var delta = to - from;
    var scaledWalkTime = walkTime * delta.magnitude;
    while (elapsed < scaledWalkTime) {
      progress?.Invoke(elapsed / walkTime);
      elapsed += Time.deltaTime;
      transform.position = Vector3.Lerp(start, target, elapsed / scaledWalkTime);
      await UniTask.NextFrame();
      if (destroyCancellationToken.IsCancellationRequested) break;
    }
    progress?.Invoke(1f);
  }

  public async UniTask TurnTask(int from, int delta, Action<float> progress = null) {
    var start = Quaternion.Euler(0, from * 45, 0);
    var target = start * Quaternion.Euler(0, delta * 45, 0);
    var elapsed = 0f;
    while (elapsed < turnTime) {
      progress?.Invoke(elapsed / turnTime);
      elapsed += Time.deltaTime;
      transform.rotation = Quaternion.Lerp(start, target, elapsed / turnTime);
      await UniTask.NextFrame();
      if (destroyCancellationToken.IsCancellationRequested) break;
    }
    progress?.Invoke(1f);
  }

  public async UniTask BumpTask(Vector2Int position, int facing, Action<float> progress = null) {
    var gridDelta = Facing.GetFacingDirection(facing);
    var startPosition = Grid.FromGrid(position, withY: transform.position.y);
    var bumpDelta = Vector3.Scale(Grid.FromGrid(gridDelta, withY: transform.position.y), new Vector3(bumpDistance, 1, bumpDistance));
    var target = startPosition + bumpDelta;
    var elapsed = 0f;
    var scaledBumpForwardTime = bumpForwardTime * gridDelta.magnitude;
    var scaledBumpBackwardTime = bumpBackwardTime * gridDelta.magnitude;
    var bumpTime = scaledBumpForwardTime + scaledBumpBackwardTime;
    while (elapsed < bumpTime) {
      var bumpForward = elapsed < scaledBumpForwardTime;
      progress?.Invoke(elapsed / bumpTime);
      elapsed += Time.deltaTime;
      transform.position = Vector3.Lerp(bumpForward ? startPosition : target, bumpForward ? target : startPosition, elapsed / bumpTime);
      await UniTask.NextFrame();
      if (destroyCancellationToken.IsCancellationRequested) break;
    }
    progress?.Invoke(1);
  }
}
