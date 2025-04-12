using UnityEngine;

public class Tail : MonoBehaviour
{
    public Transform networkedOwner;
    public Transform followTransform;

    [SerializeField] private float delayTime = 0.01f;
    [SerializeField] private float distance = 0.3f;
    [SerializeField] private float moveStep = 10f;

    private Vector3 _targetPosition;

    private void Update()
    {
        _targetPosition = followTransform.position - followTransform.forward * distance;
        _targetPosition += (transform.position - _targetPosition) * delayTime;
        _targetPosition.y = 0f;

        transform.position = Vector3.Lerp(a: transform.position, b: _targetPosition, t: Time.deltaTime * moveStep);
    }
}
