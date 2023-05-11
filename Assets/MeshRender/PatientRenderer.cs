using Dummiesman;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class PatientRenderer : MonoBehaviour
{
    //define obj path
    const string obj_filename = "giovanni2.obj";
    string obj_path = "http://10.196.170.178:8000/giovanni/" + obj_filename;

    //define mtl path
    const string mtl_filename = "giovanni2.obj.mtl";
    string mtl_path = "http://10.196.170.178:8000/giovanni/" + mtl_filename;

    //define texture path
    const string texture_filename = "test_text.png";
    string texture_path = "http://10.196.170.178:8000/giovanni/" + texture_filename;

    void Start()
    {
        LoadPatient();

    }

    public void LoadPatient()
    {
 
        StartCoroutine(download_mtl());
        StartCoroutine(download_texture());
        StartCoroutine(download_obj());
        System.Threading.Thread.Sleep(1);
        var loadedObj = new OBJLoader().Load(Application.persistentDataPath + "/" + obj_filename, Application.persistentDataPath + "/" + mtl_filename);
        // Get the renderer component of the gameobject
        Renderer renderer = loadedObj.GetComponentInChildren<Renderer>();
        // Get the current material of the gameobject
        Material material = renderer.material;
        // Create a new shader
        Shader newShader = Shader.Find("UI/Prerendered Opaque");
        // Assign the new shader to the material
        material.shader = newShader;
        loadedObj.transform.position = new Vector3(0, 1, 0);
        loadedObj.transform.rotation = Quaternion.Euler(0, 180, 0);;
    }
    IEnumerator download_obj()
    {
        WWW www = new WWW(obj_path);
        yield return www;
        File.WriteAllBytes(Application.persistentDataPath + "/" + obj_filename, www.bytes);
        Debug.Log("Resource saved at: " + Application.persistentDataPath + "/" + obj_filename);
    }
    IEnumerator download_mtl()
    {
        WWW www = new WWW(mtl_path);
        yield return www;
        File.WriteAllBytes(Application.persistentDataPath + "/" + mtl_filename, www.bytes);
        Debug.Log("Resource saved at: " + Application.persistentDataPath + "/" + mtl_filename);
    }
    IEnumerator download_texture()
    {
        WWW www = new WWW(texture_path);
        yield return www;
        File.WriteAllBytes(Application.persistentDataPath + "/" + texture_filename, www.bytes);
        Debug.Log("Resource saved at: " + Application.persistentDataPath + "/" + texture_filename);
    }
}
