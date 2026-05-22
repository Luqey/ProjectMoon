using UnityEngine;

public class GameState {
  public Vector2Int playerPosition;
}

public abstract class TurnCounterAction : MonoBehaviour {
  [SerializeField] private int turnCounter;
  private int counter = 0;

  protected abstract void TurnAction(GameState context);

  public void Increment(GameState context) {
    counter += 1;
    if (counter % turnCounter == 0) {
      TurnAction(context);
    }
  }
}
