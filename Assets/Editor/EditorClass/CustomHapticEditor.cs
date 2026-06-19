using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Runtime.InteropServices;
using System;

public class CustomHapticEditor
{
    //DllImports
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
    //! DllImports


    Vector3 currentPosition;
    string deviceIdentifier = "Default Device";
    public List<ContactPointInfo> ContactPointsInfo = new List<ContactPointInfo>();
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


    private Vector3 CurrentPosition;
    private Vector3 CurrentVelocity;
    private Vector3 CurrentForce;
    private float MagForce;
    private Vector3 JointAngles;
    private Vector3 GimbalAngles;
    private GameObject collisionMesh;
    private GameObject visualizationMesh;
    float ScaleFactor = 1.0f;
    private Matrix4x4 DeviceTransformRaw;
    private Matrix4x4 DeviceTransformRawXandZInverted;

    private Vector3 Adjustposition = new Vector3(-0.0f, -65.5f, -88.1f);
    private Vector3 DeviceTransformRawWhenrefreshed;
    private Vector3 DeviceTransformRawXandZInvertedWhenrefreshed;




    private Vector3 Kijunten;

    private GameObject collideObj;


    public GameObject CollisionMeshPub { get => collisionMesh; set => collisionMesh = value; }
    public GameObject VisualizationMeshPub { get => visualizationMesh; set => visualizationMesh = value; }

    private bool startRunning = false;

    private GameObject target;
    private int counter;
    private Vector3 LastContact;
    private Vector3 LastContactNormal;
    private GameObject startingPosition;
    private GameObject positionZero;



    private bool isCollisionEnter = false;

    private bool enableAndDisable = false;
    double[] temp_double_array_taku = new double[3];

    public CustomHapticEditor(GameObject startingPosition, GameObject positionZero)
    {
        this.startingPosition = startingPosition;
        this.positionZero = positionZero;
    }

    public void setTarget(GameObject newTarget)
    {
        this.target = newTarget;
    }

    public void setUp()
    {
        initDevice(deviceIdentifier);
        startSchedulers();
        startingPosition.transform.position = visualizationMesh.transform.position;
        startRunning = !startRunning;
    }

    public void refresh()
    {
        initDevice(deviceIdentifier);
        startSchedulers();
        startingPosition.transform.position = visualizationMesh.transform.position;
        if (!startRunning)
        {
            GetDeviceTransformationRaw();
            this.DeviceTransformRawWhenrefreshed = DeviceTransformRaw.ExtractPosition();
            this.DeviceTransformRawXandZInvertedWhenrefreshed = DeviceTransformRawXandZInverted.ExtractPosition();
        } else
        {
            collisionExit();
        }
        startRunning = !startRunning;
    }

    // Update is called once per frame
    public void Update()
    {
        if (startRunning)
        {
            UpdateDeviceInformation();
            UpdateTransfrom();
            detectCollision();
            SendContactpoints();
            ContactPointsInfo.Clear();
        }
    }

    public void UpdateDeviceInformation()
    {
        double[] temp_double_array = new double[3];
        double[] temp_double_array2 = new double[3];

        getPosition(deviceIdentifier, temp_double_array);
        CurrentPosition = DoubleArrayToVector3(temp_double_array);


        getVelocity(deviceIdentifier, temp_double_array);
        CurrentVelocity = DoubleArrayToVector3(temp_double_array);


        getCurrentForce(deviceIdentifier, temp_double_array);
        CurrentForce = DoubleArrayToVector3(temp_double_array);

        MagForce = CurrentForce.magnitude;

        GetDeviceTransformationRaw();


        getJointAngles(deviceIdentifier, temp_double_array, temp_double_array2);
        JointAngles[0] = (float)temp_double_array[0] * Mathf.Rad2Deg;
        JointAngles[1] = (float)temp_double_array[1] * Mathf.Rad2Deg;
        JointAngles[2] = (float)temp_double_array[2] * Mathf.Rad2Deg;
        GimbalAngles[0] = (float)temp_double_array2[0] * Mathf.Rad2Deg;
        GimbalAngles[1] = (float)temp_double_array2[1] * Mathf.Rad2Deg;
        GimbalAngles[2] = (float)temp_double_array2[2] * Mathf.Rad2Deg;

    }

