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
    [DllImport("HapticsDirect")] public static extern void getVersionString(StringBuilder dest, int len);  //!< OpenHaptics のバージョン文字列を返します。
    // Setup Functions
    [DllImport("HapticsDirect")] public static extern int initDevice(string deviceName);  //!< ハプティックデバイスに接続し、初期化する。
    [DllImport("HapticsDirect")] public static extern void getDeviceSN(string configName, StringBuilder dest, int len);   //!< デバイスのシリアル番号を取得
    [DllImport("HapticsDirect")] public static extern void getDeviceModel(string configName, StringBuilder dest, int len);	//!< デバイスのモデル名を取得
    [DllImport("HapticsDirect")] public static extern void getDeviceMaxValues(string configName, ref double max_stiffness, ref double max_damping, ref double max_force);
    [DllImport("HapticsDirect")] public static extern void startSchedulers(); //!< Open Hapticスケジューラを起動し、必要な内部コールバックを割り当てる。
    // Device Information
    [DllImport("HapticsDirect")] public static extern void getWorkspaceArea(string configName, double[] usable6, double[] max6); //!< デバイスの物理的な制限によって作成された境界を取得します。
    // Updates
    [DllImport("HapticsDirect")] public static extern void getPosition(string configName, double[] position3); //!< デバイスの現在位置をmm単位で取得する。左が+x、上が+y、ユーザー方向が+z。(ユニティCSys)
    [DllImport("HapticsDirect")] public static extern void getVelocity(string configName, double[] velocity3); //!< デバイスの現在の速度を mm/s 単位で取得します。注意：この値は高周波のジッタを減らすために平滑化されます。(ユニティCSys)
    [DllImport("HapticsDirect")] public static extern void getTransform(string configName, double[] matrix16); //!< デバイス・エンデフェクタの列長変換を取得します。(ユニティCSys)
    [DllImport("HapticsDirect")] public static extern void getButtons(string configName, int[] buttons4, int[] last_buttons4, ref int inkwell); //!< ボタンと最後のボタンの状態を取得し、インクウェルスイッチがある場合は、それがアクティブかどうかを取得する。
    [DllImport("HapticsDirect")] public static extern void getCurrentForce(string configName, double[] currentforce3);  //!< デバイスの現在の力を N 単位で取得します。(ユニティCSys)
    [DllImport("HapticsDirect")] public static extern void getJointAngles(string configName, double[] jointAngles, double[] gimbalAngles); //!< デバイスの関節角度をradで取得します。これらはデバイスのベースフレームに対するアーマチュアの運動学を計算するために使用されるジョイント角度です。
    //Touch デバイスの場合： Turret Left +, Thigh Up +, Shin Up + デバイスのジンバルの角度をrad単位で取得します： タッチデバイスの場合：ニュートラルポジションから 右が+、上が-、CWが+。
    [DllImport("HapticsDirect")] public static extern void getCurrentFrictionForce(string configName, double[] frictionForce);
    [DllImport("HapticsDirect")] public static extern void getGlobalForces(string configName, double[] vibrationForce, double[] constantForce, double[] springForce);
    [DllImport("HapticsDirect")] public static extern void getLocalForces(string configName, double[] stiffnessForce, double[] viscosityForce, double[] dynamicFrictionForce, double[] staticFrictionForce, double[] constantForce, double[] springForce);
    // Force output
    [DllImport("HapticsDirect")] public static extern void setForce(string configName, double[] lateral3, double[] torque3); //!< ハプティックデバイスに追加の力を加えます。スクリプト化された力のために使用できますが、ほとんどの場合、エフェクトを使用することが望ましいです。
    [DllImport("HapticsDirect")] public static extern void setAnchorPosition(string configName, double[] position3); //!< 仮想スタイラスのアンカー位置を設定する（Unity CSys）
    [DllImport("HapticsDirect")]
    public static extern void addContactPointInfo(string configName, double[] Location, double[] Normal, float MatStiffness, float MatDamping, double[] MatForce,
    float MatViscosity, float MatFrictionStatic, float MatFrictionDynamic, double[] MatConstForceDir, float MatConstForceMag, double[] MatSpringDir, float MatSpringMag, float MatPopThroughRel, float MatPopThroughAbs,
    double MatMass, double RigBSpeed, double[] RigBVelocity, double[] RigBAngularVelocity, double RigBMass, double[] ColImpulse, double PhxDeltaTime, double ImpulseDepth); //!< コンタクトポイント・リストに衝突コンタクトポイント情報を追加する。
    [DllImport("HapticsDirect")] public static extern void updateContactPointInfo(string configName); //!< コンタクトポイント情報リストの更新
    [DllImport("HapticsDirect")] public static extern void resetContactPointInfo(string configName); //!< コンタクトポイント情報リストをリセット
    [DllImport("HapticsDirect")] public static extern void setVibrationValues(string configName, double[] direction3, double magnitude, double frequency, double time); //!< 振動のパラメーターを設定する
    [DllImport("HapticsDirect")] public static extern void setSpringValues(string configName, double[] anchor, double magnitude); //!< スプリングFXのパラメータを設定する
    [DllImport("HapticsDirect")] public static extern void setConstantForceValues(string configName, double[] direction, double magnitude); //!< コンスタントフォースFXのパラメーターを設定する
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
    private Vector3 Adjustposition = new Vector3(-0.0f, -65.5f, -88.1f);
    private Vector3 DeviceTransformRawWhenrefreshed;



    

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


    //  追加したパラメータ
    private Vector3 virtualProxyPos;     // 仮想拘束点の位置
    private Vector3 virtualProxyVel;     // 仮想拘束点の速度
    private float hapticDt = 0.001f;     // Touch制御周期(1kHz基準)
    private float stiffness = 1200.0f;   // バネ定数
    private float damping = 20.0f;       // ダンパ定数
    private float maxForce = 3.0f;       // 力の上限（N）

    private float currentPenetrationDistance = 0f;
    private Vector3 currentPenetrationDirection = Vector3.zero;

    private Vector3 targetBasePosition;
    private Vector3 deviceBasePosition;
    private bool hasMoveBase = false;

    public CustomHapticEditor(GameObject startingPosition, GameObject positionZero)
    {
        this.startingPosition = startingPosition;
        this.positionZero = positionZero;
    }

    public void setTarget(GameObject newTarget)
    {
        this.target = newTarget;
        this.visualizationMesh = newTarget;
        this.collisionMesh = newTarget;

        ResetMoveBase();
    }
    // 追加した関数　操作したオブジェクトの位置を保存し、操作する際いちいち初期位置に戻ることはなくなった
    private void ResetMoveBase()
    {
        if (visualizationMesh == null)
        {
            hasMoveBase = false;
            return;
        }

        // 選択した瞬間のオブジェクト位置を保存
        targetBasePosition = visualizationMesh.transform.position;

        // 選択した瞬間のデバイス位置を保存
        GetDeviceTransformationRaw();
        deviceBasePosition = DeviceTransformRaw.ExtractPosition();

        // 既存処理との互換用
        startingPosition.transform.position = visualizationMesh.transform.position;
        DeviceTransformRawWhenrefreshed = deviceBasePosition;

        // Virtual Coupling 用
        virtualProxyPos = visualizationMesh.transform.position;
        virtualProxyVel = Vector3.zero;

        hasMoveBase = true;

        Debug.Log("移動基準を更新しました: " + targetBasePosition);
    }
    public void setUp()
    {
        if (visualizationMesh == null)
        {
            Debug.LogWarning("操作対象のオブジェクトが選択されていません。");
            return;
        }
        initDevice(deviceIdentifier);
        startSchedulers();
        ResetMoveBase();
        startRunning = !startRunning;
    }

    public void refresh()
    {
        if (visualizationMesh == null)
        {
            Debug.LogWarning("操作対象のオブジェクトが選択されていません。");
            return;
        }
        initDevice(deviceIdentifier);
        startSchedulers();
        ResetMoveBase();
        if (startRunning)
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
            UpdateTransform();
            detectCollision();
            SendContactpoints();
            ContactPointsInfo.Clear();
            var view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Debug.Log("sceneView" + view.camera.transform.position);
            }
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

    public void UpdateTransform()
    {
        // 変更点：オブジェクトの位置を記憶しその位置からの移動が可能に
        if (visualizationMesh == null || collisionMesh == null)
        {
            return;
        }

        if (!hasMoveBase)
        {
            ResetMoveBase();
            return;
        }

        Matrix4x4 newMatrix = DeviceTransformRaw;

        Vector3 deviceCurrentPosition = newMatrix.ExtractPosition();

        // 選択時からのデバイス移動量
        Vector3 deviceDelta = deviceCurrentPosition - deviceBasePosition;

        // 選択時のオブジェクト位置 + デバイス移動量
        Vector3 newObjectPosition = targetBasePosition + deviceDelta * 0.1f;

        Rigidbody rBody = collisionMesh.GetComponent<Rigidbody>();

        if (rBody != null)
        {
            Vector3 deltaPos = newObjectPosition - rBody.position;
            Vector3 newDirection = deltaPos.normalized;

            rBody.position = newObjectPosition;
            rBody.drag = 0;
        }

        visualizationMesh.transform.SetPositionAndRotation(
            newObjectPosition,
            newMatrix.ExtractRotation()
        );
    }


