using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{

    private float cam_size;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSizes();
    }


    public void SetCameraSize(float new_cam_size) { cam_size = new_cam_size; }


    private void UpdateSizes()
    {
        for (int i = 0; i < transform.childCount;i++)
        {
            transform.GetChild(i).gameObject.GetComponent<ParallaxPieceInfo>().SetScale(cam_size);
        }
    }
}
