using UnityEngine;

public class FollowTurnAction : TurnCounterAction {
  protected override Vector2 TurnAction(GameState context) {
    var position = gridMovementController.Position;
    var diff = context.playerPosition - position;
    var facing = Facing.GetFacingFromDirection(diff);
    if (!facing.HasValue) return Vector2.zero;
    var right = Facing.GetTurnDirection(gridMovementController.facing, facing.Value);
    if (right != 0) return new Vector2(right, 0);
    if (diff.sqrMagnitude != 0) return new Vector2(0, 1);
    return Vector2.zero;
  }
}
