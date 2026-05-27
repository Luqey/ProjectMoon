using System;
using UnityEngine;
using Math = Moon.Math;

public static class Facing {
  public static Vector2Int North => new(0, 1);
  public static Vector2Int NorthEast => new(1, 1);
  public static Vector2Int East => new(1, 0);
  public static Vector2Int SouthEast => new(1, -1);
  public static Vector2Int South => new(0, -1);
  public static Vector2Int SouthWest => new(-1, -1);
  public static Vector2Int West => new(-1, 0);
  public static Vector2Int NorthWest => new(-1, 1);
  public static Vector2Int GetFacingDirection(int facing) {

    return Math.Mod(facing, 8) switch {
      0 => North,
      1 => NorthEast,
      2 => East,
      3 => SouthEast,
      4 => South,
      5 => SouthWest,
      6 => West,
      7 => NorthWest,
      _ => throw new Exception("Literally impossible!")
    };
  }

  public static int Turn(int facing, int right) {
    return Math.Mod(facing + right, 8);
  }

  public static int Reverse(int facing) {
    return Math.Mod(facing + 4, 8);
  }

  public static int Resolve(int facing, int forward) {
    return forward > 0 ? facing : Reverse(facing);
  }
}


public abstract record ActionOutcome(Vector2Int Position);
public sealed record Turn(Vector2Int Position, int From, int To, int Delta) : ActionOutcome(Position);
public sealed record Stride(Vector2Int Position, Vector2Int From) : ActionOutcome(Position);
public sealed record Bump(Vector2Int Position, int Facing, int Forward, RaycastHit Hit) : ActionOutcome(Position);
public sealed record Stand(Vector2Int Position) : ActionOutcome(Position);

public class GridMovementController : MonoBehaviour {
  private Vector2Int gridPosition;
  public Vector2Int Position => gridPosition;
  private int facing;

  public int eyeHeight = 14;

  [SerializeField] private LayerMask colliderMask;

  void Start() {
    // Snap to grid on start
    gridPosition = Grid.ToGrid(transform.position);
    facing = Mathf.FloorToInt(transform.rotation.eulerAngles.y / 45);
    transform.SetPositionAndRotation(
      Grid.FromGrid(gridPosition, withY: transform.position.y),
      Quaternion.Euler(0, facing * 45, 0)
    );
  }

  public ActionOutcome OnMove(Vector2 direction) {
    if (direction.x != 0) {
      var from = facing;
      var right = Math.Sign(direction.x);
      facing = Facing.Turn(facing, right);
      return new Turn(gridPosition, from, facing, right);
    }
    if (direction.y != 0) {
      var forward = Math.Sign(direction.y);
      var gridDelta = Facing.GetFacingDirection(Facing.Resolve(facing, forward));
      var eyePosition = Grid.FromGrid(gridPosition, withY: transform.position.y) + new Vector3(0, eyeHeight, 0);
      if (Physics.Raycast(eyePosition, new Vector3(gridDelta.x, 0, gridDelta.y), out var hit, Grid.size * gridDelta.magnitude, colliderMask)) {
        return new Bump(gridPosition, facing, forward, hit);
      } else {
        var from = gridPosition;
        gridPosition += gridDelta;
        return new Stride(gridPosition, from);
      }
    }

    return new Stand(gridPosition);
  }
}