    public void UpdateTransfrom()
    {
        Matrix4x4 newMatrix = DeviceTransformRawXandZInverted;
        Vector3 targetPos;
        Vector3 deltaPos;
        Vector3 new_direction;
        Vector3 velocity;
        float distance;
        float magnitude;
        double[] temp = new double[3];

        Rigidbody rBody = collisionMesh.GetComponent<Rigidbody>();

        targetPos = newMatrix.ExtractPosition();
        deltaPos = targetPos - rBody.position;
        new_direction = deltaPos.normalized;
        rBody.position = targetPos;
        rBody.drag = 0;


        //Debug.Log("AdjustPosition" + Adjustposition);
        //Debug.Log("DeviceTransformRaw" + DeviceTransformRaw.ExtractPosition());
        //Debug.Log("Current " + CurrentPosition);

        var view = SceneView.lastActiveSceneView;
        Vector3 sceneCameraEulerAngle = new Vector3();
        if (view != null)
        {
            sceneCameraEulerAngle = view.camera.transform.rotation.eulerAngles;
        }
        float radianx = Mathf.Deg2Rad * sceneCameraEulerAngle.x;
        float radiany = Mathf.Deg2Rad * sceneCameraEulerAngle.y;
        float radianz = Mathf.Deg2Rad * sceneCameraEulerAngle.z;

        //Debug.Log("newmatrix-devicewhenrefreshed" + (newMatrix.ExtractPosition() - DeviceTransformRawWhenrefreshed));
        visualizationMesh.transform.SetPositionAndRotation(startingPosition.transform.position + (applyAllKaiten(newMatrix.ExtractPosition()- DeviceTransformRawXandZInvertedWhenrefreshed,radianx,radiany,radianz) *0.1f), newMatrix.ExtractRotation());
        //visualizationMesh.transform.SetPositionAndRotation(startingPosition.transform.position + (applyAllKaiten((newMatrix.ExtractPosition()), radianx, radiany, radianz) * 0.1f), newMatrix.ExtractRotation());

    }

    public void detectCollision()
    {
        bool isHitColliderOverlapped = false;
        Collider[] hitColliders = Physics.OverlapSphere(target.transform.position, target.GetComponent<SphereCollider>().radius * target.transform.lossyScale.x);
        foreach (var hitCollider in hitColliders)
        {
            isHitColliderOverlapped = true;
            ////Ç±Ç±Ç≈è’ìÀÇåüímÇµÇƒÅAContactpointlist Çí«â¡Ç∑ÇÈ
            if (isCollisionEnter == false)
                collisionEnter();
            collisionStay(hitCollider);
            collideObj = hitCollider.gameObject;
            isCollisionEnter = true;
        }
        //if (isCollisionEnter && !isHitColliderOverlapped)
        //{
        //    collisionExit();
        //    isCollisionEnter = false;
        //}
        hitColliders.Initialize();

    }

    public void GetDeviceTransformationRaw()
    {
        double[] matInput = new double[16];
        getTransform(deviceIdentifier, matInput);
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

        //motonobasho position C
        DeviceTransformRaw.m03 -= Adjustposition.x;
        DeviceTransformRaw.m13 -= Adjustposition.y;
        DeviceTransformRaw.m23 -= Adjustposition.z;

        //Debug.Log("x      " + DeviceTransformRaw.m03);
        //Debug.Log("z      " + DeviceTransformRaw.m23);
        DeviceTransformRawXandZInverted = DeviceTransformRaw;
        DeviceTransformRawXandZInverted.m03 = -DeviceTransformRaw.m03;
        DeviceTransformRawXandZInverted.m23 = -DeviceTransformRaw.m23;
    }

