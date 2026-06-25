using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace Moon {

  public class SlideLoop : MonoBehaviour {
    [SerializeField] float speed = 100f;
    [SerializeField] float tileSize = 32f;

    private readonly List<Vector3> startPositions = new();

    void OnDrawGizmos() {
      var min = 0f;
      var max = 0f;
      for (var i = 0; i < transform.childCount; i += 1) {
        var child = transform.GetChild(i);
        max = child.localPosition.z > max ? child.localPosition.z : max;
        min = child.localPosition.z < min ? child.localPosition.z : min;
      }

      var from = transform.position + transform.TransformVector(new Vector3(0, 0, min));
      var to = transform.position + transform.TransformVector(new Vector3(0, 0, max));

      var d = transform.TransformVector(new Vector3(16, 0, 0));
      Gizmos.DrawLine(from - d, from + d);
      Gizmos.DrawLine(to - d, to + d);
      Gizmos.DrawLine(from, to);
    }

    void Start() {
      for (var i = 0; i < transform.childCount; i += 1) {
        var obj = transform.GetChild(i);
        startPositions.Add(obj.localPosition);
      }
    }

    private float offset = 0;

    void Update() {
      offset = (offset + speed * Time.deltaTime) % (transform.childCount * tileSize);
      var index = (int)(offset / tileSize) % startPositions.Count;
      for (var i = 0; i < transform.childCount; i += 1) {
        var child = transform.GetChild(i);
        child.localPosition = startPositions[(index + i) % startPositions.Count] + Vector3.forward * (offset % tileSize);
      }
    }
  }

}
