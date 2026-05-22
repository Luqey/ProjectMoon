namespace Moon {
  public static class Math {
    public static int ZeroSign(float x) {
      return x == 0f ? 0 : x < 0f ? -1 : 1;
    }

    public static int Sign(float x) {
      return x < 0f ? -1 : 1;
    }
  }
}