    private void SendContactpoints()
    {
        counter = 0;
        resetContactPointInfo(deviceIdentifier);
        for (int i = 0; i < ContactPointsInfo.Count; i++)
        {
            addContactPointInfo(deviceIdentifier,
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
        updateContactPointInfo(deviceIdentifier);

        try
        {

            var view = SceneView.lastActiveSceneView;
            Vector3 sceneCameraEulerAngle = new Vector3();
            if (view != null)
            {
                sceneCameraEulerAngle = view.camera.transform.rotation.eulerAngles;
            }
            //Debug.Log("euler"+sceneCameraEulerAngle);
            float radianx = Mathf.Deg2Rad * sceneCameraEulerAngle.x;
            float radiany = Mathf.Deg2Rad * sceneCameraEulerAngle.y;
            float radianz = Mathf.Deg2Rad * sceneCameraEulerAngle.z;

            Vector3 CubeSurfacePosition = calculateCollisionPoint(collideObj.GetComponent<BoxCollider>());



            Vector3 CubesurfacePositionFromStartingPosition = revertApplyAllKaiten( startingPosition.transform.InverseTransformPoint(CubeSurfacePosition) , radianx, radiany, radianz);


            //Debug.Log("tempPositionFromStartingPosition      " + CubesurfacePositionFromStartingPosition);
            //Vector3 tempVector = DeviceTransformRawWhenrefreshed;
            //tempVector.x = -tempVector.x;
            //tempVector.z = -tempVector.z;
            //CubeSurfacePosition.x = -CubeSurfacePosition.x;
            //CubeSurfacePosition.z = -CubeSurfacePosition.z;

            Vector3 tempApplied = (CubesurfacePositionFromStartingPosition * 10f);
            tempApplied.x = -tempApplied.x;
            tempApplied.z = -tempApplied.z;
            Vector3 AnchoredPosition = Adjustposition + DeviceTransformRawWhenrefreshed + tempApplied;

            Debug.Log("CubeSurfacePosition" + CubeSurfacePosition);
            Debug.Log("CubesurfacePositionFromStartingPosition" + CubesurfacePositionFromStartingPosition);
            Debug.Log("Applied revert" + tempApplied);
            Debug.Log("Adjust position" + Adjustposition);
            Debug.Log("DeviceTransformRawWhenRefreshed" + DeviceTransformRawWhenrefreshed);
            Debug.Log("DeviceTransformRawXandZInvertedWhenRefreshed" + DeviceTransformRawXandZInvertedWhenrefreshed);
            Debug.Log("current" + CurrentPosition);
            Debug.Log("anchored" + AnchoredPosition);
            Debug.Log("Gosa" + (CurrentPosition - AnchoredPosition));
            //startingPosition.transform.position + (applyAllKaiten(newMatrix.ExtractPosition() - DeviceTransformRawWhenrefreshed, radianx, radiany, radianz) * 0.1f);


            setAnchorPosition(deviceIdentifier, Vector3ToDoubleArray(AnchoredPosition));
            //Debug.Log("temp   " + CubeSurfacePosition);
        } catch (NullReferenceException)
        {
            //Debug.Log("Not detected any object");
        }
    }


    public void UpdateCollision(Collider hitCollider)
    {
        UpdateForceOnCollision(hitCollider);
    }

    public void UpdateForceOnCollision(Collider collider)
    {
        int sFac, vFac, impCorrection;
        sFac = 1;
        vFac = 0;
        HapticMaterial hapMat = collider.GetComponent<HapticMaterial>();
        if (hapMat != null)
        {
                impCorrection = -1;
                ContactPointInfo contInfo = new ContactPointInfo();
            //contInfo.Location = collisionMesh.transform.InverseTransformPoint(collider.transform.position) / ScaleFactor;
            //contInfo.Location = DoubleArrayToVector3(temp_double_array_taku);//ue to issho

            //contInfo.Location = positionZero.transform.InverseTransformPoint(calculateCollisionPoint(collider));
            //contInfo.Location = calculateCollisionPoint(collider);

            Debug.Log("ContInfo");

            Debug.Log(contInfo.Location);



            var view = SceneView.lastActiveSceneView;
            Vector3 sceneCameraEulerAngle = new Vector3();
            if (view != null)
            {
                sceneCameraEulerAngle = view.camera.transform.rotation.eulerAngles;
            }
            //Debug.Log("euler"+sceneCameraEulerAngle);
            float radianx = Mathf.Deg2Rad * sceneCameraEulerAngle.x;
            float radiany = Mathf.Deg2Rad * sceneCameraEulerAngle.y;
            float radianz = Mathf.Deg2Rad * sceneCameraEulerAngle.z;

            //contInfo.Normal = revertApplyAllKaiten( -startingPosition.transform.InverseTransformVector(calculateCollisionVector(collider)),radianx,radiany,radianz);

            Debug.Log("ContInfo normal");

            //Debug.Log(contInfo.Normal);

            contInfo.MaterialMass = hapMat.hMass;
                contInfo.MaterialStiffness = hapMat.hStiffness * sFac;
            Debug.Log("stiffness" + contInfo.MaterialStiffness);
                //if (hapMat.hStiffness > collisionMesh.GetComponent<ExperimentHapticCollider>().hStiffness)
                //{
                    //contInfo.MaterialStiffness = collisionMesh.GetComponent<ExperimentHapticCollider>().hStiffness * sFac;
                //}
                FixedJoint joint = (FixedJoint)collisionMesh.GetComponent(typeof(FixedJoint));
                {
                    contInfo.MaterialDamping = hapMat.hDamping;
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
                    contInfo.MatSpringDir = collisionMesh.transform.InverseTransformPoint(hapMat.SpringAnchorObj.transform.position) / ScaleFactor;
                }
                    contInfo.RigBodySpeed = 0.0f;
                    contInfo.RigBodyVelocity = Vector3.zero;
                    contInfo.RigBodyAngularVelocity = Vector3.zero;
                    contInfo.RigBodyMass = 1.0f;
                    contInfo.ColImpulse = Vector3.zero;
                    contInfo.PhxDeltaTime = Time.fixedDeltaTime;
                    contInfo.ImpulseDepth = 0.0f;
                contInfo.ColliderName = collider.name;
                if (contInfo.Normal.magnitude > 0)
                {
                    ContactPointsInfo.Add(contInfo);
                }
                LastContact = contInfo.Location;
                LastContactNormal = contInfo.Normal;
        }
    }

    //private bool CheckImpulseDirection(Collision collision)
    //{
    //    bool result = false;
    //    Vector3 ContPointSum = new Vector3(0.0f, 0.0f, 0.0f);

    //    for (int i = 0; i < collision.contactCount; i++)
    //    {

    //        ContPointSum = ContPointSum + startingPosition.transform.InverseTransformVector(collision.GetContact(i).normal);
    //    }

    //    float angle = Vector3.Angle(ContPointSum, collision.impulse);
    //    //Debug.Log("Normal / Impulse Angle: " + angle);

    //    if (angle > 150.0f)
    //    {
    //        result = true;
    //    }

    //    return result;
    //}

    private Vector3 calculateCollisionPoint(Collider hitCollider)
    {
        Collider partner = hitCollider;
        Vector3 collisionPoint = new Vector3(999,999,999);
        switch (getAttachedColliderType(visualizationMesh))
        {
            case colliderTypes.Box:
                break;
            case colliderTypes.Capsule:
                break;
            case colliderTypes.Sphere:
                SphereCollider sphereCollider = visualizationMesh.GetComponent<SphereCollider>();
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                        BoxChouten boxChouten = new BoxChouten(hitCollider,visualizationMesh);
                        collisionPoint = boxChouten.getClosestPoint(visualizationMesh);
                        break;
                    case colliderTypes.Capsule:
                        break;
                    case colliderTypes.Sphere:
                        break;
                }
                break;
        }
        return collisionPoint;
    }

    private Vector3 calculateCollisionVector(Collider hitCollider)
    {
        Collider partner = hitCollider;
        Vector3 collisionVector = new Vector3(999, 999, 999);
        switch (getAttachedColliderType(visualizationMesh))
        {
            case colliderTypes.Box:
                break;
            case colliderTypes.Capsule:
                break;
            case colliderTypes.Sphere:
                SphereCollider sphereCollider = visualizationMesh.GetComponent<SphereCollider>();
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                        BoxChouten boxChouten = new BoxChouten(hitCollider,visualizationMesh);
                        boxChouten.getClosestPoint(visualizationMesh);
                        collisionVector = boxChouten.getHousenOfClosestSurface();
                        break;
                    case colliderTypes.Capsule:
                        break;
                    case colliderTypes.Sphere:
                        break;
                }
                break;
        }
        return collisionVector;
    }

