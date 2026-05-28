using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

[ExecuteAlways]
public class PatchMaker : MonoBehaviour {
  public GameObject prefab;
  public int gridSize = 16;
  public float radius = 1f;

  [SerializeField] private List<GameObject> spawnedObjects = new();

  void OnValidate() {
    if (PrefabUtility.IsPartOfPrefabAsset(this)) return;
    EditorApplication.delayCall -= Refresh;
    EditorApplication.delayCall += Refresh;
  }

  [ContextMenu("Refresh")]
  void Refresh() {
    EditorApplication.delayCall -= Refresh;
    if (this == null) return;
    ClearSpawned();
    if (prefab == null || radius <= 0) return;

    int r = Mathf.CeilToInt(radius);
    var origin = transform.position;

    for (int x = -r; x <= r; x++) {
      for (int y = -r; y <= r; y++) {
        if (x * x + y * y > radius * radius) continue;
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, transform);
        obj.transform.position = new Vector3(origin.x + x * gridSize, origin.y, origin.z + y * gridSize);
        spawnedObjects.Add(obj);
      }
    }
  }

  void ClearSpawned() {
    for (var i = transform.childCount - 1; i >= 0; i--) {
      DestroyImmediate(transform.GetChild(i).gameObject);
    }
    spawnedObjects.Clear();
  }

  void OnDestroy() {
    ClearSpawned();
  }
}
#else
public class PatchMaker : UnityEngine.MonoBehaviour { }
#endif
