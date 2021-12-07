using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARManager : MonoBehaviour
{
    [SerializeField]
    GameObject MapPrefab;

    GameObject map;

    private ARPlaneManager _planeManager;
    private ARRaycastManager _raycastManager;
    private ARAnchorManager _anchorManager;
    private bool mapInstantiated = false;

    private void Awake()
    {
        _planeManager = this.GetComponent<ARPlaneManager>();
        _raycastManager = this.GetComponent<ARRaycastManager>();
        _anchorManager = this.GetComponent<ARAnchorManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0 && !mapInstantiated)
        {
            List<ARRaycastHit> hit = new List<ARRaycastHit>();
            if (_raycastManager.Raycast(Input.GetTouch(0).position, hit))
            {
                ARPlane plane = (ARPlane)hit[0].trackable;
                ARAnchor anchor = _anchorManager.AttachAnchor(plane, hit[0].pose);
                map = GameObject.Instantiate(MapPrefab, anchor.transform);
                mapInstantiated = true;

                _planeManager.requestedDetectionMode = UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;
                _planeManager.SetTrackablesActive(false);
            }
        }
    }
}
