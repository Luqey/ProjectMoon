using UnityEditor;
using UnityEngine;

public class TrainParallax : MonoBehaviour
{
  [SerializeField] float speed = 100f;
  [SerializeField] int loopPos = 2144;
  [SerializeField] Transform[] movingObjs;

  void Update() {
    foreach (Transform trans in movingObjs) {
      trans.Translate(transform.forward * speed * Time.deltaTime);
      if(trans.position.z >= loopPos) {
        trans.position = new Vector3(trans.position.x, trans.position.y, -loopPos);
      }
    }
  }
}
