using System;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using UnityEngine;

public class TargetPoseSender : MonoBehaviour
{

    // Variables required for ROS communication
    [SerializeField]
    //string m_TopicName = "/ur5e_unity_joint_state";
    private string m_TopicName = "target_frame";
    [SerializeField]
    public GameObject Target;

    // ROS Connector
    ROSConnection m_Ros;
    public float publishMessageFrequency =0.016666667f;
    private float timeElapsed;

    void Start()
    {
        timeElapsed=0f;
         //Get ROS connection static instance
        m_Ros = ROSConnection.GetOrCreateInstance();
        m_Ros.RegisterPublisher<PoseStampedMsg>(m_TopicName);
    }

    public void Publish()
    {
        var pose_msg = new PoseStampedMsg();
        Vector3 position = Target.transform.position;
        position.y -= 1.0f;
        pose_msg.pose.position = position.To<FLU>();
        pose_msg.pose.orientation = Target.transform.rotation.To<FLU>();

        pose_msg.header.frame_id="base_link";
        m_Ros.Publish(m_TopicName, pose_msg);
    }
    void  Update() 
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed > publishMessageFrequency)
        {
    	    Publish();
            timeElapsed=0;
        }
    }

    void check_target_pos()
    {
        //limit x position
        if (Target.transform.position.x > 0.9)
        {
            Target.transform.position =
                new Vector3(0.51f,
                    Target.transform.position.y,
                    Target.transform.position.z);
        }
        if (Target.transform.position.x < -0.9)
        {
            Target.transform.position =
                new Vector3(-0.51f,
                    Target.transform.position.y,
                    Target.transform.position.z);
        }

        //limit y position
        if (Target.transform.position.y > 1.3)
        {
            Target.transform.position =
                new Vector3(Target.transform.position.x,
                    1.3f,
                    Target.transform.position.z);
        }
        if (Target.transform.position.y < 0.85)
        {
            Target.transform.position =
                new Vector3(Target.transform.position.x,
                    0.85f,
                    Target.transform.position.z);
        }

        //limit z position
        if (Target.transform.position.z > 0.05)
        {
            Target.transform.position =
                new Vector3(Target.transform.position.x,
                    Target.transform.position.y,
                    0.05f);
        }
        if (Target.transform.position.z < -0.46)
        {
            Target.transform.position =
                new Vector3(Target.transform.position.x,
                    Target.transform.position.y,
                    -0.46f);
        }
    }
}