using UnityEngine;

public class GameState {
  public Vector2Int playerPosition;
}

public abstract class TurnCounterAction : MonoBehaviour {
  [SerializeField] private GridMovementController gridMovementController;
  [SerializeField] private int turnCounter;
  private int counter = 0;

  protected abstract Vector2 TurnAction(GameState context);

  public ActionOutcome Increment(GameState context) {
    var direction = Vector2.zero;
    counter += 1;
    if (counter % turnCounter == 0) {
      direction = TurnAction(context);
    }
    return gridMovementController.OnMove(direction);
  }
}
