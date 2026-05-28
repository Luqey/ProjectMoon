using UnityEngine;

public static class Grid {
  public static readonly int size = 48;

  public static Vector2 FromGrid(Vector2Int pos) {
    return pos * size;
  }

  public static Vector3 FromGrid(Vector2Int pos, float withY = 0f) {
    return new(pos.x * size, withY, pos.y * size);
  }

  public static Vector2Int ToGrid(Vector3 pos) {
    return new(Mathf.RoundToInt(pos.x / size), Mathf.RoundToInt(pos.z / size));
  }
}
