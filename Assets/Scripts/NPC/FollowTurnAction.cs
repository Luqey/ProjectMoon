using UnityEngine;

public class FollowTurnAction : TurnCounterAction {
  protected override Vector2 TurnAction(GameState context) {
    var position = gridMovementController.Position;
    var diff = context.playerPosition - position;
    var heading = Heading.GetHeadingFromDirection(diff);
    if (!heading.HasValue) return Vector2.zero;
    var right = Heading.GetTurnDirection(gridMovementController.Facing, heading.Value);
    if (right != 0) return new Vector2(right, 0);
    if (diff.sqrMagnitude != 0) return new Vector2(0, 1);
    return Vector2.zero;
  }
}
