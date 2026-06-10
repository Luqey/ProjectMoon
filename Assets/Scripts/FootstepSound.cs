using System;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSound : MonoBehaviour {

  [Serializable]
  public struct SurfaceSound {
    public SurfaceType surfaceType;
    public List<AudioClip> clips;
  }

  [SerializeField] private LayerMask layerMask;
  [SerializeField] private AudioSource audioSource;
  [SerializeField] private List<SurfaceSound> surfaceSounds;

  public void Fire() {
    var surfaceType = SurfaceType.Grass;
    if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out var hit, 2, layerMask)) {
      if (hit.collider.gameObject.TryGetComponent<Surface>(out var surface)) {
        surfaceType = surface.Type;
      }
    }

    foreach (var ss in surfaceSounds) {
      if (surfaceType.HasFlag(ss.surfaceType)) {
        var clip = ss.clips[UnityEngine.Random.Range(0, ss.clips.Count)];
        audioSource.clip = clip;
        audioSource.Play();
      }
    }
  }
}
