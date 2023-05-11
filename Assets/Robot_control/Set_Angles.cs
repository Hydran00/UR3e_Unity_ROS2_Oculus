using UnityEngine;
//using RosMessageTypes.Std;
using RosMessageTypes.Sensor;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using System.Collections.Generic;

using Unity.Robotics.UrdfImporter.Control;

public class Set_Angles : MonoBehaviour
{

    private ArticulationBody[] articulationChain;

    private bool first_callback;
    ArticulationBody[] JointList;

    public static Dictionary<string, int> joints_order_in_msg;
    //The old controller
    void Start()
    {
        first_callback = true;
        ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>("/joint_states", callback);
        articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        JointList = new ArticulationBody[6];
        joints_order_in_msg = new Dictionary<string, int>();
        //Get joints from Unity scene
        foreach (ArticulationBody joint in articulationChain)
        {
            switch (joint.name)
            {
                case "shoulder_link":
                    JointList[0] = joint;
                    break;
                case "upper_arm_link":
                    JointList[1] = joint;
                    break;
                case "forearm_link":
                    JointList[2] = joint;
                    break;
                case "wrist_1_link":
                    JointList[3] = joint;
                    break;
                case "wrist_2_link":
                    JointList[4] = joint;
                    break;
                case "wrist_3_link":
                    JointList[5] = joint;
                    break;
                default:
                    break;
            }
        }
    }
    double Rad2Deg(double radx)
    {
        return radx * (180 / 3.141592653589793238463);
    }

    //set angles reading real robot position   
    void callback(JointStateMsg state_msg)
    {
        if (first_callback)
        {
            prepare_dictionary(state_msg);
            first_callback = false;
        }
        int i = 0;
        foreach (ArticulationBody joint in JointList)
        {
            ArticulationDrive currentDrive = joint.xDrive;

            currentDrive.target = (float)Rad2Deg(state_msg.position[joints_order_in_msg[joint.name]]);
            joint.xDrive = currentDrive;
            i++;
        }
    }
    //since sometimes order of joints is different, I use dictionary to easily access 
    //a specific joint position in the array given his name
    void prepare_dictionary(JointStateMsg state_msg)
    {
        for (var i = 0; i < 6; i++)
        {
            string joint_names = state_msg.name[i];
            switch (joint_names)
            {
                case "shoulder_pan_joint":
                    joints_order_in_msg.Add("shoulder_link",i);
                    break;
                case "shoulder_lift_joint":
                    joints_order_in_msg.Add("upper_arm_link",i);
                    break;
                case "elbow_joint":
                    joints_order_in_msg.Add("forearm_link",i);
                    break;
                case "wrist_1_joint":
                    joints_order_in_msg.Add("wrist_1_link",i);
                    break;
                case "wrist_2_joint":
                    joints_order_in_msg.Add("wrist_2_link",i);
                    break;
                case "wrist_3_joint":
                    joints_order_in_msg.Add("wrist_3_link",i);
                    break;
                default:
                    break;
            }
        }
    }


}

