using UnityEngine;

public class ParallaxPieceInfo : MonoBehaviour
{
    public float ScaleWithCamera;
    public Vector2 ShiftSpeed;
    public Vector2 ConstantScrollSpeed;

    private Vector2 OGSize;

    void Start()
    {
        OGSize = transform.localScale;
    }


    public void SetScale(float new_float)
    {
        Vector2 new_scale = OGSize*new_float;

        transform.localScale = (new_scale * ScaleWithCamera) + OGSize * ( 1 - ScaleWithCamera );
    }


}
