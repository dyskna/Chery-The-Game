using UnityEngine;

public class FadeController : MonoBehaviour
{
    public Transform lightSource; // np. gracz albo jego latarka
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (rend.material.HasProperty("_LightPos"))
        {
            rend.material.SetVector("_LightPos", lightSource.position);
        }
    }
}
