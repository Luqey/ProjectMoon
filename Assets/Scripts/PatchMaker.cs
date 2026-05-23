using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PatchMaker : MonoBehaviour {
  public GameObject prefab;
  public int gridSize = 16;
  public float radius = 1f;

  [SerializeField] private List<GameObject> spawnedObjects = new();

  void OnValidate() {
#if UNITY_EDITOR
    EditorApplication.delayCall -= Refresh;
    EditorApplication.delayCall += Refresh;
#endif
  }

  [ContextMenu("Refresh")]
  void Refresh() {
#if UNITY_EDITOR
    EditorApplication.delayCall -= Refresh;
    if (this == null) return;
#endif
    ClearSpawned();
    if (prefab == null || radius <= 0) return;

    int r = Mathf.CeilToInt(radius);
    var origin = transform.position;

    for (int x = -r; x <= r; x++) {
      for (int y = -r; y <= r; y++) {
        if (x * x + y * y > radius * radius) continue;
        var worldPos = new Vector3(origin.x + x * gridSize, origin.y, origin.z + y * gridSize);
#if UNITY_EDITOR
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
#else
                var obj = Instantiate(prefab, transform);
#endif
        obj.transform.position = worldPos;
        spawnedObjects.Add(obj);
      }
    }
  }

  void ClearSpawned() {
    for (var i = 0; i < transform.childCount; i += 1) {
      var child = transform.GetChild(i);
      if (Application.isPlaying) {
        Destroy(child.gameObject);
      } else {
        DestroyImmediate(child.gameObject);
      }
    }
    spawnedObjects.Clear();
  }

  void OnDestroy() {
    ClearSpawned();
  }
}
