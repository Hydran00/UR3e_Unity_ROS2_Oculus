using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using UnityEngine;

//using Oculus.VR;
public class ForceFeedback : MonoBehaviour
{
    private float force_threshold = 1f;
    private float vibration_multiplier = 0.1f;

    private float freq = 1 / 30;
    private float time_passed = 0f;

    public GameObject arrow;
    private bool was_pressed = false;
    private GameObject[] targets;
    private int index = 0;
    private const int max_waypoints = 30;
    private int current_frames_between_msgs = 0;
    private bool already_sent = false;

    private int max_num_of_waypoints_to_visualize = 30;

    string filePath = @"Assets/Ivysaur_OBJ/Pokemon.obj";


    ROSConnection ros;

    List<GameObject> spline_waypoints;
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        //ros.Subscribe<WrenchStampedMsg>("/force_torque_sensor_broadcaster/wrench", callback1);
        ros.Subscribe<PoseArrayMsg>("/spline", callback2);
        ros.RegisterPublisher<PoseArrayMsg>("/desired_waypoints");
        ros.RegisterPublisher<BoolMsg>("/execute_spline_trajectory");
        targets = new GameObject[max_waypoints];
        spline_waypoints = new List<GameObject>();
    }

    void Update()
    {
        //var cube = GameObject.Find("Target");
        //cube.GetComponent<TargetPoseSender>().enabled = false;
        //Vector3 pos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        //Quaternion ori = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        if (index >= max_waypoints)
        {
            return;
        }
        if (set_waypoint_button_pressed() && !was_pressed)
        {
            was_pressed = true;
            Debug.Log("Button pressed at time" + Time.time);
            targets[index] = (GameObject)Instantiate(
                arrow,
                GameObject.Find("RightHandAnchor").transform.position,
                GameObject.Find("RightHandAnchor").transform.rotation * Quaternion.Euler(0, 270, 180));
            Debug.Log("Placing in " + targets[index].transform.position + "   " + targets[index].transform.rotation);
            targets[index].transform.parent = GameObject.Find("RightHandAnchor").transform;
            targets[index].transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            return;
        }
        if (!set_waypoint_button_pressed() && was_pressed)
        {
            was_pressed = false;
            if (GameObject.Find("RightHandAnchor/frame(Clone)") == null)
            {
                Debug.Log("arrow not found");
                return;
            }
            else
            {
                Debug.Log("arrow is active");
            }
            //Debug.Log("button released -> placing in " + pos + " " + ori);

            targets[index].transform.parent = GameObject.Find("Waypoints").transform;

            index += 1;
            return;
        }
        if (send_waypoint_button_pressed() && index > 0)
        {
            Debug.Log("Sending " + index + " waypoints");
            PoseArrayMsg poses = new PoseArrayMsg();
            poses.header.frame_id = "base_link";
            //poses.header.stamp = ros.GetTime();
            poses.poses = new PoseMsg[index];
            for (int i = 0; i < index; i++)
            {
                PoseMsg pose = new PoseMsg();
                pose.position = targets[i].transform.position.To<FLU>();
                pose.orientation = targets[i].transform.rotation.To<FLU>();
                Debug.Log("Sending " + pose.orientation.x + " " + pose.orientation.y + " " + pose.orientation.z + " " + pose.orientation.w);
                poses.poses[i] = pose;
            }
            ros.Publish("/desired_waypoints", poses);
            return;
        }
        if (delete_spline_button_pressed())
        {
            Debug.Log("Deleting spline");
            foreach (GameObject waypoint in spline_waypoints)
            {
                Destroy(waypoint);
            }
            spline_waypoints.Clear();
            foreach (GameObject target in targets)
            {
                Destroy(target);
                index = 0;
            }
            return;
        }
        if (execute_trajectory_button_pressed())
        {
            if (index < 1)
            {
                Debug.Log("ERROR: At least 1 waypoint is needed to execute a trajectory");
                return;
            }
            else
            {
                Debug.Log("Executing trajectory");
                var msg = new BoolMsg();
                msg.data = true;
                ros.Publish("/execute_spline_trajectory", msg);
            }

        }

    }
    bool set_waypoint_button_pressed()
    {
        if (Application.isEditor)
        {
            return Input.GetKey(KeyCode.A);
        }
        else
        {
            return OVRInput.Get(OVRInput.Button.One);
        }
    }
    bool send_waypoint_button_pressed()
    {
        if (Application.isEditor)
        {
            return Input.GetKeyDown(KeyCode.B);
        }
        else
        {
            return OVRInput.GetDown(OVRInput.Button.Two);
        }
    }
    bool delete_spline_button_pressed()
    {
        if (Application.isEditor)
        {
            return Input.GetKeyDown(KeyCode.Q);
        }
        else
        {
            return (0.5 < OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch));
        }
    }
    bool execute_trajectory_button_pressed()
    {
        if (Application.isEditor)
        {
            return Input.GetKeyDown(KeyCode.C);
        }
        else
        {
            return (0.5 < OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch));
        }
    }

    void callback1(WrenchStampedMsg feedback)
    {
        if (time_passed < freq)
        {
            time_passed += Time.deltaTime;
            return;
        }
        Vector3 force = feedback.wrench.force.From<FLU>();
        float force_module = Mathf.Sqrt(force.x * force.x + force.y * force.y + force.z * force.z);
        float frequency = 1;
        //Scaling force into vibration, which is in [0,1]
        float amplitude = Mathf.Abs(force_module * vibration_multiplier);
        if (amplitude > 1)
        {
            amplitude = 1;
        }
        //Deactivate vibration when  -threshold < force < threshold 
        if (force_module > -force_threshold && force_module < force_threshold)
        {
            amplitude = 0;
            frequency = 0;
        }
        OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
    }
    void callback2(PoseArrayMsg poses)
    {
        Debug.Log("Received trajectory, now you can press trigger button to execute the trajectory");
        StartCoroutine(spawn_trajectory_visualisation(poses));

    }
    private IEnumerator spawn_trajectory_visualisation(PoseArrayMsg poses)
    {
        int arrow_per_way_point = poses.poses.Length / max_num_of_waypoints_to_visualize;

        for (var i = 0; i < poses.poses.Length; i++)
        {
            if (arrow_per_way_point > 0 && i % arrow_per_way_point != 0)
            {
                continue;
            }
            var pose = poses.poses[i];
            var position = pose.position.From<FLU>();
            Debug.Log("Received " + pose.orientation.x + " " + pose.orientation.y + " " + pose.orientation.z + " " + pose.orientation.w);
            var orientation = pose.orientation.From<FLU>();
            var waypoint_arrow = (GameObject)Instantiate(arrow, position, orientation);
            waypoint_arrow.transform.parent = GameObject.Find("Waypoints").transform;
            waypoint_arrow.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            spline_waypoints.Add(waypoint_arrow);
        }
        yield return null;
    }

}
