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
using UnityEngine;

public class ShaderEffects : MonoBehaviour {

	private Renderer rend;
	private float offset;
	private GameObject canvas;
	private Renderer canvasRenderer;

	private Camera mainCam;

	public RenderTexture rt;
	private Camera myCam;


	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer> ();
		canvas = transform.Find ("Quad").gameObject;
		myCam = transform.Find ("Camera").GetComponent<Camera> ();

		mainCam = GameObject.Find ("First Person Camera").GetComponent<Camera> ();

		offset = Random.Range (0, 100);
		canvasRenderer = canvas.GetComponent<Renderer> ();


		rt = new RenderTexture(Screen.width, Screen.height, 1, RenderTextureFormat.ARGB32);
		rt.antiAliasing = 8;
		rt.depth = 32;
		rt.Create ();

		myCam.targetTexture = rt;
		rend.material.SetTexture ("_MainTex", rt);

	}
	
	// Update is called once per frame
	void Update () {
		// A little bit of optimization--if we're not looking at the portal, we don't need everything enabled
		if (CanSeeMe()) {
			rend.material.SetFloat ("_Threshold", Mathf.Sin (Time.time + offset));
			rend.material.SetTexture ("_MaskTex", canvasRenderer.material.mainTexture);
			rend.enabled = true;
			myCam.enabled = true;
		} else {
			rend.enabled = true;
			myCam.enabled = false;
		}
	}

	private bool CanSeeMe() {
		Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCam);
		if (GeometryUtility.TestPlanesAABB(planes, rend.bounds))
			return true;
		else
			return false;
	}
}
