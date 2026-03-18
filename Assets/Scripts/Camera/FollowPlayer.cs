using System;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject ThePlayer;
    public Vector2 MaxCameraOffset;
    public float CameraMovementSpeed;
    public float CameraSizeSpeed;
    private float OGCameraSize;
    public float NewCameraSize;
    private float OldCameraSize;


    private Camera player_camera;

    void Start()
    {
        ThePlayer = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        player_camera = GetComponent<Camera>();
        OGCameraSize = player_camera.orthographicSize;
        OldCameraSize = OGCameraSize;
        //Debug.Log("OGCameraSize = "+OGCameraSize);
    }

    void Update()
    {
        Vector3 cur_camera_pos = transform.position;
        Vector3 player_pos = ThePlayer.transform.position;
        Vector2 cur_distance = player_pos - cur_camera_pos;
        Vector2 calc_max_speed;

        // Smoothing function
        calc_max_speed.x = CameraMovementSpeed * (float)Math.Sqrt(Math.Abs(cur_distance.x/MaxCameraOffset.x));
        calc_max_speed.y = CameraMovementSpeed * (float)Math.Sqrt(Math.Abs(cur_distance.y/MaxCameraOffset.y));

        // Cap max speed
        if (calc_max_speed.x > CameraMovementSpeed) { calc_max_speed.x = CameraMovementSpeed; }
        if (calc_max_speed.y > CameraMovementSpeed) { calc_max_speed.y = CameraMovementSpeed; }

        // New camera velocity.
        rb.linearVelocity = cur_distance*calc_max_speed;


        if (OldCameraSize != NewCameraSize)
        {
            float new_camera_size;

            // Smoothing function
            if (OldCameraSize > NewCameraSize) { new_camera_size = CameraSizeSpeed * (float)Math.Sqrt(Math.Abs(NewCameraSize / OldCameraSize)); }
            else                               { new_camera_size = CameraSizeSpeed * (float)Math.Sqrt(Math.Abs(OldCameraSize / NewCameraSize)); }

            // Cap max speed
            if (new_camera_size > CameraSizeSpeed) { new_camera_size = CameraSizeSpeed; }

            // Sets negative direction.
            if (OldCameraSize > NewCameraSize) { new_camera_size *= -1; }

            // New camera size.
            player_camera.orthographicSize += new_camera_size;

            // If camera overshoots.
            if ((OldCameraSize > NewCameraSize && player_camera.orthographicSize < NewCameraSize) || (OldCameraSize < NewCameraSize && player_camera.orthographicSize > NewCameraSize))
            {
                player_camera.orthographicSize = NewCameraSize;
            }

        }

        if (player_camera.orthographicSize == NewCameraSize)
        {
            OldCameraSize = NewCameraSize;
        }
    }
}
