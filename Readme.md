# Mixed Reality Racer (MRR)

Play a mix reality remote control car game with your Bridge headset. Hit all the checkpoints before you explode, avoid the tornado!

![gif](documentation/MRRDemo.gif)

### Prerequisites

```
Unity 2017.2 + 
Bridge Headset, Control, for iPhone 7 and above
Structural Sensor
```
## Technical notes

### Special Effects
Special effects are triggered when the car hit a wall, land on the floor from a jump, or just run out of time. The effects are prefabs in the `CarControllerInput` base class. The prefabs are stored in `Assets/Team_XR/Sean_VFX`

### Car
The car controller is a Unity standard vehicle asset. We modified the input method to fit for the Bridge headset remote control.

### Controller
We have three controller styles available. 

`ControllerInputA` -uses DPAD for steering. Acceleration and braking is based on where you are pressing on DPAD relative to the center, hold the secondary buttons to reset

`ControllerInputB` -uses the 6 Axis rotation for steering, primary button for acceleration, secondary button for braking, hold both primary + secondary buttons to reset

`ControllerInputC` - uses the DPAD for steering, primary button for acceleration, and secondary button for braking. hold both primary + secondary buttons to reset

The default is ControllerInputA, to switch controllers, remove controllerInputX from the car, and swap in the desired controller, the controllers are stored in `Assets/Team_XR/Car/Scripts`

![controller_input_location](documentation/controllerLocation.png)


## Known Issues
Wall collision special effects are spawned multiple times per collision. We discovered it was a multi-thread issue involving the `collisionDetectionCall`. We will need to change the `isReadyEffect` boolean to a multi-thread friendly variable.


## Authors

* Samanthe Yueh - Scrum Master, UI/UX Designer

* Seán Conroy - Creative Lead

* Nick Kubash - Art Style Lead, 3D Artist

* Viet Phan - Programming Lead

* Andy Yao - Programmer

* Thalita Karina -  3D Motion Designer

* Manni Zhang - 3D Artist

## Acknowledgments
* Aaron Hilton at https://steampunk.digital/
* Jacob Ervin