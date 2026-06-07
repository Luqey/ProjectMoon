using UnityEngine;

public class GameState {
  public Vector2Int playerPosition;
}

public abstract class TurnCounterAction : MonoBehaviour {
  [SerializeField] protected GridMovementController gridMovementController;
  [SerializeField] private TurnManager turnManager;
  [SerializeField] private int turnCounter = 1;
  private int counter = 0;

  protected abstract Vector2 TurnAction(GameState context);

  void Start() {
    turnManager.Register(this);
  }

  public ActionOutcome Increment(GameState context) {
    var direction = Vector2.zero;
    counter += 1;
    if (counter % turnCounter == 0) {
      direction = TurnAction(context);
    }
    Debug.Log(name + direction);
    return gridMovementController.OnMove(direction, strafe: false);
  }
}
