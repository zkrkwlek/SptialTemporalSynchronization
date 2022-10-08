using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class TouchManager : MonoBehaviour
{
    public ARRaycastManager mRaycastManager;
    //public ParticleSystem mParticleSystem;
    public Text mText;
    // Start is called before the first frame update
    void Awake()
    {
        //mParticleSystem = GetComponent<ParticleSystem>();
        //var renderer = mParticleSystem.GetComponent<Renderer>();
        //if (renderer != null)
        //    renderer.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        try {
            bool bTouch = false;
            Vector2 touchPos = Vector2.zero;
            int phase = -1; //0 == begin, 1 = move, 2 = ended

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                //List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
                //if (mRaycastManager.Raycast(touch.position, aRRaycastHits) && aRRaycastHits.Count > 0)
                //{
                //    ARRaycastHit hit = aRRaycastHits[0];
                //    hit.
                //    //OnSelectObjectInteraction(hit.pose.position, hit);
                //}
                Ray raycast = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit raycastHit;
                if (Physics.Raycast(raycast, out raycastHit))
                {
                    var obj = raycastHit.collider.gameObject;
                    var pathManager = obj.GetComponent<PathManager>();
                    pathManager.Move();
                    //mText.text = obj.name+" "+ ;
                    
                    //Debug.Log("Something Hit " + raycastHit.collider.name);
                }
            }
                //#if (UNITY_IOS || UNITY_ANDROID)
                
            
//            if (touch.phase == TouchPhase.Began)
//            {
//                touchPos = touch.position;
//                bTouch = true;
//                phase = 0;
//            }
//            else if (touch.phase == TouchPhase.Moved)
//            {
//                touchPos = touch.position;
//                bTouch = true;
//                phase = 1;
//            }
//            else if (touch.phase == TouchPhase.Ended)
//            {
//                touchPos = touch.position;
//                bTouch = true;
//                phase = 2;
//            }
////#endif
//            if (phase == 0 || phase == 1)
//            {
//                var particles = new ParticleSystem.Particle[1];
//                particles[0].startColor = mParticleSystem.main.startColor.color; ;
//                particles[0].startSize = mParticleSystem.main.startSize.constant; ;
//                particles[0].position = Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 1f));
//                particles[0].remainingLifetime = 2f;
//                mParticleSystem.SetParticles(particles,1);
//                mText.text = touchPos.ToString() + " "+ particles[0].position.ToString() + " "+ phase;
//            }
            
        }catch(Exception e)
        {
            mText.text = e.ToString();
        }
        
    }
}