    private colliderTypes getAttachedColliderType(GameObject checkTarget)
    {
        colliderTypes type = new colliderTypes();
        if (checkTarget.GetComponent<BoxCollider>())
            type = colliderTypes.Box;
        else if (checkTarget.GetComponent<CapsuleCollider>())
            type = colliderTypes.Capsule;
        else if (checkTarget.GetComponent<SphereCollider>())
            type = colliderTypes.Sphere;
        else
            Debug.LogError("no corresponding collider attached");
        return type;
    }

    private colliderTypes getAttachedColliderType(Collider checkTarget)
    {
        colliderTypes type = new colliderTypes();
        if (checkTarget.GetComponent<BoxCollider>())
            type = colliderTypes.Box;
        else if (checkTarget.GetComponent<CapsuleCollider>())
            type = colliderTypes.Capsule;
        else if (checkTarget.GetComponent<SphereCollider>())
            type = colliderTypes.Sphere;
        else
            Debug.LogError("no corresponding collider attached");
        return type;
    }

    private void collisionEnter()
    {
        Debug.Log("OnCollisionEnter");
    }

    private void collisionStay(Collider hitCollider)
    {
        Debug.Log("OnCollisionStay");

        if (hitCollider.name != "Sphere")
        {
            //Debug.Log(hitCollider);
            UpdateCollision(hitCollider);
            enableAndDisable = true;
        }
    }

