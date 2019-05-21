using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vrcontroller : MonoBehaviour
{
    public Transform HipCenter;
    public Transform Spine;
    public Transform Neck;
    public Transform Head;

    public Transform LeftClavicle;
    public Transform LeftUpperArm;
    public Transform LeftElbow;
    public Transform LeftHand;
    public Transform LeftFingers;

    public Transform RightClavicle;
    public Transform RightUpperArm;
    public Transform RightElbow;
    public Transform RightHand;
    public Transform RightFingers;

    public Transform LeftThigh;
    public Transform LeftKnee;
    public Transform LeftFoot;
    public Transform LeftToes;

    public Transform RightThigh;
    public Transform RightKnee;
    public Transform RightFoot;
    public Transform RightToes;

    public Transform BodyRoot;
    public GameObject OffsetNode;

    Transform[] Bones;
    Quaternion[] InitialRots;

    GameObject Hips;

    KinectManager manager;
    // Start is called before the first frame update
    void Start()
    {
        manager = KinectManager.Instance;
        Bones = new Transform[22];
        Bones[0] = HipCenter;
        Bones[1] = Spine;
        Bones[2] = Neck;
        Bones[3] = Head;
        
        Bones[4] = LeftClavicle;
        Bones[5] = LeftUpperArm;
        Bones[6] = LeftElbow;
        Bones[7] = LeftHand;
        Bones[8] = LeftFingers;

        Bones[9] = RightClavicle;
        Bones[10] = RightUpperArm;
        Bones[11] = RightElbow;
        Bones[12] = RightHand;
        Bones[13] = RightFingers;

        Bones[14] = LeftThigh;
        Bones[15] = LeftKnee;
        Bones[16] = LeftFoot;
        Bones[17] = LeftToes;

        Bones[18] = RightThigh;
        Bones[19] = RightKnee;
        Bones[20] = RightFoot;
        Bones[21] = RightToes;

        InitialRots = new Quaternion[Bones.Length];
        for(int i = 0; i < Bones.Length; i++)
        {
            InitialRots[i] = Bones[i].rotation;
        }
    }

    const bool mirror = true;
    // Update is called once per frame
    void Update()
    {
        if (manager && manager.IsInitialized())
        {
            if (manager.IsUserDetected())
            {
                for(int i = 0; i < Bones.Length; i++)
                {
                    if (JointMap.ContainsKey(i))
                    {
                        KinectWrapper.NuiSkeletonPositionIndex joint = !mirror ? JointMap[i] : MirroredJointMap[i];
                        TransformBone(joint, i, !mirror);
                    }
                    else if (SpecJointMap.ContainsKey(i))
                    {
                        continue;
                        var joint = !mirror ? SpecJointMap[i] : SpecMirroredJointMap[i];
                        Vector3 baseDir = joint[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
                        TransformSpecialBone(joint[0], joint[1], i, baseDir, !mirror);
                    }
                }
            }
        }
    }

    void TransformBone(KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {

        Transform boneTransform = Bones[boneIndex];
        if (boneTransform == null || manager == null)
            return;

        int iJoint = (int)joint;
        if (iJoint < 0)
            return;

        // Get Kinect joint orientation
        Quaternion jointRotation = manager.GetJointOrientation(manager.GetPlayer1ID(), iJoint, flip);

        if (joint.ToString().Contains("Shoulder1"))
        {
            return;
            var v = jointRotation.eulerAngles;
            jointRotation = Quaternion.Euler(v.z, v.x - 90 - 90, v.y - 90);
        }
        if (jointRotation == Quaternion.identity)
            return;

        // Smoothly transition to the new rotation
        Quaternion newRotation = jointRotation * InitialRots[boneIndex];
        if (OffsetNode != null)
        {
            // Grab the total rotation by adding the Euler and offset's Euler.
            Vector3 totalRotation = newRotation.eulerAngles + OffsetNode.transform.rotation.eulerAngles;
            // Grab our new rotation.
            newRotation = Quaternion.Euler(totalRotation);
        }

        boneTransform.rotation = newRotation;
    }

    void TransformSpecialBone(KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
    {
        Transform boneTransform = Bones[boneIndex];
        if (boneTransform == null || manager == null)
            return;

        if (!manager.IsJointTracked(manager.GetPlayer1ID(), (int)joint) ||
           !manager.IsJointTracked(manager.GetPlayer1ID(), (int)jointParent))
        {
            return;
        }

        Vector3 jointDir = manager.GetDirectionBetweenJoints(manager.GetPlayer1ID(), (int)jointParent, (int)joint, false, true);
        Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;

        //		if(!flip)
        //		{
        //			Vector3 mirroredAngles = jointRotation.eulerAngles;
        //			mirroredAngles.y = -mirroredAngles.y;
        //			mirroredAngles.z = -mirroredAngles.z;
        //			
        //			jointRotation = Quaternion.Euler(mirroredAngles);
        //		}

        if (jointRotation != Quaternion.identity)
        {
            // Smoothly transition to the new rotation
            Quaternion newRotation = jointRotation * InitialRots[boneIndex];
            if (OffsetNode != null)
            {
                // Grab the total rotation by adding the Euler and offset's Euler.
                Vector3 totalRotation = newRotation.eulerAngles + OffsetNode.transform.rotation.eulerAngles;
                // Grab our new rotation.
                newRotation = Quaternion.Euler(totalRotation);
            }

            boneTransform.rotation = newRotation;
        }
    }

    readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {3, KinectWrapper.NuiSkeletonPositionIndex.Head},

        {5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {6, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {7, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        {8, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},

        {10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {11, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {12, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        {13, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
    };

    readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> MirroredJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
    {
        {0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
        {1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
        {2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
        {3, KinectWrapper.NuiSkeletonPositionIndex.Head},

        {5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
        {6, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
        {7, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
        {8, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
        {11, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
        {12, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
        {13, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootRight},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
    };

    readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> SpecJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };

    readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> SpecMirroredJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
    {
        {4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
        {9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
    };
}
