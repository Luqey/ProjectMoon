using System;
using UnityEngine;
using Math = Moon.Math;

public static class Heading {
  public static Vector2Int North => new(0, 1);
  public static Vector2Int NorthEast => new(1, 1);
  public static Vector2Int East => new(1, 0);
  public static Vector2Int SouthEast => new(1, -1);
  public static Vector2Int South => new(0, -1);
  public static Vector2Int SouthWest => new(-1, -1);
  public static Vector2Int West => new(-1, 0);
  public static Vector2Int NorthWest => new(-1, 1);
  public static Vector2Int GetDirectionFromHeading(int heading) {

    return Math.Mod(heading, 8) switch {
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

  public static int? GetHeadingFromDirection(Vector2Int direction) {
    var n = new Vector2Int(Math.ZeroSign(direction.x), Math.ZeroSign(direction.y));
    for (var i = 0; i < 8; i += 1) {
      if (GetDirectionFromHeading(i) == n) return i;
    }
    return null;
  }

  public static int GetTurnDirection(int a, int b) {
    int clockwise = Math.Mod(b - a, 8);
    if (clockwise == 0) return 0;
    return clockwise <= 4 ? 1 : -1;
  }

  public static int Turn(int heading, int right) {
    return Math.Mod(heading + right, 8);
  }

  public static int Right(int heading, int right) {
    return Math.Mod(heading + 2 * right, 8);
  }

  public static int Reverse(int heading) {
    return Math.Mod(heading + 4, 8);
  }

  public static int Resolve(int heading, int forward) {
    return forward >= 0 ? heading : Reverse(heading);
  }
}


public abstract record ActionOutcome(Vector2Int Position);
public sealed record Turn(Vector2Int Position, int From, int To, int Delta) : ActionOutcome(Position);
public sealed record Stride(Vector2Int Position, Vector2Int From) : ActionOutcome(Position);
public sealed record Bump(Vector2Int Position, int Facing, int Heading, RaycastHit Hit) : ActionOutcome(Position);
public sealed record Stand(Vector2Int Position) : ActionOutcome(Position);

public class GridMovementController : MonoBehaviour {
  private Vector2Int gridPosition;
  public Vector2Int Position => gridPosition;
  public int Facing { get; private set; }

  public int eyeHeight = 14;

  [SerializeField] private LayerMask colliderMask;

  void Start() {
    // Snap to grid on start
    gridPosition = Grid.ToGrid(transform.position);
    Facing = Mathf.FloorToInt(transform.rotation.eulerAngles.y / 45);
    transform.SetPositionAndRotation(
      Grid.FromGrid(gridPosition, withY: transform.position.y),
      Quaternion.Euler(0, Facing * 45, 0)
    );
  }

  public ActionOutcome OnMove(Vector2 direction, bool strafe) {
    if (strafe || direction.y != 0) {
      var forward = Math.ZeroSign(direction.y);
      var right = Math.ZeroSign(direction.x);
      var heading = GetHeading(forward, right);
      if (!heading.HasValue) return new Stand(gridPosition);
      var gridDelta = Heading.GetDirectionFromHeading(heading.Value);
      var eyePosition = Grid.FromGrid(gridPosition, withY: transform.position.y) + new Vector3(0, eyeHeight, 0);
      if (Physics.Raycast(eyePosition, new Vector3(gridDelta.x, 0, gridDelta.y), out var hit, Grid.size * gridDelta.magnitude, colliderMask)) {
        return new Bump(gridPosition, Facing, heading.Value, hit);
      }
      var from = gridPosition;
      gridPosition += gridDelta;
      return new Stride(gridPosition, from);
    }

    if (direction.x != 0) {
      var from = Facing;
      var right = Math.Sign(direction.x);
      Facing = Heading.Turn(Facing, right);
      return new Turn(gridPosition, from, Facing, right);
    }

    return new Stand(gridPosition);
  }

  private int? GetHeading(int forward, int right) {
    if (forward != 0) return Heading.Resolve(Facing, forward);
    if (right != 0) return Heading.Right(Facing, right);
    return null;
  }
}