    private void collisionExit()
    {
        Debug.Log("OnCollisionExit");
        ContactPointsInfo.Clear();
        resetContactPointInfo(deviceIdentifier);
    }

    private Vector3 applyAllKaiten(Vector3 zahyou, float radianx,float radiany,float radianz)
    {
        Vector3 result = kaitenXjiku(zahyou, radianx);
        result = kaitenYjiku(result, radiany);
        result = kaitenZjiku(result, radianz);
        return result;
    }

    private Vector3 revertApplyAllKaiten(Vector3 zahyou, float radianx, float radiany, float radianz)
    {
        Vector3 result = kaitenXjiku(zahyou, -radianx);
        result = kaitenYjiku(result, -radiany);
        result = kaitenZjiku(result, -radianz);
        return result;
    }

    private Vector3 kaitenYjiku(Vector3 zahyou, float radian)
    {
        return new Vector3(zahyou.x * cos(radian) + zahyou.z * (sin(radian)), zahyou.y, -zahyou.x * (sin(radian)) + zahyou.z * cos(radian));
    }

    private Vector3 kaitenXjiku(Vector3 zahyou, float radian)
    {
        return new Vector3(zahyou.x, zahyou.y * cos(radian) - zahyou.z * sin(radian), zahyou.y * sin(radian) + zahyou.z * cos(radian));
    }

    private Vector3 kaitenZjiku(Vector3 zahyou, float radian)
    {
        return new Vector3(zahyou.x * cos(radian) - zahyou.y * sin(radian), zahyou.x * sin(radian) + zahyou.y * cos(radian), zahyou.z);
    }

    private float sin(float theta)
    {
        return ((float)Math.Round(Math.Sin(theta) * 100.0f)) * 0.01f;
    }

    private float cos(float theta)
    {
        return (((float)Math.Cos(theta) * 100.0f)) * 0.01f;
    }

    private Vector3 DoubleArrayToVector3(double[] darray)
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
}

enum colliderTypes
{
    Box,
    Capsule,
    Sphere
}