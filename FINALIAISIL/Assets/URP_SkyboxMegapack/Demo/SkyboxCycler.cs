using UnityEngine;

public class SkyboxCycler : MonoBehaviour
{
    public Material[] skyboxes;
    public float rotationSpeed = 2f;
    int index = 0;

    void Start()
    {
        Apply(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Apply(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Apply(+1);

        if (RenderSettings.skybox != null)
        {
            float rot = RenderSettings.skybox.GetFloat("_Rotation");
            RenderSettings.skybox.SetFloat("_Rotation", rot + rotationSpeed * Time.deltaTime);
        }
    }

    void Apply(int delta)
    {
        if (skyboxes == null || skyboxes.Length == 0) return;
        index = (index + delta + skyboxes.Length) % skyboxes.Length;
        RenderSettings.skybox = skyboxes[index];
        DynamicGI.UpdateEnvironment();
    }
}
