using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MarkerTest2 : MonoBehaviour
{
    public ArucoMarkerDetector mMarkerDetector;
    public ARRaycastManager arRaycastManager;
    public GameObject AnchoredObjectPrefab;
    public Text mText;
    //public GameObject anchorObj;

    void OnEnable()
    {
        MarkerDetectEvent.markerDetected += OnMarkerInteraction;
    }
    void OnDisable()
    {
        MarkerDetectEvent.markerDetected -= OnMarkerInteraction;
    }
    void Awake()
    {
        
    }
    void OnMarkerInteraction(object sender, MarkerDetectEventArgs me)
    {
        try
        {
            //var ids = mMarkerDetector.mListIDs;
            //var markers = mMarkerDetector.mDictMarkers;

            //if (ids.Count > 0)
            //{
            //    int id = ids[0];
            //    var marker = mMarkerDetector.GetMarker(id);
            //    var position = marker.corners[0];
            //    var pos3D = marker.origin;
            //    List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            //    if (arRaycastManager.Raycast(position, aRRaycastHits) && aRRaycastHits.Count > 0)
            //    {
            //        ARRaycastHit hit = aRRaycastHits[0];
            //        //mText.text = position + " "+mMarkerDetector.corner3Ds[0].ToString();
            //        if (hit.trackable is ARPlane plane)
            //        {
            //            //localAnchor = anchorManager.AddAnchor(hits[0].pose);
            //            var anchorGameObject = Instantiate(AnchoredObjectPrefab, pos3D, hit.pose.rotation);
            //            //mText.text = "created " + pos3D;
            //        }
                    
            //    }
            //    mText.text = "marekr event " + id+" "+pos3D;
            //}
            //else
            //    mText.text = "adsfasdfasdfasdf";
            
            //mText.text = "marekr event";
        }
        catch (Exception e)
        {
            mText.text = e.ToString();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
