using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IGridMovementAnimator {
  public Action<float> Stride(Vector2Int from, Vector2Int to);
  public Action<float> Turn(int from, int delta);
  public Action<float> Bump(Vector2Int position, int facing);
  public Action<float> Stand();
}

public static class DefaultGridMovement {
  public static float bumpDistance = 0.5f;
  public static float bumpForwardPortion = 0.3f;

  public static Action<float> Stride(Transform transform, Vector2Int from, Vector2Int to) {
    var start = Grid.FromGrid(from, withY: transform.position.y);
    var target = Grid.FromGrid(to, withY: transform.position.y);
    return progress => {
      transform.position = Vector3.Lerp(start, target, progress);
    };
  }

  public static Action<float> Turn(Transform transform, int from, int delta) {
    var start = Quaternion.Euler(0, from * 45, 0);
    var target = start * Quaternion.Euler(0, delta * 45, 0);
    return progress => {
      transform.rotation = Quaternion.Lerp(start, target, progress);
    };
  }

  public static Action<float> Bump(Transform transform, Vector2Int position, int facing) {
    var gridDelta = Facing.GetDirectionFromFacing(facing);
    var startPosition = Grid.FromGrid(position, withY: transform.position.y);
    var bumpDelta = Vector3.Scale(Grid.FromGrid(gridDelta, withY: transform.position.y), new Vector3(bumpDistance, 1, bumpDistance));
    var target = startPosition + bumpDelta;
    return progress => {
      var bumpForward = progress < bumpForwardPortion;
      if (bumpForward) {
        transform.position = Vector3.Lerp(startPosition, target, progress / bumpForwardPortion);
      } else {
        transform.position = Vector3.Lerp(target, startPosition, (progress - bumpForwardPortion) / (1f - bumpForwardPortion));
      }
    };
  }

  public static Action<float> Stand(Transform transform) {
    return progress => { };
  }
}

public class DefaultGridMovementAnimator : IGridMovementAnimator {
  private readonly Transform transform;
  public DefaultGridMovementAnimator(Transform t) => transform = t;
  public Action<float> Bump(Vector2Int position, int facing) => DefaultGridMovement.Bump(transform, position, facing);
  public Action<float> Stand() => DefaultGridMovement.Stand(transform);
  public Action<float> Stride(Vector2Int from, Vector2Int to) => DefaultGridMovement.Stride(transform, from, to);
  public Action<float> Turn(int from, int delta) => DefaultGridMovement.Turn(transform, from, delta);
}

public class GridMovementAnimator : MonoBehaviour, IGridMovementAnimator {
  public Action<float> Stride(Vector2Int from, Vector2Int to) => DefaultGridMovement.Stride(transform, from, to);
  public Action<float> Turn(int from, int delta) => DefaultGridMovement.Turn(transform, from, delta);
  public Action<float> Bump(Vector2Int position, int facing) => DefaultGridMovement.Bump(transform, position, facing);
  public Action<float> Stand() => DefaultGridMovement.Stand(transform);
}
