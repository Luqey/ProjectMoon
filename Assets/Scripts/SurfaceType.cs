using System;
using UnityEngine;

[Flags]
public enum SurfaceType {
  None = 0,
  Grass = 1 << 0,
  Wood = 1 << 1,
  Stone = 1 << 2,
  Metal = 1 << 3,
}

public class Surface : MonoBehaviour {
  public SurfaceType Type;
}
