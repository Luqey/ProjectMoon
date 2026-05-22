// Handles player and npc turns. In the player's case, wait for input to progress the turn, and npcs wait for the player's turn to start before doing their turn
// Turns happen "simultaneously" except that players technically move first before npcs


// All NPCs have a turn counter that counts down until their turn can happen

using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

  private readonly List<TurnCounterAction> turnCounters = new();

  public void Register(TurnCounterAction turnCounter) {
    turnCounters.Add(turnCounter);
  }

  public void EndPlayerTurn(GameState context) {
    foreach (var turnCounter in turnCounters) {
      turnCounter.Increment(context);
    }
  }
}
