using UnityEngine;
using System.Collections;

public class StickyGameObject : MonoBehaviour {

    public Quaternion rotationOffSet;
    private Transform parentTransform;

    void Start() {
        parentTransform = GetComponentInParent<Transform>();
    }

	void FixedUpdate()
    {
        if (parentTransform != null) {
            transform.rotation = parentTransform.rotation;
            //transform.localRotation = parentTransform.localRotation;
        }
	
	}
}
