using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController{
    // 常量
    private const string PATH_GO_PLAYER = "World/Player";
    private const string PATH_GO_PLAYER_CAMERA = "World/playerCamera";
    public float pcHeightDis = 500f;                             // 摄像机与player高度差
    public float pcHoriDis = 500f;                                        // 摄像机与player水平距离
    public float pcDis = 600f;
    private Transform playerTf;
    private Transform cameraTf;
    private float rotateSensitivity = 0.1f;                       // 右键拖拽镜头旋转灵敏度

    private static PlayerCameraController _instance = null;
    public static PlayerCameraController Instance{
        get{ return _instance ?? (_instance = new PlayerCameraController());}
    }
    private PlayerCameraController(){}
    // Start is called before the first frame update

    // 初始化 start
    public void Init()
    {
        InitVars();
        RegisterEvent();
    }

    private void InitVars(){
        playerTf = GameObject.Find(PATH_GO_PLAYER).transform;
        cameraTf = GameObject.Find(PATH_GO_PLAYER_CAMERA).transform;
        if(null == cameraTf){
            Debug.LogError("can't find camera");
        }
        else{
            Debug.Log("init camera successfully");
        }
    }

    private void RegisterEvent(){
    }
    // 初始化 end 

    // 镜头控制 start
    /// <summary>
    /// 镜头角度控制
    /// </summary>
    /// <param name="rotateAngle">player旋转角度</param>
    public void RotateWithMouse(Vector3 mouseMovement){
        // 获取当前镜头与player距离
        Vector3 pcDis = playerTf.position - cameraTf.position;
        float dh = pcDis.x * Mathf.Acos(pcDis.z / pcDis.x);         // 镜头与player水平距离
        float dv = pcDis.y;                                         // 镜头与player竖直距离

        Debug.LogAssertion(0 == mouseMovement.z);
        float ry = mouseMovement.magnitude / dh;
        Vector3 rotation = new Vector3(0, ry, 0);
        if(0.0001 >= dh){
            cameraTf.Rotate(rotation, Space.World);
            return;
        }
        cameraTf.Rotate(-rotation, Space.World);

        float yR = playerTf.eulerAngles.y;
        float xP = pcHoriDis * Mathf.Sin(yR * Mathf.Deg2Rad);
        float yP = -pcHeightDis;
        float zP = pcHoriDis * Mathf.Cos(yR * Mathf.Deg2Rad);
        cameraTf.position = playerTf.position - new Vector3(xP, yP, zP);
        cameraTf.LookAt(playerTf);
    }

    public void RotateWithPlayer(Vector3 playerRoAngle){
        // if(0.0001 >= pcHoriDis){
        //     cameraTf.Rotate(playerRoAngle, Space.World);
        //     return;
        // }
        // float xR = playerTf.eulerAngles.x;
        // float yR = playerTf.eulerAngles.y;
        // float zR = playerTf.eulerAngles.z;

        // float xzR = Mathf.Pow((Mathf.Pow(xR, 2) + Mathf.Pow(zR, 2)), 0.5f);
        // float xzDis = pcDis * Mathf.Sin(xzR);
        // float dy = pcDis * Mathf.Cos(xzR);
        // float dx = xzDis * Mathf.Sin(playerTf.eulerAngles.y);
        // float dz = xzDis * Mathf.Cos(playerTf.eulerAngles.y);

        // cameraTf.Rotate(-playerRoAngle, Space.World);
        // cameraTf.position = playerTf.position + new Vector3(dx, dy, dz);
        // cameraTf.LookAt(playerTf);    

        if(0.0001 >= pcHoriDis){
            cameraTf.Rotate(playerRoAngle, Space.World);
            return;
        }
        cameraTf.Rotate(-playerRoAngle, Space.World);

        float yR = playerTf.eulerAngles.y;
        float xP = pcHoriDis * Mathf.Sin(yR * Mathf.Deg2Rad);
        float yP = -pcHeightDis;
        float zP = pcHoriDis * Mathf.Cos(yR * Mathf.Deg2Rad);
        cameraTf.position = playerTf.position - new Vector3(xP, yP, zP);
        cameraTf.LookAt(playerTf);
    }

    public void Move(Vector3 movement){
        cameraTf.Translate(movement, Space.World);
    }
    public void Move(float x, float y, float z){
        cameraTf.Translate(x, y, z, Space.World);
    }

    // 鼠标控制镜头 start
    
    // 鼠标控制镜头 end 

    // 镜头控制 end 
}
