using UnityEngine;

public class FollowTurnAction : TurnCounterAction {
  protected override Vector2 TurnAction(GameState context) {
    var position = gridMovementController.Position;
    var facing = gridMovementController.facing;
    var diff = context.playerPosition - position;
    var right = Facing.GetTurnDirection(facing, Facing.GetFacingFromDirection(diff));
    if (right != 0) return new Vector2(right, 0);
    if (diff.sqrMagnitude != 0) return new Vector2(0, 1);
    return Vector2.zero;
  }
}
