using UnityEngine;

namespace Moon {
  public static class Util {
  }


  public static class GizmoExtensions {
    public static void DrawArrow(Vector3 from, Vector3 to, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f) {
      // Draw the main shaft of the arrow
      Gizmos.DrawLine(from, to);

      // Don't draw head if the line is negligible
      Vector3 direction = to - from;
      if (direction.sqrMagnitude < 0.001f) return;

      // Calculate arrowhead sides
      Vector3 lookRotation = direction.normalized;
      Vector3 right = Quaternion.LookRotation(lookRotation) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
      Vector3 left = Quaternion.LookRotation(lookRotation) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;

      // Draw the arrowhead wings
      Gizmos.DrawLine(to, to + right * arrowHeadLength);
      Gizmos.DrawLine(to, to + left * arrowHeadLength);
    }
  }
}
