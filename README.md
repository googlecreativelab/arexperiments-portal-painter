# Portal Painter

Doodle new worlds onto your own, with [Google ARCore](https://developers.google.com/ar).

![Portal Painter](portal-painter.gif)

## Technology:

[Portal Painter](http://www.experiments.with.google.com/ar/portal-painter) is built in Unity with [Google ARCore](https://developers.google.com/ar). Portal Painter uses [ARCore’s motion tracking and environmental understanding](https://developers.google.com/ar/discover/concepts) to let users paint, wander around, and peer into the portals they create.

When the user starts the app, they see particles appear on parts of the environment that they can interact with. When the user touches the screen, we use ARCore to detect whether their touch intersects with any detected real-world features. If we have a hit, ARCore returns data on what intersected with our raycast. If we intersected with a flat (i.e. horizontal) plane, we instantiate a portal at the correct location and orient it to match our plane's normal. If we intersected with a point in the point cloud (e.g. vertical features, which don't have planes or normals), we instantiate a portal at that location, but rotated to face the camera instead.

A portal contains a child canvas (invisible) and a child camera (spawned in one of the fantasy worlds). As the user continues to draw their portal, we raycast their touch onto the invisible canvas to figure out what pixels should become visible, while rendering the child camera feed to a texture. We then use a shader to combine these two textures into one, which we put on the portal itself, revealing the other worlds.

This is not an official Google product.

## Develop:

After [setting up and getting ARCore](https://developers.google.com/ar/develop/unity/getting-started), you will want to do the following:

* **Make your worlds.** Each world should be its own GameObject, and should have a child called `Center` located at whatever point you’d like the portal-camera to be. Whenever you spawn a portal, the `UIManager` will look for this `Center` object and place its camera there. (See the default `DemoWorld` object for an example.)

* **Add them to the UIManager GameObject.** They will be accessed in the `WorldTracker` script. You may add a custom skybox for a particular world there as well (make sure their indexes match).

* **Build and run.** Point your camera at your environment and wait until you see faint particles rise up from it. You can then use your finger to draw.

## Debug commands:

* 2-finger tap to start painting with the next world (won’t change old portals)
* 3-finger tap to remove all portals on-screen

## Acknowledgements:

Built by [Jane Friedhoff](http://www.janefriedhoff.com) with friends on the Creative Lab team at Google. Sample model created by [Alex Jacobo-Blonder](http://ajacoboblonder.com/). Check out more at [AR Experiments](https://experiments.withgoogle.com/ar/) or read more about [ARCore](https://developers.google.com/ar).  
