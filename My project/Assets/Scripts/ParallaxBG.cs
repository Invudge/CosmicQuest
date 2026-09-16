using UnityEngine;

public class ParallaxBG : MonoBehaviour
{
    [Header("Настройки")]
    [Range(0f, 1f)] public float parallaxFactor = 0.3f; // 0 = стоит на месте, 1 = движется с камерой
    public Camera targetCamera;

    private Vector3 startPos;
    private float startZ;

    void Start()
    {
        startPos = transform.position;
        startZ = startPos.z;
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        Vector3 camPos = targetCamera.transform.position;

        transform.position = new Vector3(
            startPos.x + camPos.x * parallaxFactor,
            startPos.y + camPos.y * parallaxFactor,
            startZ
        );
    }
}