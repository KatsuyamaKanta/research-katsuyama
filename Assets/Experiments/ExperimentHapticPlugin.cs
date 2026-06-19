// This code contains 3D SYSTEMS Confidential Information and is disclosed to you
// under a form of 3D SYSTEMS software license agreement provided separately to you.
//
// Notice
// 3D SYSTEMS and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from 3D SYSTEMS is strictly prohibited.
//
// ALL 3D SYSTEMS DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". 3D SYSTEMS MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, 3D SYSTEMS assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of 3D SYSTEMS. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// 3D SYSTEMS products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// 3D SYSTEMS.
//
// Copyright (c) 2020 3D SYSTEMS. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Events;
using System;
using HapticGUI;
//using StackableDecorator;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExperimentHapticPlugin : MonoBehaviour
{
    #region DLL_Imports
    [DllImport("HapticsDirect")] public static extern void getVersionString(StringBuilder dest, int len);  //!< Retreives the OpenHaptics version string.

    // Setup Functions
    [DllImport("HapticsDirect")] public static extern int initDevice(string deviceName);  //!< Connects to and Initializes a haptic device.
    [DllImport("HapticsDirect")] public static extern void getDeviceSN(string configName, StringBuilder dest, int len);   //!< Retrieves device serial number
    [DllImport("HapticsDirect")] public static extern void getDeviceModel(string configName, StringBuilder dest, int len);	//!< Retrieves devices model name

    [DllImport("HapticsDirect")] public static extern void getDeviceMaxValues(string configName, ref double max_stiffness, ref double max_damping, ref double max_force);
    [DllImport("HapticsDirect")] public static extern void startSchedulers(); //!< Starts the Open Haptic schedulers and assigns the required internal callbacks

    // Device Information
    [DllImport("HapticsDirect")] public static extern void getWorkspaceArea(string configName, double[] usable6, double[] max6); //!< Retrieves the bounds created by the physical limitations of the device. 

    // Updates
    [DllImport("HapticsDirect")] public static extern void getPosition(string configName, double[] position3); //!< Get the current position in mm of the device facing the device base. Left is + x, up is +y, toward user is +z. (Unity CSys)
    [DllImport("HapticsDirect")] public static extern void getVelocity(string configName, double[] velocity3); //!< Get the current velocity in mm/s of the device. Note: This value is smoothed to reduce high frequency jitter. (Unity CSys)
    [DllImport("HapticsDirect")] public static extern void getTransform(string configName, double[] matrix16); //!< Get the column-major transform of the device endeffector. (Unity CSys)
    [DllImport("HapticsDirect")] public static extern void getButtons(string configName, int[] buttons4, int[] last_buttons4, ref int inkwell); //!< Get the button, last button states and get whether the inkwell switch, if one exists is active.
    [DllImport("HapticsDirect")] public static extern void getCurrentForce(string configName, double[] currentforce3);  //!< Get the current force in N of the device. (Unity CSys)

    [DllImport("HapticsDirect")] public static extern void getJointAngles(string configName, double[] jointAngles, double[] gimbalAngles); //!< Get the joint angles in rad of the device. These are joint angles used for computing the kinematics of the armature relative to the base frame of the device. For Touch devices: Turret Left +, Thigh Up +, Shin Up + Get the angles in rad of the device gimbal.For Touch devices: From Neutral position Right is +, Up is -, CW is +

    [DllImport("HapticsDirect")] public static extern void getCurrentFrictionForce(string configName, double[] frictionForce);

    [DllImport("HapticsDirect")] public static extern void getGlobalForces(string configName, double[] vibrationForce, double[] constantForce, double[] springForce);

    [DllImport("HapticsDirect")] public static extern void getLocalForces(string configName, double[] stiffnessForce, double[] viscosityForce, double[] dynamicFrictionForce, double[] staticFrictionForce, double[] constantForce, double[] springForce);

    // Force output
    [DllImport("HapticsDirect")] public static extern void setForce(string configName, double[] lateral3, double[] torque3); //!< Adds an additional force to the haptic device. Can be eseed for scripted forces, but in most cases using an Effect is preferable. 

    [DllImport("HapticsDirect")] public static extern void setAnchorPosition(string configName, double[] position3); //!< Set the anchor position of the virtual stylus (Unity CSys)

    [DllImport("HapticsDirect")]
    public static extern void addContactPointInfo(string configName, double[] Location, double[] Normal, float MatStiffness, float MatDamping, double[] MatForce,
    float MatViscosity, float MatFrictionStatic, float MatFrictionDynamic, double[] MatConstForceDir, float MatConstForceMag, double[] MatSpringDir, float MatSpringMag, float MatPopThroughRel, float MatPopThroughAbs,
    double MatMass, double RigBSpeed, double[] RigBVelocity, double[] RigBAngularVelocity, double RigBMass, double[] ColImpulse, double PhxDeltaTime, double ImpulseDepth); //!< Add a collision contact point info to the contact points list
    [DllImport("HapticsDirect")] public static extern void updateContactPointInfo(string configName); //!< Update the contact point info list
    [DllImport("HapticsDirect")] public static extern void resetContactPointInfo(string configName); //!< Reset the contact point info list

    [DllImport("HapticsDirect")] public static extern void setVibrationValues(string configName, double[] direction3, double magnitude, double frequency, double time); //!< Set the parameters of the vibration
    [DllImport("HapticsDirect")] public static extern void setSpringValues(string configName, double[] anchor, double magnitude); //!< Set the parameters of the Spring FX
    [DllImport("HapticsDirect")] public static extern void setConstantForceValues(string configName, double[] direction, double magnitude); //!<Set the parameters of the Constant Force FX

    [DllImport("HapticsDirect")] public static extern void setGravityForce(string configName, double[] gForce3);


    //Cleanup functions
    //! Disconnects from all devices.

    [DllImport("HapticsDirect")] public static extern void disconnectAllDevices();

    //Error Handling Functions
    [DllImport("HapticsDirect")] public static extern int getHDError(StringBuilder Info, int len);


    #endregion

    #region Inspector
    // Plugin Menu 

    private bool enablef = false;



    [Header("Device Setup")]
    [Tooltip("Enter the device name e.g. Default Device or Right Device or Left Device")]
    [Label(title = "Device Identifier")]
    [EnableIf("$enablef", disable = false)]
    [StackableField]
    public string DeviceIdentifier = "Default Device";
    [Label(title = "Connect on Start")]
    [Tooltip("The default status is checked and causes the haptic device to be initialized in game mode and the servo loop to be started.")]
    public bool ConnectOnStart = true;
    [Label(title = "Scale to Meter")]
    [Tooltip("Activates the scaling of the workspace.")]
    [StackableField]
    public bool ScaleToMeter = true;
    [ShowIf("$ScaleToMeter", order = 1)]
    [Label(title = "Scale Factor")]
    [Tooltip("Defines by which factor the workspace is scaled. Default value 0.001. (mm->m)")]
    [StackableField]
    public float GlobalScale = 0.001f;

    [Header("On Screen Stylus")]
    [Label(title = "Visual Mesh")]
    [Tooltip("The Visual Mesh represents the on screen stylus. It can be any mesh.")]
    [StackableField]
    public GameObject VisualizationMesh;
    [Label(title = "Collision Mesh")]
    [Tooltip("The Collision Mesh has the function of a collider of the on screen stylus. It consists of a (convex) mesh, one or more convex colliders, a rigid body and the HapticCollider.cs script.")]
    [StackableField]
    public GameObject CollisionMesh;
    // public GameObject OriCollisionMesh;

    [HideInInspector]
    public GameObject GrabObject;
    private Transform GrabTransform;
    private Vector3 GrabAnchor = new Vector3(0.0f, 0.0f, 0.0f);
    private FixedJoint joint = null;

    [Foldout(title = "Device Information", order = 2)]
    [Group("DeviceInfo", 11)]
    [Label(title = "Model Type")]
    [Tooltip("Model Type displays the device model type.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public string ModelType = "Not Connected";
    [InGroup("DeviceInfo")]
    [Label(title = "Serial Number")]
    [Tooltip("Serial Number displays the device serial number.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public string SerialNumber = "Not Connected";
    [InGroup("DeviceInfo")]
    [Label(title = "HHD")]
    [Tooltip("HHD displays the device handle of an initialized device.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public int DeviceHHD = -1;
    [InGroup("DeviceInfo")]
    [Label(title = "Force Max")]
    [Tooltip("Force Max displays the nominal maximum force, i.e. the amount of force that the device can sustain when the motors are at room temperature (optimal).")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public double MaxForce;
    [InGroup("DeviceInfo")]
    [Label(title = "Stiffness Max")]
    [Tooltip("Stiffness Max displays the maximum closed loop stiffness that is recommended for the device.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public double MaxStiffness;
    [InGroup("DeviceInfo")]
    [Label(title = "Damping Max")]
    [Tooltip("Damping Max displays the maximum level of damping that is recommended for the device.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public double MaxDamping;
    [InGroup("DeviceInfo")]
    [Label(title = "Current Position")]
    [Tooltip("Current Position displays the current position of the device facing the device base.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public Vector3 CurrentPosition;
    [InGroup("DeviceInfo")]
    [Label(title = "Current Velocity")]
    [Tooltip("Current Velocity displays the current velocity of the device. Note: This value is smoothed to reduce high frequency jitter.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public Vector3 CurrentVelocity;
    [InGroup("DeviceInfo")]
    [Label(title = "Current Force")]
    [Tooltip("Current Force shows the current force as Cartesian coordinated vector.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public Vector3 CurrentForce;
    [InGroup("DeviceInfo")]
    [Label(title = "Force Magnitude")]
    [Tooltip("Force Magnitude displays the magnitude of the current force vector.")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public float MagForce;
    [InGroup("DeviceInfo")]
    [Label(title = "Joint Angles")]
    [Tooltip("Joint Angles displays the joint angles of the device. These are joint angles used for computing the kinematics of the armature relative to the base frame of the device. For Touch devices: Turret Left +, Thigh Up +, Shin Up + .")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public Vector3 JointAngles;
    [InGroup("DeviceInfo")]
    [Label(title = "Gimbal Angles")]
    [Tooltip("Gimbal Angles Get the angles of the device gimbal. For Touch devices: From Neutral position Right is +, Up is -, CW is + .")]
    [EnableIf("$enablef", disable = true)]
    [StackableField]
    public Vector3 GimbalAngles;


    [Label(title = "Use Simple Buttons Setup")]
    [Tooltip("The Simple Button Setup provides a simple way to define the buttons of the haptic device for grabbing and releasing objects. ")]
    [StackableField]
    public bool SimpleButtons = false;
    [Group("SimpleButtons", 7)]
    [ShowIf("$SimpleButtons", order = 8)]
    [LabelOnly]
    [Label(title = "Button 1 - Mode")]
    [StackableField]
    public string labelOnly;
    [InGroup("SimpleButtons")]
    [Label(title = "Grab")]
    [StackableField]
    public bool bButton1g;
    [InGroup("SimpleButtons")]
    [Label(title = "Release")]
    [StackableField]
    public bool bButton1r;
    [InGroup("SimpleButtons")]
    [Label(title = "Hold")]
    [StackableField]
    public bool bButton1h;
    [InGroup("SimpleButtons")]
    [LabelOnly]
    [Label(title = "Button 2 - Mode")]
    public string labelOnly2;
    [InGroup("SimpleButtons")]
    [Label(title = "Grab")]
    [StackableField]
    public bool bButton2g;
    [InGroup("SimpleButtons")]
    [Label(title = "Release")]
    [StackableField]
    public bool bButton2r;
    [InGroup("SimpleButtons")]
    [Label(title = "Hold")]
    [StackableField]
    public bool bButton2h;

    public HapticEvents Events;


    public enum BoxType { usableWorkspace, maxWorkspace };
    public enum Axis { X = 1, Y = 2, Z = 4 }

    [Foldout(title = "Camera and Navigation", order = 2)]
    [Group("G1", 41)]
    [Label(title = "Master Camera")]
    [Tooltip("Master Camera defines the camera that is used to focus on the Haptic Device Workspace.")]
    [StackableField]
    public Camera masterCamera;
    [InGroup("G1")]
    [Label(title = "Camera Distance")]
    [Tooltip("Camera Distance sets the distance to the workspace.")]
    [Slider(0, 5)]
    public float CameraDistance = 1.0f;
    [InGroup("G1")]
    [Label(title = "Fit To Workspace")]
    [Tooltip("Fit To Workspace defines to which workspace (Usable / Max) the camera should focus.")]
    [StackableField]
    public BoxType FitToWorkspace = BoxType.usableWorkspace;
    [InGroup("G1")]
    [Label(title = "Transform Anchor")]
    [Tooltip("Transform Anchor allows to transform the workspace around an anchor point. (optional)")]
    [StackableField]
    public GameObject TransformAnchor;
    [InGroup("G1")]
    [Label(title = "Limit Scene Navigation")]
    [Tooltip("By enabling this option, the transformation range of the workspace can be limited via Min/Max Position and Min/Max Rotation (Angle). The valid transformation range is displayed as a red box (gizmo).")]
    [StackableField]
    public bool SceneLimit;
    [ShowIf("$SceneLimit")]
    [InGroup("G1")]
    [Label(title = "Min Postion")]
    [StackableField]
    public Vector3 Min_TNav;
    [ShowIf("$SceneLimit")]
    [InGroup("G1")]
    [Label(title = "Max Postion")]
    [StackableField]
    public Vector3 Max_TNav;
    [ShowIf("$SceneLimit")]
    [InGroup("G1")]
    [Label(title = "Min Rotation")]
    [StackableField]
    public Vector3 Min_RNav;
    [ShowIf("$SceneLimit")]
    [InGroup("G1")]
    [Label(title = "Max Rotation")]
    [StackableField]
    public Vector3 Max_RNav;
    [InGroup("G1")]
    [Label(title = "Freeze Translation")]
    [Tooltip("The X,Y,Z,All checkboxes can be used to block the translation to one or more axes.")]
    [EnumMaskButton(styles = "toggle")]
    public Axis FreezeTranslation;
    [InGroup("G1")]
    [Label(title = "Freeze Rotation")]
    [Tooltip("The X,Y,Z,All checkboxes can be used to block the rotation to one or more axes.")]
    [EnumMaskButton(styles = "toggle")]
    public Axis FreezeRotation;
    [InGroup("G1")]
    [Label(title = "Speed Translation Z1")]
    [Tooltip("This value defines how fast the workspace is moved in zone 1.")]
    [Slider(0, 5)]
    public float SpeedT1 = 1.0f;
    [InGroup("G1")]
    [Label(title = "Speed Translation Z2")]
    [Tooltip("This value defines how fast the workspace is moved in zone 2.")]
    [Slider(0, 5)]
    public float SpeedT2 = 1.0f;
    [InGroup("G1")]
    [Label(title = "Speed Rotation Z1")]
    [Tooltip("This value defines how fast the workspace is rotated in zone 1.")]
    [Slider(0, 5)]
    public float SpeedR1 = 1.0f;
    [InGroup("G1")]
    [Label(title = "Speed Rotation Z2")]
    [Tooltip("This value defines how fast the workspace is rotated in zone 2.")]
    [Slider(0, 5)]
    public float SpeedR2 = 1.0f;
    [InGroup("G1")]
    [Label(title = "In Death Zone first")]
    [Tooltip("If this option is activated, the angle of the joint must first be in the angle range of the death zone before translation or rotation become active. ")]
    [StackableField]
    public bool InDZfirst = false;
    [InGroup("G1")]
    [Label(title = "Disable Collisions")]
    [Tooltip("By enabling this options all collisions will be ignored as long as navigation mode is enabled.")]
    [StackableField]
    public bool DisableCollisions = false;

    [InGroup("G1")]
    [Label(title = "Edit Speed Zones")]
    [Tooltip("By enabling this options it is possible to specify the individual zones for navigation. ( Joint X,Y,Z – Translation X,Y,Z, Gimbal Joint X,Y,Z - Rotation X,Y,Z)")]
    [StackableField]
    public bool SpeedZone;

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Translation X")]
    public string labelOnly3;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTXZ1p = new Vector2(10, 40);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTXZ0 = new Vector2(-10, 10);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTXZ1n = new Vector2(-40, -10);

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Translation Y")]
    public string labelOnly4;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTYZ1p = new Vector2(65, 95);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTYZ0 = new Vector2(45, 65);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTYZ1n = new Vector2(12, 45);

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Translation Z")]
    public string labelOnly5;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTZZ1p = new Vector2(60, 100);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTZZ0 = new Vector2(40, 60);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderTZZ1n = new Vector2(-5, 40);

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Rotation X")]
    public string labelOnly6;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRXZ1p = new Vector2(15, 95);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRXZ0 = new Vector2(-15, 15);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRXZ1n = new Vector2(-80, -15);

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Rotation Y")]
    public string labelOnly7;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRYZ1p = new Vector2(-15, 100);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRYZ0 = new Vector2(-15, 15);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRYZ1n = new Vector2(-100, -15);

    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [LabelOnly]
    [Label(title = "Zone Setup Rotation Z")]
    public string labelOnly8;
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1+")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRZZ1p = new Vector2(20, 100);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone0")]
    [Color(0.99f, 0.2f, 0.2f, 1.0f)]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRZZ0 = new Vector2(-20, 20);
    [ShowIf("$SpeedZone")]
    [InGroup("G1")]
    [Label(title = "Zone1-")]
    [Color(0.0f, 0.99f, 0.0f, 1.0f)]
    [ShowIf("#CorrectValues")]
    [RangeSlider(-120, 120, integer = true, showInLabel = true)]
    public Vector2 SliderRZZ1n = new Vector2(-100, -20);

    [Foldout(title = "Global Vibration Settings", order = 2)]
    [Group("G2", 3)]
    [Label(title = "Frequency")]
    [StackableField]
    public int VibrationGFrequency;
    [InGroup("G2")]
    [Label(title = "Magnitude")]
    [Slider(0, 1)]
    public float VibrationGMag;
    [InGroup("G2")]
    [Label(title = "Direction")]
    [StackableField]
    public Vector3 VibrationGDir;
    [InGroup("G2")]
    [Label(title = "Enable Vibration")]
    [StackableField]
    public bool enable_Vibration = false;

    [Foldout(title = "Global Spring Settings", order = 2)]
    [Group("G3", 3)]
    [Label(title = "Magnitude")]
    [Slider(0, 1)]
    public float SpringGMag;
    [InGroup("G3")]
    [Label(title = "Position")]
    [StackableField]
    public Vector3 SpringGDir;
    [InGroup("G3")]
    [Label(title = "Spring Anchor Object")]
    [StackableField]
    public GameObject SpringAnchorObj;
    [InGroup("G3")]
    [Label(title = "Enable Spring")]
    [StackableField]
    public bool enable_GloablSpring = false;

    [Foldout(title = "Global Constant Force Settings", order = 2)]
    [Group("G4", 1)]
    [Label(title = "Magnitude")]
    [Slider(0, 1)]
    public float ConstForceGMag;
    [InGroup("G4")]
    [Label(title = "Direction")]
    [StackableField]
    public Vector3 ConstForceGDir;

    #endregion

    #region Control_Members
    int[] Buttons;
    int[] LastButtons;
    int inkwell;
    private float last_force;
    private Queue hapticErrorQueue;
    [HideInInspector]
    public bool enable_damping = false;
    double[] max_extents = { -0.210, -0.110, -0.085, 0.210, 0.205, 0.130 };
    double[] usable_extents = { -0.08, -0.06, -0.035, 0.08, 0.06, 0.035 };
    private Matrix4x4 DeviceTransformRaw;   //!< (Readonly) Stylus transform, in device coordinates.
    float ScaleFactor = 1.0f;
    [HideInInspector]
    public Vector3 LastContact;
    [HideInInspector]
    public Vector3 LastContactNormal;
    [HideInInspector]
    public bool bIsGrabbingActive;
    [HideInInspector]
    public bool bIsGrabbing;
    [HideInInspector]
    public bool bIsRelease;
    private float oldDrag;
    private float oldAngularDrag;
    bool isTouching;
    bool transformDirect = true;

    Vector3 debug_force_collider;
    float debug_force_collider_mag;
    Vector3 debug_collision_obj;
    float debug_collision_mag;

    //public float debug_max_velocity;

    Vector3 stiffnessForceD;
    Vector3 viscosityForceD;
    Vector3 dynamicFrictionForceD;
    Vector3 staticFrictionForceD;
    Vector3 constantForceD;
    Vector3 springForceD;

    int counter = 0;

    // Camera
    GameObject inital_camera_rot;
    private bool enable_WS_translate = false;
    private bool enable_WS_rotate = false;
    bool[] isDZFirst = { false, false, false, false, false, false };
    // Global FX
    //bool enable_Vibration = false;

    bool enable_GlobalConstForce = false;

    private GameObject ExternalRBControl = null;

    #endregion

    #region ContactPointInfo_Exchange
    [System.Serializable]
    public struct ContactPointInfo
    {
        public Vector3 Location;
        public Vector3 Normal;
        public float MaterialStiffness;
        public float MaterialDamping;
        public float MaterialForce;
        public float MaterialViscosity;
        public float MaterialFrictionStatic;
        public float MaterialFrictionDynamic;
        public float MaterialConstantForce;
        public Vector3 MatConstForceDir;
        public float MaterialSpring;
        public Vector3 MatSpringDir;

        public float MaterialMass;
        public float RigBodySpeed;
        public Vector3 RigBodyVelocity;
        public Vector3 RigBodyAngularVelocity;
        public float RigBodyMass;
        public Vector3 ColImpulse;
        public float PhxDeltaTime;
        public float ImpulseDepth;

        public string ColliderName;
    }

    public ContactPointInfo contactPInfo;

    public struct ContactInfo
    {
        public Vector3 AnchorPoint;
        public List<ContactPointInfo> ContactPointsInfo;
    }

    public List<ContactPointInfo> ContactPointsInfo = new List<ContactPointInfo>();

    public struct ActiveMaterial
    {
        public int MaterialID;
        public float PopThrough;
        public bool PopThroughDone;
    }

    public List<ActiveMaterial> ActiveMaterials = new List<ActiveMaterial>();

    #endregion

    #region Unity_Default_Functions 
    public void OnEnable()
    {
        Buttons = new int[4];
        LastButtons = new int[4];

        for (int i = 0; i < 4; i++)
        {
            Buttons[i] = 0;
            LastButtons[i] = 0;

        }

        if (ScaleToMeter)
        {
            ScaleFactor = GlobalScale;
        }
        else
        {
            ScaleFactor = 1.0f;
        }

        // Get Version String
        StringBuilder sb = new StringBuilder(256);
        getVersionString(sb, sb.Capacity);
        Debug.Log("Haptic Plugin Version : " + sb.ToString());

        if (ConnectOnStart)
        {
            InitializeHapticDevice();
        }

        isTouching = false;
        hapticErrorQueue = new Queue();


    }


    public void Start()
    {
        //debug_max_velocity = 0.0f;
        startSchedulers();
        resetContactPointInfo(DeviceIdentifier);
        ContactPointsInfo.Clear();
        transformDirect = true;
        if (masterCamera != null)
        {
            inital_camera_rot = new GameObject("Temp");
            inital_camera_rot.transform.rotation = masterCamera.transform.rotation;
        }

    }

    private void Update()
    {


    }



    public void FixedUpdate()
    {
        if (DeviceHHD >= 0)
        {
            UpdateDeviceInformation();

            UpdateTransfrom();

            SendContactpoints();
            ContactPointsInfo.Clear();

        }

    }

    private void OnDestroy()
    {
        //if (isIncorrectVersion) return;

        Debug.Log("Disconnecting from Haptic");
        //resetContactPointInfo(configName);
        disconnectAllDevices();

    }
    #endregion

    #region HapticDevice
    public bool InitializeHapticDevice()
    {
        bool success = false;
        DeviceHHD = initDevice(DeviceIdentifier);
        if (DeviceHHD < 0)
        {
            //Error.
            DeviceIdentifier = "Not Connected";
            ModelType = "Not Connected";
            success = false;
            //showNoDevicePopup = true;

            if (DeviceHHD == -1001) // Constant indicating incorrect OH Version
            {
                //showOldVersionPopup = true;
                //isIncorrectVersion = true;
                hapticErrorQueue.Enqueue(System.DateTime.Now.ToLongTimeString() + " - " + "Incorrect Open Haptic Version.");
            }


        }
        else
        {

            StringBuilder sb = new StringBuilder(256);
            getDeviceSN(DeviceIdentifier, sb, sb.Capacity);
            SerialNumber = sb.ToString();
            sb.Clear();
            getDeviceModel(DeviceIdentifier, sb, sb.Capacity);
            ModelType = sb.ToString();



            getDeviceMaxValues(DeviceIdentifier, ref MaxStiffness, ref MaxDamping, ref MaxForce);
            getWorkspaceArea(DeviceIdentifier, usable_extents, max_extents);


            if (ScaleToMeter)
            {
                ScaleFactor = GlobalScale;
            }
            else
            {
                ScaleFactor = 1.0f;
            }

            for (int i = 0; i < 6; i++)
            {
                usable_extents[i] = usable_extents[i] * ScaleFactor;
                max_extents[i] = max_extents[i] * ScaleFactor;
            }


            success = true;
            //showNoDevicePopup = false;
            //isIncorrectVersion = false;
        }

        return success;
    }

    public void DisconnectHapticDevice()
    {
        disconnectAllDevices();
    }


    public void GetDeviceTransformationRaw()
    {
        double[] matInput = new double[16];
        getTransform(DeviceIdentifier, matInput);

        for (int ii = 0; ii < 16; ii++)
            if (ii % 4 != 3)
                matInput[ii] *= ScaleFactor;

        Matrix4x4 mat;
        mat.m00 = (float)matInput[0];
        mat.m01 = (float)matInput[1];
        mat.m02 = (float)matInput[2];
        mat.m03 = (float)matInput[3];
        mat.m10 = (float)matInput[4];
        mat.m11 = (float)matInput[5];
        mat.m12 = (float)matInput[6];
        mat.m13 = (float)matInput[7];
        mat.m20 = (float)matInput[8];
        mat.m21 = (float)matInput[9];
        mat.m22 = (float)matInput[10];
        mat.m23 = (float)matInput[11];
        mat.m30 = (float)matInput[12];
        mat.m31 = (float)matInput[13];
        mat.m32 = (float)matInput[14];
        mat.m33 = (float)matInput[15];
        DeviceTransformRaw = mat.transpose;

    }

    public void UpdateDeviceInformation()
    {
        double[] temp_double_array = new double[3];
        double[] temp_double_array2 = new double[3];
        double[] temp_double_array3 = new double[3];
        double[] temp_double_array4 = new double[3];
        double[] temp_double_array5 = new double[3];
        double[] temp_double_array6 = new double[3];

        getPosition(DeviceIdentifier, temp_double_array);
        CurrentPosition = DoubleArrayToVector3(temp_double_array);

        getVelocity(DeviceIdentifier, temp_double_array);
        CurrentVelocity = DoubleArrayToVector3(temp_double_array);

        // if(CurrentVelocity.x > debug_max_velocity)
        // {
        //     debug_max_velocity = CurrentVelocity.x;
        // }

        getCurrentForce(DeviceIdentifier, temp_double_array);
        CurrentForce = DoubleArrayToVector3(temp_double_array);

        MagForce = CurrentForce.magnitude;
        if (MagForce > 0)
        {
            last_force = MagForce;
        }

        GetDeviceTransformationRaw();

        getJointAngles(DeviceIdentifier, temp_double_array, temp_double_array2);
        JointAngles[0] = (float)temp_double_array[0] * Mathf.Rad2Deg;
        JointAngles[1] = (float)temp_double_array[1] * Mathf.Rad2Deg;
        JointAngles[2] = (float)temp_double_array[2] * Mathf.Rad2Deg;
        GimbalAngles[0] = (float)temp_double_array2[0] * Mathf.Rad2Deg;
        GimbalAngles[1] = (float)temp_double_array2[1] * Mathf.Rad2Deg;
        GimbalAngles[2] = (float)temp_double_array2[2] * Mathf.Rad2Deg;

        Vector3 friction = new Vector3();
        getCurrentFrictionForce(DeviceIdentifier, temp_double_array);
        friction = DoubleArrayToVector3(temp_double_array);
        //Debug.Log("Friction: " + friction.ToString("F5"));

        getLocalForces(DeviceIdentifier, temp_double_array, temp_double_array2, temp_double_array3, temp_double_array4, temp_double_array5, temp_double_array6);
        stiffnessForceD = DoubleArrayToVector3(temp_double_array);
        viscosityForceD = DoubleArrayToVector3(temp_double_array2);
        dynamicFrictionForceD = DoubleArrayToVector3(temp_double_array3);
        staticFrictionForceD = DoubleArrayToVector3(temp_double_array4);
        constantForceD = DoubleArrayToVector3(temp_double_array5);
        springForceD = DoubleArrayToVector3(temp_double_array6);



    }

    public void UpdateTransfrom()
    {

        Debug.Log("local to world devicetransform newmatrix");
        Matrix4x4 newMatrix = transform.localToWorldMatrix * DeviceTransformRaw;
        Debug.Log(transform.localToWorldMatrix);
        Debug.Log(DeviceTransformRaw);
        Debug.Log(newMatrix);
        Vector3 targetPos;
        Vector3 deltaPos;
        Vector3 new_direction;
        Vector3 velocity;
        float distance;
        float magnitude;

        Rigidbody rBody = CollisionMesh.GetComponent<Rigidbody>();

        Debug.Log(transform.localToWorldMatrix);

        targetPos = newMatrix.ExtractPosition();
        deltaPos = targetPos - rBody.position;
        new_direction = deltaPos.normalized;
        velocity = new_direction;
        distance = deltaPos.magnitude;


        if (transformDirect == true)
        {
            rBody.position = targetPos;
            rBody.drag = 0;
            Debug.Log(targetPos);

            VisualizationMesh.transform.SetPositionAndRotation(newMatrix.ExtractPosition(), newMatrix.ExtractRotation());

        }
        else
        {
            VisualizationMesh.transform.SetPositionAndRotation(newMatrix.ExtractPosition() - deltaPos, newMatrix.ExtractRotation());
            if (distance > 0.1 * ScaleFactor)
            {
                magnitude = 20.0f;
                rBody.drag = 1;
                //rBody.AddForce(deltaPos * magnitude, ForceMode.VelocityChange);
            }
            else
            {
                rBody.velocity = Vector3.zero;
                magnitude = 0.0f;
                rBody.drag = 1;



            }
        }
    }




    public void UpdateButtonStatus()
    {
        int[] LastButtonsT = new int[4];

        LastButtons[0] = Buttons[0];
        LastButtons[1] = Buttons[1];
        LastButtons[2] = Buttons[2];
        LastButtons[3] = Buttons[3];

        getButtons(DeviceIdentifier, Buttons, LastButtonsT, ref inkwell);

        //Debug.Log("Button1 = " + Buttons[0] + "  " + LastButtons[0]);

        if (LastButtons[0] == 0 && Buttons[0] == 1)
        {

            Events.OnClickButton1.Invoke();


        }
        if (LastButtons[0] == 1 && Buttons[0] == 1)
        {

            Events.OnHoldButton1.Invoke();


        }
        if (LastButtons[0] == 1 && Buttons[0] == 0)
        {
            Events.OnReleaseButton1.Invoke();
        }

        // Button 2
        if (LastButtons[1] == 0 && Buttons[1] == 1)
        {

            Events.OnClickButton2.Invoke();


        }
        if (LastButtons[1] == 1 && Buttons[1] == 1)
        {

            Events.OnHoldButton2.Invoke();


        }
        if (LastButtons[1] == 1 && Buttons[1] == 0)
        {
            Events.OnReleaseButton2.Invoke();
        }

        if (SimpleButtons == true)
        {

            if (bButton1h == true)
            {
                if (Buttons[0] == 1)
                {
                    bIsGrabbing = true;
                    bIsRelease = false;
                }
                else
                {
                    bIsGrabbing = false;
                    bIsRelease = true;
                }


            }
            else
            {
                if (LastButtons[0] != Buttons[0])
                {
                    //Debug.Log("Button1 = " + Buttons[0] + "  " + LastButtons[0]);
                    if (bButton1g == true && Buttons[0] == 1 && bIsGrabbingActive == false)
                    {

                        bIsGrabbing = true;
                        bIsRelease = false;
                        //Debug.Log("Button 1 - Grabbing");
                    }
                    if (bButton1r == true && Buttons[0] == 1 && GrabObject != null)
                    {

                        bIsRelease = true;
                        bIsGrabbing = false;
                        //Debug.Log("Button 1 - Releasing");
                    }
                }
            }
            if (bButton2h == true)
            {
                if (Buttons[1] == 1)
                {
                    bIsGrabbing = true;
                    bIsRelease = false;
                }
                else
                {
                    bIsGrabbing = false;
                    bIsRelease = true;
                }


            }
            else
            {
                if (LastButtons[1] != Buttons[1])
                {
                    if (bButton2g == true && Buttons[1] == 1 && bIsGrabbingActive == false)
                    {

                        bIsGrabbing = true;
                        bIsRelease = false;
                        //Debug.Log("Button 2 - Grabbing");
                    }
                    if (bButton2r == true && Buttons[1] == 1 && GrabObject != null)
                    {

                        bIsRelease = true;
                        bIsGrabbing = false;
                        //Debug.Log("Button 2 - Releasing");
                    }
                }
            }
        }
    }
    #endregion

    #region Collision_and_Force
    public void UpdateCollision(Collision collision, bool isCollisionStart, bool isCollision, bool isCollisionExit)
    {
        //Debug.Log("Collision" + collision.collider.name);

        float colmass = 1.0f;

        //if (isCollisionExit)
        //{
        //    transformDirect = true;

        //    ContactPointsInfo.Clear();
        //    resetContactPointInfo(DeviceIdentifier);
        //    //updateContactPointInfo(DeviceIdentifier);
        //    isTouching = false;


        //}


        if (isCollision)
        {
            //isTouching = true;
            //Events.OnTouch.Invoke();



            //debug_force_collider = CollisionMesh.GetComponent<Rigidbody>().velocity / Time.fixedDeltaTime;
            //debug_force_collider_mag = Time.fixedDeltaTime;//debug_force_collider.magnitude;

            //if (collision.collider.GetComponent<HapticMaterial>() != null)
            //{
            //    colmass = collision.collider.GetComponent<HapticMaterial>().hMass;
            //}
            //else
            //{
            //    colmass = 1.0f;
            //}
            //if (collision.rigidbody != null)
            //{
            //    debug_collision_obj = CollisionMesh.GetComponent<Rigidbody>().velocity / Time.fixedDeltaTime;
            //    debug_collision_mag = debug_collision_obj.magnitude;
            //    //CollisionMesh.GetComponent<Rigidbody>().AddForce(-1.0f*debug_collision_obj, ForceMode.Force);
            //    //debug_collision_obj = collision.impulse / Time.fixedDeltaTime;
            //    //debug_collision_mag = debug_collision_obj.magnitude;

            //    if (debug_collision_mag > MaxForce)
            //    {
            //        Debug.DrawRay(collision.GetContact(0).point, debug_collision_obj / 100, Color.red, 1, false);
            //    }
            //    else
            //    {
            //        Debug.DrawRay(collision.GetContact(0).point, debug_collision_obj / 100, Color.magenta, 1, false);
            //    }
            //}
            if (bIsGrabbingActive == false)
            {

                transformDirect = false;
                if (bIsGrabbing)
                {

                    transformDirect = true;
                    //GrabObj(collision);
                    //GrabObjectDirect(collision);
                }


            }
            else
            {
                transformDirect = true;
            }
            transformDirect = false;

            Debug.Log("transformDirect");
            Debug.Log(transformDirect);
            ///////TEST BLOCK

            //Vector3 v_stylus_pos = VisualizationMesh.transform.position;//this.transform.InverseTransformPoint(VisualizationMesh.transform.position) / GlobalScale ;
            //Vector3 r_stylus_pos = (transform.localToWorldMatrix * DeviceTransformRaw).ExtractPosition();
            ////Vector3 rr_stylus_pos = CurrentPosition;
            //Vector3 contact_normal = gameObject.transform.InverseTransformVector(collision.GetContact(0).normal);
            //Vector3 contact_point = gameObject.transform.InverseTransformVector(collision.GetContact(0).point);

            //Debug.Log("VPos: " + v_stylus_pos.ToString("F5"));
            //Debug.Log("RRPos: " + rr_stylus_pos.ToString("F5"));
            //Debug.Log("RPos: " + r_stylus_pos.ToString("F5"));

            //Debug.Log("CN: " + contact_normal.ToString("F5"));
            //Debug.Log("CP: " + contact_point.ToString("F5"));

            //Vector3 delta_pos = contact_point + (r_stylus_pos - v_stylus_pos);

            //Debug.Log("DPos: " + delta_pos.ToString("F5"));

            //float pos_d = (Vector3.Dot(r_stylus_pos,contact_normal) - Vector3.Dot(contact_point,contact_normal)) / contact_normal.magnitude;
            //Debug.Log("POS_D: " + pos_d.ToString("F5"));

            //Vector3 cpos = delta_pos;

            //Vector3 epos = cpos - Vector3.Scale((cpos - Vector3.Scale(contact_point, contact_normal)), contact_normal);
            //Debug.Log("EPOS: " + epos.ToString("F5"));

            UpdateForceOnCollision(collision);

            //updateContactPointInfo(DeviceIdentifier);



        }


    }


    public void UpdateForceOnCollision(Collision collision)
    {


        int sFac, vFac, impCorrection;

        sFac = 1;
        vFac = 0;


        HapticMaterial hapMat = collision.collider.GetComponent<HapticMaterial>();
        if (hapMat != null)
        {

            //Debug.Log("contactCount: " + collision.contactCount);

            if (CheckImpulseDirection(collision))
            {
                impCorrection = -1;
            }
            else
            {
                impCorrection = 1;
            }

            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPointInfo contInfo = new ContactPointInfo();


                contInfo.Location = gameObject.transform.InverseTransformPoint(CollisionMesh.transform.position) / ScaleFactor;
                Debug.Log("ContInfo");

                Debug.Log(contInfo.Location);
                contInfo.Normal = gameObject.transform.InverseTransformVector(collision.GetContact(i).normal);
                Debug.Log("ContInfo normal");

                Debug.Log(contInfo.Normal);

                contInfo.MaterialMass = hapMat.hMass;
                contInfo.MaterialStiffness = hapMat.hStiffness * sFac;
                if (hapMat.hStiffness > CollisionMesh.GetComponent<ExperimentHapticCollider>().hStiffness)
                {
                    contInfo.MaterialStiffness = CollisionMesh.GetComponent<ExperimentHapticCollider>().hStiffness * sFac;
                }
                FixedJoint joint = (FixedJoint)CollisionMesh.GetComponent(typeof(FixedJoint));
                //if (joint != null && GrabObject != null)
                //{
                //    if (hapMat.hStiffness > GrabObject.GetComponent<HapticMaterial>().hStiffness)
                //    {
                //        contInfo.MaterialStiffness = GrabObject.GetComponent<HapticMaterial>().hStiffness * sFac;
                //    }
                //}

                if (enable_damping == true)
                {
                    contInfo.MaterialDamping = hapMat.hDamping;
                }
                else
                {
                    contInfo.MaterialDamping = hapMat.hDamping * 0.0f;
                }
                contInfo.MaterialFrictionStatic = hapMat.hFrictionS;
                contInfo.MaterialFrictionDynamic = hapMat.hFrictionD;
                contInfo.MaterialViscosity = hapMat.hViscosity * vFac;
                contInfo.MaterialSpring = hapMat.hSpringMag;
                contInfo.MaterialConstantForce = hapMat.hConstForceMag;
                contInfo.MatConstForceDir = hapMat.hConstForceDir;

                if (hapMat.UseContactNormalCF)
                {
                    contInfo.MatConstForceDir = contInfo.Normal;
                    if (hapMat.ContactNormalInverseCF)
                    {
                        contInfo.MatConstForceDir *= -1.0f;
                    }
                }

                contInfo.MaterialSpring = hapMat.hSpringMag;
                contInfo.MatSpringDir = hapMat.hSpringDir;
                if (hapMat.SpringAnchorObj != null)
                {
                    contInfo.MatSpringDir = gameObject.transform.InverseTransformPoint(hapMat.SpringAnchorObj.transform.position) / ScaleFactor;
                }


                if (collision.collider.GetComponent<Rigidbody>() != null)
                {
                    contInfo.RigBodySpeed = collision.collider.GetComponent<Rigidbody>().velocity.magnitude;
                    contInfo.RigBodyVelocity = collision.collider.GetComponent<Rigidbody>().velocity;
                    contInfo.RigBodyAngularVelocity = collision.collider.GetComponent<Rigidbody>().angularVelocity;
                    contInfo.RigBodyMass = collision.collider.GetComponent<Rigidbody>().mass;
                    //contInfo.ColImpulse = collision.impulse.magnitude * contInfo.Normal;
                    contInfo.ColImpulse = collision.impulse * impCorrection;
                    //Debug.Log("Collision Impulse:" + contInfo.ColImpulse);
                    contInfo.PhxDeltaTime = Time.fixedDeltaTime;
                    contInfo.ImpulseDepth = hapMat.hImpulseD;
                }
                else
                {
                    contInfo.RigBodySpeed = 0.0f;
                    contInfo.RigBodyVelocity = Vector3.zero;
                    contInfo.RigBodyAngularVelocity = Vector3.zero;
                    contInfo.RigBodyMass = 1.0f;
                    contInfo.ColImpulse = Vector3.zero;
                    contInfo.PhxDeltaTime = Time.fixedDeltaTime;
                    contInfo.ImpulseDepth = 0.0f;
                }


                contInfo.ColliderName = collision.collider.name;


                if (contInfo.Normal.magnitude > 0 && !isInCPList(contInfo))
                {

                    ContactPointsInfo.Add(contInfo);

                }

                LastContact = contInfo.Location;
                LastContactNormal = contInfo.Normal;


                //Debug.DrawRay(contInfo.Location*ScaleFactor, contInfo.Normal/10, Color.green, 2, false);
            }


        }

    }

    private void SendContactpoints()
    {
        counter = 0;
        resetContactPointInfo(DeviceIdentifier);
        for (int i = 0; i < ContactPointsInfo.Count; i++)
        {
            addContactPointInfo(DeviceIdentifier,
                                Vector3ToDoubleArray(ContactPointsInfo[i].Location),
                                Vector3ToDoubleArray(ContactPointsInfo[i].Normal),
                                ContactPointsInfo[i].MaterialStiffness,
                                ContactPointsInfo[i].MaterialDamping,
                                Vector3ToDoubleArray(ContactPointsInfo[i].Location),
                                ContactPointsInfo[i].MaterialViscosity,
                                ContactPointsInfo[i].MaterialFrictionStatic,
                                ContactPointsInfo[i].MaterialFrictionDynamic,
                                Vector3ToDoubleArray(ContactPointsInfo[i].MatConstForceDir),
                                ContactPointsInfo[i].MaterialConstantForce,
                                Vector3ToDoubleArray(ContactPointsInfo[i].MatSpringDir),
                                ContactPointsInfo[i].MaterialSpring,
                                0.0f, 0.0f,
                                ContactPointsInfo[i].MaterialMass,
                                ContactPointsInfo[i].RigBodySpeed,
                                Vector3ToDoubleArray(ContactPointsInfo[i].RigBodyVelocity),
                                Vector3ToDoubleArray(ContactPointsInfo[i].RigBodyAngularVelocity),
                                ContactPointsInfo[i].RigBodyMass,
                                Vector3ToDoubleArray(ContactPointsInfo[i].ColImpulse),
                                ContactPointsInfo[i].PhxDeltaTime,
                                ContactPointsInfo[i].ImpulseDepth);
            counter++;

        }
        //Debug.Log("Number of Collision Points:" + counter);
        //x=-5.2
        updateContactPointInfo(DeviceIdentifier);
        //Vector3 anPos = gameObject.transform.InverseTransformPoint(CollisionMesh.transform.position);
        Vector3 anPos = this.transform.InverseTransformPoint(VisualizationMesh.transform.position) / ScaleFactor;
        anPos = new Vector3(anPos.x * 51.0f, anPos.y, anPos.z);
        Debug.Log("anchor");
        Debug.Log(anPos);
        Debug.Log(this.transform.position);
        Debug.Log(ScaleFactor);
        setAnchorPosition(DeviceIdentifier, Vector3ToDoubleArray(anPos));
        //setAnchorPosition(DeviceIdentifier, Vector3ToDoubleArray(CurrentPosition));
        //Debug.Log("AnchorPos:" + anPos);
        //Debug.Log("Velocity:" + CollisionMesh.GetComponent<Rigidbody>().velocity);
    }
    #endregion

    #region Helpers
    public bool CorrectValues()
    {
        SliderTXZ1n.y = SliderTXZ0.x;
        SliderTXZ1p.x = SliderTXZ0.y;

        SliderTYZ1n.y = SliderTYZ0.x;
        SliderTYZ1p.x = SliderTYZ0.y;

        SliderTZZ1n.y = SliderTZZ0.x;
        SliderTZZ1p.x = SliderTZZ0.y;

        SliderRXZ1n.y = SliderRXZ0.x;
        SliderRXZ1p.x = SliderRXZ0.y;

        SliderRYZ1n.y = SliderRYZ0.x;
        SliderRYZ1p.x = SliderRYZ0.y;

        SliderRZZ1n.y = SliderRZZ0.x;
        SliderRZZ1p.x = SliderRZZ0.y;
        return true;
    }

    private bool isInMaterialList(int MaterialID)
    {
        bool result = false;
        foreach (ActiveMaterial aMat in ActiveMaterials)
        {
            if (MaterialID == aMat.MaterialID)
            {
                result = true;
            }
        }
        return result;
    }

    private bool isPopThroughDone(int MaterialID)
    {
        bool result = false;
        foreach (ActiveMaterial aMat in ActiveMaterials)
        {
            if (MaterialID == aMat.MaterialID)
            {
                result = aMat.PopThroughDone;
            }
        }
        return result;

    }




    private void UpdatePopThrough(int MaterialID, bool status)
    {
        for (int i = 0; i < ActiveMaterials.Count; i++)
        {
            if (MaterialID == ActiveMaterials[i].MaterialID)
            {
                ActiveMaterial tempMat = new ActiveMaterial();
                tempMat.MaterialID = ActiveMaterials[i].MaterialID;
                tempMat.PopThrough = ActiveMaterials[i].PopThrough;
                tempMat.PopThroughDone = status;
                //tempMat.SpringDone = ActiveMaterials[i].SpringDone;

                ActiveMaterials[i] = tempMat;

            }
        }
    }


    private void UpdateActiveMaterial(ref ActiveMaterial instance, bool status)
    {
        instance.PopThroughDone = status;

    }

    private bool isInCPList(ContactPointInfo cpi)
    {
        bool result = false;

        foreach (ContactPointInfo ContPI in ContactPointsInfo)
        {
            if (cpi.ColliderName == ContPI.ColliderName)
            {
                result = true;
            }
        }


        return result;

    }

    private bool CheckImpulseDirection(Collision collision)
    {
        bool result = false;
        Vector3 ContPointSum = new Vector3(0.0f, 0.0f, 0.0f);

        for (int i = 0; i < collision.contactCount; i++)
        {

            ContPointSum = ContPointSum + gameObject.transform.InverseTransformVector(collision.GetContact(i).normal);
        }

        float angle = Vector3.Angle(ContPointSum, collision.impulse);
        //Debug.Log("Normal / Impulse Angle: " + angle);

        if (angle > 150.0f)
        {
            result = true;
        }

        return result;
    }

    private Vector3 DetermineTorque(Rigidbody body, Quaternion targetRotation)
    {
        if (body == null)
            return Vector3.zero;

        Quaternion AngleDifference = targetRotation * Quaternion.Inverse(body.rotation);

        float AngleToCorrect = Quaternion.Angle(body.rotation, targetRotation);
        Vector3 Perpendicular = Vector3.Cross(transform.up, transform.forward);
        if (Vector3.Dot(targetRotation * Vector3.forward, Perpendicular) < 0)
            AngleToCorrect *= -1;
        Quaternion Correction = Quaternion.AngleAxis(AngleToCorrect, transform.up);

        Vector3 MainRotation = RectifyAngleDifference((AngleDifference).eulerAngles);
        Vector3 CorrectiveRotation = RectifyAngleDifference((Correction).eulerAngles);

        Vector3 torque = ((MainRotation - CorrectiveRotation / 2) - body.angularVelocity);
        return torque;
    }
    private Vector3 RectifyAngleDifference(Vector3 angdiff)
    {
        if (angdiff.x > 180) angdiff.x -= 360;
        if (angdiff.y > 180) angdiff.y -= 360;
        if (angdiff.z > 180) angdiff.z -= 360;
        return angdiff;
    }


    private static Vector3 DoubleArrayToVector3(double[] darray)
    {
        Vector3 vec3out;

        vec3out.x = (float)darray[0];
        vec3out.y = (float)darray[1];
        vec3out.z = (float)darray[2];

        return vec3out;
    }

    private static double[] Vector3ToDoubleArray(Vector3 vec)
    {
        double[] darray = new double[3];

        darray[0] = vec.x;
        darray[1] = vec.y;
        darray[2] = vec.z;

        return darray;
    }

    private static double[] Vector3ArrayToDoubleArray(Vector3[] inVectors)
    {
        double[] outDoubles = new double[inVectors.Length * 3];

        for (int ii = 0; ii < inVectors.Length; ii++)
        {
            outDoubles[3 * ii + 0] = inVectors[ii].x;
            outDoubles[3 * ii + 1] = inVectors[ii].y;
            outDoubles[3 * ii + 2] = inVectors[ii].z;
        }

        return outDoubles;
    }
    //Convert Matrix4x4 to double[]  
    private static double[] MatrixToDoubleArray(Matrix4x4 M)
    {
        double[] out16 = new double[16];

        out16[0] = M.m00;
        out16[1] = M.m10;
        out16[2] = M.m20;
        out16[3] = M.m30;

        out16[4] = M.m01;
        out16[5] = M.m11;
        out16[6] = M.m21;
        out16[7] = M.m31;

        out16[8] = M.m02;
        out16[9] = M.m12;
        out16[10] = M.m22;
        out16[11] = M.m32;

        out16[12] = M.m03;
        out16[13] = M.m13;
        out16[14] = M.m23;
        out16[15] = M.m33;

        return out16;
    }
    #endregion

    #region Gizmos
#if UNITY_EDITOR



    // In editor Gizmos
    void OnDrawGizmos()
    {
        if (DeviceHHD >= 0)
        {
            // Draw Extants

            const int minX = 0;
            const int minY = 1;
            const int minZ = 2;
            const int maxX = 3;
            const int maxY = 4;
            const int maxZ = 5;

            Vector3 usableBox = new Vector3(
                                    (float)(usable_extents[maxX] - usable_extents[minX]),
                                    (float)(usable_extents[maxY] - usable_extents[minY]),
                                    (float)(usable_extents[maxZ] - usable_extents[minZ]));
            Vector3 usableCenter = new Vector3(
                                       0.5f * (float)(usable_extents[maxX] + usable_extents[minX]),
                                       0.5f * (float)(usable_extents[maxY] + usable_extents[minY]),
                                       0.5f * (float)(usable_extents[maxZ] + usable_extents[minZ]));

            Gizmos.color = Color.green;
            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(usableCenter, usableBox);

            Vector3 maxBox = new Vector3(
                                 (float)(max_extents[maxX] - max_extents[minX]),
                                 (float)(max_extents[maxY] - max_extents[minY]),
                                 (float)(max_extents[maxZ] - max_extents[minZ]));
            Vector3 maxCenter = new Vector3(
                                    0.5f * (float)(max_extents[maxX] + max_extents[minX]),
                                    0.5f * (float)(max_extents[maxY] + max_extents[minY]),
                                    0.5f * (float)(max_extents[maxZ] + max_extents[minZ]));


            if (SceneLimit)
            {
                Vector3 navigationBox = Max_TNav - Min_TNav + maxBox;

                Vector3 navigationCenter = 0.5f * (Max_TNav + Min_TNav) + maxCenter;

                //Vector3 navigationCenter = gameObject.transform.InverseTransformPoint(.transform.localPosition) + 0.5f * (maxCenter + navigationBox);
                //Vector3 navigationCenter = maxBox;
                //Vector3 navigationBox= 2.0f * Max_TNav.transform.position - Min_TNav.transform.position;

                Gizmos.color = Color.red;
                if (transform.parent != null)
                {
                    Gizmos.matrix = gameObject.transform.parent.localToWorldMatrix;
                }
                else
                {
                    Gizmos.matrix = Matrix4x4.identity;
                }
                Gizmos.DrawWireCube(navigationCenter, navigationBox);
                //Handles.SphereHandleCap(0, maxCenter+this.transform.position, this.transform.rotation, 0.2f,EventType.Repaint );
            }

            Gizmos.color = Color.yellow;
            //if (isNavRotation())
            //{
            Gizmos.color = Color.blue;
            //}
            //if (isNavTranslation())
            //{
            Gizmos.color = Color.magenta;
            //}
            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(maxCenter, maxBox);



            // Draw Stylus!
            if (bIsGrabbing)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.matrix = gameObject.transform.localToWorldMatrix * DeviceTransformRaw;
            Gizmos.DrawWireSphere(Vector3.zero, 10);
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(new Vector3(0, 0, 40), new Vector3(5, 5, 80));

            // Draw Buttons
            if (Buttons != null)
            {
                if (Buttons.Length > 0)
                {
                    if (Buttons[0] == 1)
                        Gizmos.color = Color.green;
                    else
                        Gizmos.color = Color.gray;
                    Gizmos.DrawWireSphere(new Vector3(0, 0, 10), 4f);
                    if (Buttons[1] >= 0)
                    {
                        if (Buttons[1] == 1)
                            Gizmos.color = Color.green;
                        else
                            Gizmos.color = Color.gray;
                        Gizmos.DrawWireSphere(new Vector3(0, 0, 20), 4f);
                    }
                }
            }


            if (MagForce > 0)
            {
                GUIStyle style = new GUIStyle();
                style.fontSize = 20;
                style.normal.textColor = Color.white;
                Gizmos.color = new Color(2.0f * CurrentForce.magnitude / (float)MaxForce, 2.0f * (1 - CurrentForce.magnitude / (float)MaxForce), 0);//percentToColor(CurrentForce.magnitude/ (float)MaxForce);
                                                                                                                                                    //Gizmos.matrix = gameObject.transform.localToWorldMatrix * DeviceTransformRaw;
                Gizmos.matrix = Matrix4x4.identity;

                //Gizmos.matrix.s .SetTRS(CollisionMesh.transform.position, Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));

                Gizmos.DrawCube(CollisionMesh.transform.position + new Vector3(0.0f, 0.05f * MagForce / 2.0f, 0.0f), new Vector3(0.01f, 0.05f * MagForce, 0.01f));
                //Gizmos.DrawCube(CollisionMesh.transform.position + new Vector3(10.0f*MagForce, 0.0f,0.0f) , new Vector3(-20.0f*MagForce, 20.0f, 0.01f));
                Handles.Label(CollisionMesh.transform.position + new Vector3(0.0f, 0.05f * MagForce / 2.0f, 0.0f) + new Vector3(0.0f, 0.05f * MagForce / 2 + 0.025f, 0.0f), "" + MagForce, style);
            }
            /*
            if (stiffnessForceD.magnitude > 0)
            {
                GUIStyle style1 = new GUIStyle();
                style1.fontSize = 10;
                style1.normal.textColor = Color.white;
                Gizmos.color = new Color(2.0f * stiffnessForceD.magnitude / (float)MaxForce, 2.0f * (1 - stiffnessForceD.magnitude / (float)MaxForce), 0);//percentToColor(CurrentForce.magnitude/ (float)MaxForce);
                Gizmos.matrix = gameObject.transform.localToWorldMatrix;                                                                                                                                    //Gizmos.matrix = gameObject.transform.localToWorldMatrix * DeviceTransformRaw;
                //Gizmos.matrix = Matrix4x4.identity;

                //Gizmos.matrix.s .SetTRS(CollisionMesh.transform.position, Quaternion.identity, new Vector3(1.0f, 1.0f, 1.0f));

                //Gizmos.DrawCube(usableCenter + new Vector3(0.0f, 0.05f * stiffnessForceD.magnitude / 2.0f, 0.0f), new Vector3(0.01f, 0.05f * stiffnessForceD.magnitude, 0.01f));
                Gizmos.DrawCube(usableCenter + new Vector3(-0.05f * stiffnessForceD.magnitude / 2.0f, 0.0f, 0.0f), new Vector3(-0.05f * stiffnessForceD.magnitude, 0.01f, 0.01f));
                //Gizmos.DrawCube(CollisionMesh.transform.position + new Vector3(10.0f*MagForce, 0.0f,0.0f) , new Vector3(-20.0f*MagForce, 20.0f, 0.01f));
                
                Handles.Label(gameObject.transform.localToWorldMatrix.MultiplyPoint(usableCenter) + new Vector3(-0.05f * stiffnessForceD.magnitude / 2.0f - 0.01f, 0.005f, 0.0f) + new Vector3(-0.05f * stiffnessForceD.magnitude / 2 - 0.025f, 0.005f, 0.0f), "" + stiffnessForceD.magnitude, style1);
            }*/
        }
        else
        {
            // Else no connection.
            Vector3 OmniBox = new Vector3(160, 120, 70);
            Gizmos.color = Color.grey;
            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, OmniBox);


        }
    } //OnDrawGizmos()
#endif
    [CustomEditor(typeof(ExperimentHapticPlugin))]
    public class ExperimentHapticInspector : Editor
    {
        public override void OnInspectorGUI()
        {

            ExperimentHapticPlugin hp = (ExperimentHapticPlugin)target;
            if (GUILayout.Button("Update Device Info"))
            {
                if (hp.InitializeHapticDevice())
                {
                    hp.DisconnectHapticDevice();
                }
            }
            DrawDefaultInspector();


        }
    }
}
#endregion
