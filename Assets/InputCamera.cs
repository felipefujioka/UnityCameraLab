using DG.Tweening;
using TouchScript.Gestures.TransformGestures;
using UnityEngine;

[RequireComponent(typeof(ScreenTransformGesture))]
public class InputCamera : MonoBehaviour
{
    private int boundariesLayer;
    
    private const float MAX_RAY_DISTANCE = 100;
    
    public Vector2 zoomLimits;
    public float panSpeed;
    public float rotationDuration;
    public bool invertedCamera;
    
    private ScreenTransformGesture inputSource;
    private Camera cam;
    private Transform pivot;
    private bool rotating;
    
    // Use this for initialization
    void Start()
    {
        boundariesLayer = 1 << LayerMask.NameToLayer("Boundaries");
        cam = Camera.main;
        inputSource = GetComponent<ScreenTransformGesture>();
        pivot = cam.transform.parent;
        
        inputSource.Transformed += CameraTransform;
    }

    private void OnDestroy()
    {
        inputSource.Transformed -= CameraTransform;
    }

    public void RotateCamera()
    {
        if (rotating)
        {
            return;
        }

        var signedAngle = 90;
        
        var sign = Mathf.Sign(signedAngle);
        var rotationModule = Mathf.Abs(signedAngle);
        var targetRotation = Quaternion.AngleAxis(rotationModule, Vector3.up);

        float delay = rotationDuration > 0f ? rotationDuration : 0f;
        Tween tween = pivot.DORotate(sign * targetRotation.eulerAngles, delay, RotateMode.WorldAxisAdd);
        tween.onComplete += () =>
        {
            rotating = false;
        };

        tween.Play();

        rotating = true;
    }


    private void CameraTransform(object sender, System.EventArgs e) 
    {
        var speed = panSpeed * (cam.orthographicSize / zoomLimits.y) * (invertedCamera ? 1 : -1);

        var deltaPos = cam.transform.rotation * inputSource.DeltaPosition * speed;

        var nextPosition = pivot.localPosition + deltaPos;

        if (CanSeesBoundaries(cam.transform.position + deltaPos))
        {
            Debug.Log("Can see boundaries");
            pivot.localPosition = nextPosition;
        }

        
    }

    private bool CanSeesBoundaries(Vector3 newPosition)
    {
        Debug.DrawRay(newPosition, MAX_RAY_DISTANCE * cam.transform.forward, Color.green, 0.5f);
        return Physics.Raycast(newPosition, cam.transform.forward, MAX_RAY_DISTANCE, boundariesLayer);
    }
}