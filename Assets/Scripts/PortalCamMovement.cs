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

public class PortalCamMovement : MonoBehaviour {

	public GameObject tangoCam;
	public Camera myCam;

	public WorldTracker worldTracker;

	public GameObject worldModel;

	private Vector3 myStartPos;

	// Use this for initialization
	void Start () {
		tangoCam = GameObject.Find ("First Person Camera");
		myCam = gameObject.GetComponent<Camera> ();
		worldTracker = GameObject.Find ("UIManager").GetComponent<WorldTracker> ();

		worldModel = worldTracker.worlds [worldTracker.curWorld];

		// Optional ability to give each world a custom skybox
		if (worldTracker.skyboxes [worldTracker.curWorld] != null) {
			gameObject.GetComponent<Skybox> ().material = worldTracker.skyboxes [worldTracker.curWorld];
		}

		myStartPos = worldModel.transform.Find("Center").transform.position;

		transform.position = myStartPos;

	}

	// Update is called once per frame
	void Update () {
		transform.position = myStartPos;
	}
}
