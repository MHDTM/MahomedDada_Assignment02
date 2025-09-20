using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player1;
    public Transform player2;
    public float smoothTime = 0.2f;
    public float minZoom = 5f;
    public float maxZoom = 10f;
    public float zoomLimiter = 10f;

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // --- Position ---
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        Vector3 targetPosition = new Vector3(centerPoint.x, centerPoint.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // --- Zoom ---
        float distance = Vector3.Distance(player1.position, player2.position);

        // zoom OUT when players are far, zoom IN when close
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, distance / zoomLimiter);
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * 5f);
    }
}