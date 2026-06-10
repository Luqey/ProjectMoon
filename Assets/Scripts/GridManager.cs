using UnityEngine;

public static class Grid {
  public static readonly int size = 48;

  public static int FromGrid(int x) {
    return x * size;
  }

  public static float ToGrid(float x) {
    return x / size;
  }

  public static int RoundToGrid(float x) {
    return Mathf.RoundToInt(ToGrid(x));
  }

  public static Vector3 FromGrid(Vector2Int pos, float withY = 0f) {
    return new(FromGrid(pos.x), withY, FromGrid(pos.y));
  }

  public static Vector2 ToGrid(Vector3 pos) {
    return new(ToGrid(pos.x), ToGrid(pos.z));
  }

  public static Vector2Int RoundToGrid(Vector3 pos) {
    return new(RoundToGrid(pos.x), RoundToGrid(pos.z));
  }
}
