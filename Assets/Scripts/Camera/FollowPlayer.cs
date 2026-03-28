using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject ThePlayer;
    public Vector2 MaxCameraOffset;
    public float CameraMovementSpeed;
    public float CameraSizeSpeed;
    public Vector2 CameraBoundaryMax;
    public Vector2 CameraBoundaryMin;
    public List<float> ZoneSizes;
    private float OGCameraSize;
    private float NewCameraSize;
    private float OldCameraSize;

    private bool CutsceneOverride = false;
    private Vector2 CutscenePos;

    private Camera player_camera;
    private ParallaxBackground parallaxBackground;

    void Start()
    {
        ThePlayer = GameObject.FindWithTag("Player");
        parallaxBackground = GetComponentInChildren<ParallaxBackground>();
        rb = GetComponent<Rigidbody2D>();
        player_camera = GetComponent<Camera>();
        OGCameraSize = player_camera.orthographicSize;
        NewCameraSize = OGCameraSize;
        OldCameraSize = OGCameraSize;

    }



    public void UpdateCutscenePos(Vector2 camera_pos) { CutscenePos = camera_pos; }



    void Update()
    {
        Vector3 camera_pos;

        if (CutsceneOverride) { camera_pos = CutscenePos; }
        else { camera_pos = ThePlayer.transform.position;}
        
        UpdateCameraPos(camera_pos);
        UpdateCameraSize();
        parallaxBackground.SetCameraSize(player_camera.orthographicSize/OGCameraSize);
    }



    public void SetZone(int new_zone)
    {
        if (new_zone < 0 || new_zone >= ZoneSizes.Count)
        {
            NewCameraSize = OGCameraSize;
        }
        else
        {
            OldCameraSize = player_camera.orthographicSize;
            NewCameraSize = ZoneSizes[new_zone];
        }
    }







    private void UpdateCameraPos(Vector3 player_pos)
    {
        Vector3 cur_camera_pos = transform.position;
        Vector2 cur_distance = player_pos - cur_camera_pos;
        Vector2 calc_max_speed;

        // Smoothing function
        calc_max_speed.x = CameraMovementSpeed * (float)Math.Sqrt(Math.Abs(cur_distance.x/MaxCameraOffset.x));
        calc_max_speed.y = CameraMovementSpeed * (float)Math.Sqrt(Math.Abs(cur_distance.y/MaxCameraOffset.y));

        // Cap max speed
        if (calc_max_speed.x > CameraMovementSpeed) { calc_max_speed.x = CameraMovementSpeed; }
        if (calc_max_speed.y > CameraMovementSpeed) { calc_max_speed.y = CameraMovementSpeed; }

        // New camera velocity.
        if (ValidateCameraBoundary()) { rb.linearVelocity = cur_distance*calc_max_speed; }
        else { rb.linearVelocity = new Vector2(0,0); }
    }


    private bool ValidateCameraBoundary()
    {
        if (transform.position.x > CameraBoundaryMax.x && CameraBoundaryMax.x != 0)
        { 
            transform.position = new Vector3( CameraBoundaryMax.x, transform.position.y, transform.position.z);
            return false;
        }
        if (transform.position.x < CameraBoundaryMin.x && CameraBoundaryMax.x != 0)
        {
            transform.position = new Vector3( CameraBoundaryMin.x, transform.position.y, transform.position.z);
            return false;
        }
        if (transform.position.y > CameraBoundaryMax.y && CameraBoundaryMax.x != 0)
        {
            transform.position = new Vector3( transform.position.x, CameraBoundaryMax.y, transform.position.z);
            return false;
        }
        if (transform.position.y < CameraBoundaryMin.y && CameraBoundaryMax.x != 0)
        {
            transform.position = new Vector3( transform.position.x, CameraBoundaryMin.y, transform.position.z);
            return false;
        }
        return true;
    }





    private void UpdateCameraSize()
    {
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
