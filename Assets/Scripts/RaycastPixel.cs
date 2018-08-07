/*
Copyright 2017 Google Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

public class RaycastPixel : MonoBehaviour
{
	private Camera cam;
	private AudioSource audioSource;
	public GameObject portal;

	public int brushRadius;

	private bool canPlacePortal;
	private bool canChangeWorld;

	private WorldTracker worldTracker;

	private bool m_IsQuitting = false;

	private List<DetectedPlane> m_newPlanes = new List<DetectedPlane>();

	void Start()
	{
		cam = GameObject.Find("First Person Camera").GetComponent<Camera>();
		worldTracker = gameObject.GetComponent<WorldTracker> ();
		
        const int SLEEP_TIMEOUT = 15;
        Screen.sleepTimeout = SLEEP_TIMEOUT;
		
		canPlacePortal = true;
		canChangeWorld = true;
	}

	void Update() {

		_UpdateApplicationLifecycle();

        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

		Session.GetTrackables<DetectedPlane>(m_newPlanes, TrackableQueryFilter.New);

		int layerMask = 1 << 8;
		layerMask = ~layerMask;

		// One touch = paint a portal
		if (Input.touchCount == 1) {
			RaycastHit h;
			// First: does our touch intersect with an existing portal? If so, draw on that one instead of making a new one
			if (Physics.Raycast (cam.ScreenPointToRay (Input.GetTouch (0).position), out h, Mathf.Infinity, layerMask)) {
				UpdatePixels (h);
			} else {
			// If not, then we just make a new one
				if (canPlacePortal) {
					TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
                    TrackableHit hit;
					Vector2 touchPoint = Input.GetTouch(0).position;
					if (Frame.Raycast(touchPoint.x, touchPoint.y, raycastFilter, out hit)) {
						PlacePortal (hit);
						canPlacePortal = false;
					}
				}
			}

		// Two touches = change which world is being painted
		} else if (Input.touchCount == 2) {
			if (canChangeWorld) {
				int curWorld = worldTracker.curWorld + 1;
				curWorld = curWorld % worldTracker.worlds.Length; 
				worldTracker.curWorld = curWorld;
				canChangeWorld = false;
			}

		// Three touches = delete all portals
		} else if (Input.touchCount == 3) {
			GameObject[] gameobjects = GameObject.FindGameObjectsWithTag("Portal");
			foreach (GameObject g in gameobjects) {
				Destroy(g);
			}
		}

		// No touches? Let the user switch worlds/clear/paint more portals
		else {
			canPlacePortal = true;
			canChangeWorld = true;
		}
	}

	void PlacePortal(TrackableHit h) {
    	var anchor = h.Trackable.CreateAnchor(h.Pose);
        var placedObject = Instantiate(portal, h.Pose.position, h.Pose.rotation);
    	
    	placedObject.transform.Rotate(90, 0, 0, Space.Self);
    	placedObject.transform.parent = anchor.transform;
	}

	void UpdatePixels(RaycastHit hit) {
		Renderer rend = hit.transform.GetComponent<Renderer> ();
		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
			return;

		Texture2D tex = rend.material.mainTexture as Texture2D;
		Vector2 pixelUV = hit.textureCoord;
		pixelUV.x *= tex.width;
		pixelUV.y *= tex.height;

		for (int y = -brushRadius; y <= brushRadius; y++) {
			for (int x = -brushRadius; x <= brushRadius; x++) {
				if (x * x + y * y <= brushRadius * brushRadius) {

					Vector2 center = new Vector2 (pixelUV.x, pixelUV.y);
					Vector2 pt = new Vector2 (pixelUV.x + x, pixelUV.y + y);
					float dist = Vector2.Distance (center, pt);

					Color newColor;

					newColor = Color.Lerp (Color.black, Color.white, 1 - (dist / brushRadius));
					Color curColor = tex.GetPixel((int)pixelUV.x + x, (int)pixelUV.y + y);
					curColor -= newColor;

					tex.SetPixel((int)pixelUV.x + x, (int)pixelUV.y + y, curColor);
				}

			}
		}
			
		tex.Apply ();
	}

	private void _UpdateApplicationLifecycle() {
        // Exit the app when the 'back' button is pressed.
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Only allow the screen to sleep when not tracking.
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    private void _DoQuit() {
        Application.Quit();
    }

    private static void _ShowAndroidToastMessage(string message) {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                    message, 0);
                toastObject.Call("show");
            }));
        }
    }
}