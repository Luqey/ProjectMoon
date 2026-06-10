using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public interface IGridMovementAnimator {
  public Action<float> Stride(Vector2Int from, Vector2Int to);
  public Action<float> Turn(int from, int delta);
  public Action<float> Bump(Vector2Int position, int heading, float distance);
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

  public static Action<float> Bump(Transform transform, Vector2Int position, int heading, float distance) {
    var gridDistance = Grid.ToGrid(distance);
    var gridDelta = Heading.GetDirectionFromHeading(heading);
    var startPosition = Grid.FromGrid(position, withY: transform.position.y);
    var bumpDelta = Vector3.Scale(Grid.FromGrid(gridDelta), new Vector3(bumpDistance * gridDistance, 1, bumpDistance * gridDistance));
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
  public Action<float> Bump(Vector2Int position, int heading, float distance) => DefaultGridMovement.Bump(transform, position, heading, distance);
  public Action<float> Stand() => DefaultGridMovement.Stand(transform);
  public Action<float> Stride(Vector2Int from, Vector2Int to) => DefaultGridMovement.Stride(transform, from, to);
  public Action<float> Turn(int from, int delta) => DefaultGridMovement.Turn(transform, from, delta);
}

[Serializable]
public enum MovementType {
  Stride,
  Turn,
  Bump,
  Stand,
}

[Serializable]
public struct ThresholdAction {
  public MovementType MovementType;
  public float Threshold;
  public UnityEvent Event;
}

public class GridMovementAnimator : MonoBehaviour, IGridMovementAnimator {
  [SerializeField] private List<ThresholdAction> movementTriggers;

  private List<ThresholdTrigger<float>> GetTriggers(MovementType movementType) {
    return movementTriggers
      .Where(trigger => trigger.MovementType == movementType)
      .Select(trigger => new ThresholdTrigger<float>(trigger.Threshold, trigger.Event.Invoke)).ToList();
  }

  public Action<float> Stride(Vector2Int from, Vector2Int to) {
    var triggers = GetTriggers(MovementType.Stride);
    var f = DefaultGridMovement.Stride(transform, from, to);
    return progress => {
      f(progress);
      foreach (var t in triggers) t.Update(progress);
    };
  }

  public Action<float> Turn(int from, int delta) {
    var triggers = GetTriggers(MovementType.Turn);
    var f = DefaultGridMovement.Turn(transform, from, delta);
    return progress => {
      f(progress);
      foreach (var t in triggers) t.Update(progress);
    };
  }
  public Action<float> Bump(Vector2Int position, int heading, float distance) {
    var triggers = GetTriggers(MovementType.Bump);
    var f = DefaultGridMovement.Bump(transform, position, heading, distance);
    return progress => {
      f(progress);
      foreach (var t in triggers) t.Update(progress);
    };
  }

  public Action<float> Stand() {
    var triggers = GetTriggers(MovementType.Stand);
    var f = DefaultGridMovement.Stand(transform);
    return progress => {
      f(progress);
      foreach (var t in triggers) t.Update(progress);
    };
  }
}
