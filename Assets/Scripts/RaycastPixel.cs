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
using UnityEngine;
using GoogleARCore.HelloAR;

public class RaycastPixel : MonoBehaviour
{
	private Camera cam;
	private AudioSource audioSource;
	public GameObject portal;

	public int brushRadius;

	private bool canPlacePortal;
	private bool canChangeWorld;

	private WorldTracker worldTracker;

	private List<TrackedPlane> m_newPlanes = new List<TrackedPlane>();

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

		_QuitOnConnectionErrors();

        if (Frame.TrackingState != FrameTrackingState.Tracking)
        {
            return;
        }

		Frame.GetNewPlanes(ref m_newPlanes);

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
					TrackableHitFlag raycastFilter = TrackableHitFlag.PlaneWithinBounds |
                        TrackableHitFlag.PlaneWithinPolygon |
                        TrackableHitFlag.PointCloud;
                    TrackableHit hit;

					if (Session.Raycast(cam.ScreenPointToRay(Input.GetTouch (0).position), raycastFilter, out hit)) {
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
		var anchor = Session.CreateAnchor(h.Point, Quaternion.identity);
        var placedObject = Instantiate(portal, h.Point, Quaternion.identity, anchor.transform);

        // Did we intersect with a plane? If so, let's use its normal
        if (h.Plane != null) {
        	placedObject.GetComponent<PlaneAttachment>().Attach(h.Plane);
        	placedObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, h.Normal);
    	} else {
    	// If we don't have a real normal, we'll just point it to face the camera
    		placedObject.transform.LookAt(cam.transform);
    	}

    	placedObject.transform.Rotate(0, 180, 0);
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

	private void _QuitOnConnectionErrors() {
        // Do not update if ARCore is not tracking.
        if (Session.ConnectionState == SessionConnectionState.DeviceNotSupported)
        {
            _ShowAndroidToastMessage("This device does not support ARCore.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.UserRejectedNeededPermission)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            Application.Quit();
        }
        else if (Session.ConnectionState == SessionConnectionState.ConnectToServiceFailed)
        {
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
            Application.Quit();
        }
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