public void detectCollision()
{
    // 変更点：　Physics.ComputePenetrationを用いて接触判定を行う　コライダーの種類にかかわらず判定可能
    bool isHitColliderOverlapped = false;

    Collider myCollider = target.GetComponent<Collider>();
    if (myCollider == null)
    {
        Debug.LogError("Target に Collider がありません。");
        return;
    }

    // 近傍のすべてのコライダーを取得（必要に応じて範囲調整）
    Collider[] nearbyColliders = Physics.OverlapSphere(target.transform.position, 1.0f);

    foreach (Collider other in nearbyColliders)
    {
        if (other == myCollider) continue; // 自分自身は無視

        Vector3 direction;
        float distance;

        bool isColliding = Physics.ComputePenetration(
            myCollider, target.transform.position, target.transform.rotation,
            other, other.transform.position, other.transform.rotation,
            out direction, out distance
        );

        if (isColliding)
        {
            currentPenetrationDistance = distance;
            currentPenetrationDirection = direction;


            isHitColliderOverlapped = true;

            if (!isCollisionEnter)
            {
                collisionEnter();
                isCollisionEnter = true;
            }

            collisionStay(other);
            collideObj = other.gameObject;
        }
    }

    // 離れたときの処理
    if (isCollisionEnter && !isHitColliderOverlapped)
    {
        currentPenetrationDistance = 0f;
        currentPenetrationDirection = Vector3.zero;

        collisionExit();
        isCollisionEnter = false;
        collideObj = null;
    }
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
            Vector3 CubeSurfacePosition = calculateCollisionPoint(collideObj.GetComponent<Collider>());
            Vector3 CubesurfacePositionFromStartingPosition = startingPosition.transform.InverseTransformPoint(CubeSurfacePosition);

            
            //Debug.Log("tempPositionFromStartingPosition      " + CubesurfacePositionFromStartingPosition);
            Vector3 AnchoredPosition = Adjustposition + DeviceTransformRawWhenrefreshed+ (CubesurfacePositionFromStartingPosition * 10f);
            setAnchorPosition(deviceIdentifier, Vector3ToDoubleArray(AnchoredPosition));
            Debug.Log("Cubesurfaceposition   " + CubeSurfacePosition);
            Debug.Log("CubesurfacePositionFromStartingPosition" + CubesurfacePositionFromStartingPosition);
            Debug.Log("visualization" + visualizationMesh.transform.position);
            Debug.Log("current" + CurrentPosition);
            Debug.Log("AnchoredPosition" + AnchoredPosition);



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
        // 変更点：　meshに接触した際の触覚フィードバックを調整する条件分岐を作成
        int sFac, vFac, impCorrection;
        sFac = 1;
        vFac = 0;

        HapticMaterial hapMat = collider.GetComponent<HapticMaterial>();
        if (hapMat != null)
        {
            impCorrection = -1;
            ContactPointInfo contInfo = new ContactPointInfo();

            Debug.Log("ContInfo");
            Debug.Log(contInfo.Location);

            contInfo.Normal = -startingPosition.transform.InverseTransformVector(calculateCollisionVector(collider));

            contInfo.MaterialMass = hapMat.hMass;

            // まず基本値を入れる
            contInfo.MaterialStiffness = hapMat.hStiffness * sFac;
            contInfo.MaterialDamping = hapMat.hDamping;

            // Mesh のときだけ penetration を使って調整
            if (getAttachedColliderType(collider) == colliderTypes.Mesh)
            {
                float penetration = currentPenetrationDistance;
                Debug.Log("penetration = " + penetration);

                // 剛性は前より控えめに
                contInfo.MaterialStiffness *= penetration * 1000f;

                // ダンピングを追加して振動を抑える
                contInfo.MaterialDamping = hapMat.hDamping + penetration * 300f;

                // 上限をかけて暴れを防ぐ
                contInfo.MaterialStiffness = Mathf.Clamp(contInfo.MaterialStiffness, 0f, 1.0f);
                contInfo.MaterialDamping = Mathf.Clamp(contInfo.MaterialDamping, 0f, 1.2f);
            }

            Debug.Log("stiffness = " + contInfo.MaterialStiffness);
            Debug.Log("damping = " + contInfo.MaterialDamping);

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
                contInfo.MatSpringDir =
                    collisionMesh.transform.InverseTransformPoint(hapMat.SpringAnchorObj.transform.position) / ScaleFactor;
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

    // private bool TryGetBoxSdfResult(Collider boxCollider, out Vector3 closestPoint, out Vector3 normal, out float signedDistance)
    // {
    //     closestPoint = Vector3.zero;
    //     normal = Vector3.zero;
    //     signedDistance = float.PositiveInfinity;

    //     if (boxCollider == null)
    //         return false;

    //     BoxChouten boxChouten = new BoxChouten(boxCollider, visualizationMesh);
    //     signedDistance = boxChouten.SignedDistance(
    //         visualizationMesh.transform.position,
    //         out closestPoint,
    //         out normal
    //     );

    //     return true;
    // }

    private Vector3 calculateCollisionPoint(Collider hitCollider)
    {
        // 変更点：判定可能なコライダーの種類が増えたこと　partnerのコライダー（接触される側）のBoxは先行研究の内容、Meshが自分の成果である
        Collider partner = hitCollider;
        Vector3 collisionPoint = new Vector3(999, 999, 999);
        const float meshContactMargin = 0.002f;

        switch (getAttachedColliderType(visualizationMesh))
        {
            case colliderTypes.Box:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        collisionPoint = boxChouten.getClosestPoint(visualizationMesh);
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        MeshCollider meshCollider = partner as MeshCollider;
                        if (meshCollider != null)
                        {
                            MeshSDF meshSdf = new MeshSDF(meshCollider);

                            Vector3 cp, normal;
                            float d = meshSdf.SignedDistance(
                                visualizationMesh.transform.position,
                                out cp,
                                out normal
                            );

                            
                            
                            collisionPoint = cp;
                            
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Capsule:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        collisionPoint = boxChouten.getClosestPoint(visualizationMesh);
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        MeshCollider meshCollider = partner as MeshCollider;
                        if (meshCollider != null)
                        {
                            MeshSDF meshSdf = new MeshSDF(meshCollider);

                            Vector3 cp, normal;
                            float d = meshSdf.SignedDistance(
                                visualizationMesh.transform.position,
                                out cp,
                                out normal
                            );

                            
                            
                            collisionPoint = cp;
                            
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Sphere:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        collisionPoint = boxChouten.getClosestPoint(visualizationMesh);
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        MeshCollider meshCollider = partner as MeshCollider;
                        if (meshCollider != null)
                        {
                            MeshSDF meshSdf = new MeshSDF(meshCollider);

                            Vector3 cp, normal;
                            float d = meshSdf.SignedDistance(
                                visualizationMesh.transform.position,
                                out cp,
                                out normal
                            );

                            
                            
                            collisionPoint = cp;
                            
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Mesh:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        collisionPoint = boxChouten.getClosestPoint(visualizationMesh);
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        if (partner != null)
                        {
                            collisionPoint = partner.ClosestPoint(visualizationMesh.transform.position);
                        }
                        break;
                    }
                }
                break;
            }
        }

        return collisionPoint;
    }

    private Vector3 calculateCollisionVector(Collider hitCollider)
    {
        // 変更点：判定可能なコライダーの種類が増えたこと　partnerのコライダー（接触される側）のBoxは先行研究の内容、Meshが自分の成果である
        Collider partner = hitCollider;
        Vector3 collisionVector = Vector3.zero;
        const float meshContactMargin = 0.01f;

        switch (getAttachedColliderType(visualizationMesh))
        {
            case colliderTypes.Box:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        boxChouten.getClosestPoint(visualizationMesh);
                        collisionVector = boxChouten.getHousenOfClosestSurface();
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        if (currentPenetrationDirection.sqrMagnitude > 1e-8f)
                        {
                            collisionVector = currentPenetrationDirection.normalized;
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Capsule:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        boxChouten.getClosestPoint(visualizationMesh);
                        collisionVector = boxChouten.getHousenOfClosestSurface();
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        if (currentPenetrationDirection.sqrMagnitude > 1e-8f)
                        {
                            collisionVector = currentPenetrationDirection.normalized;
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Sphere:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        boxChouten.getClosestPoint(visualizationMesh);
                        collisionVector = boxChouten.getHousenOfClosestSurface();
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        if (currentPenetrationDirection.sqrMagnitude > 1e-8f)
                        {
                            collisionVector = currentPenetrationDirection.normalized;
                        }
                        break;
                    }
                }
                break;
            }

            case colliderTypes.Mesh:
            {
                switch (getAttachedColliderType(partner))
                {
                    case colliderTypes.Box:
                    {
                        BoxChouten boxChouten = new BoxChouten(hitCollider, visualizationMesh);
                        boxChouten.getClosestPoint(visualizationMesh);
                        collisionVector = boxChouten.getHousenOfClosestSurface();
                        break;
                    }

                    case colliderTypes.Mesh:
                    {
                        if (partner != null)
                        {
                            Vector3 cp = partner.ClosestPoint(visualizationMesh.transform.position);
                            Vector3 dir = visualizationMesh.transform.position - cp;

                            if (dir.sqrMagnitude > 1e-8f)
                            {
                                collisionVector = dir.normalized;
                            }
                            else if (currentPenetrationDirection.sqrMagnitude > 1e-8f)
                            {
                                collisionVector = currentPenetrationDirection.normalized;
                            }
                        }
                        break;
                    }
                }
                break;
            }
        }

        return collisionVector;
    }

    private float calculateMeshPenetration(Collider hitCollider)
    {
        if (hitCollider == null)
            return 0f;

        MeshCollider meshCollider = hitCollider as MeshCollider;
        if (meshCollider == null)
            return 0f;

        MeshSDF meshSdf = new MeshSDF(meshCollider);

        Vector3 cp, normal;
        float d = meshSdf.SignedDistance(
            visualizationMesh.transform.position,
            out cp,
            out normal
        );

        // d < 0 のときだけめり込み
        float penetration = Mathf.Max(0f, -d);
        return penetration;
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
        else if (checkTarget.GetComponent<MeshCollider>())
            type = colliderTypes.Mesh;
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
        else if (checkTarget.GetComponent<MeshCollider>())
            type = colliderTypes.Mesh;
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
    Sphere,
    Mesh
}