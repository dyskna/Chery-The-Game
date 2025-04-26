using UnityEngine;
using UnityEngine.Rendering.Universal;

public class YourScript : MonoBehaviour
{
    [SerializeField] public Transform playerTransform;
    private Light2D playerLight;

    void Awake()
    {
        playerLight = playerTransform.GetComponent<Light2D>();
    }

    void Update()
    {
        playerLight.enabled = playerTransform.position.y <= transform.position.y;
    }
}
