using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerMoveType {
    STRAIGHT,                //  左右控制移动
    ROTATE                  // 左右控制旋转
}

public class PlayerController : MonoBehaviour {
    public Vector3 defaultPlayPos = new Vector3(0, 0 ,0);       // player默认位置
    public float moveSpeed = 10;                                    // 移动速度
    public float rotateSpeed = 100;                                    // 旋转速度
    private Transform cameraTf;                                  // 镜头
    private Vector3 rotateAngle = new Vector3(0, 0, 0);             // 旋转角度
    private int mtX = 0;                    // -1 向左，0不动，1向右
    private int mtY = 0;
    private int mtZ = 0;
    
    private bool pressLeft = false;
    private bool pressRight = false;
    private bool pressUp = false;
    private bool pressDown = false;
    private bool _setDefaultPos = false;
    // Start is called before the first frame update

    // 工具方法 begin

    // 工具方法 end 

    // 事件监听 start
    private void RegisterEvent(){
        MouseEventMgr.Instance.OnMousePressedR(MouseEventType.rotatePlayer, KeyCode.Mouse1, OnMouse1Pressed);
    }

    private void OnMouse1Pressed(Vector3 mouseMovement){
        PlayerRotateWithMouse(mouseMovement);
    }
    // 事件监听 end 

    // player控制 begin 
    private void PlayerMove(PlayerMoveType moveType)
    {
        switch(moveType)
        {
            case PlayerMoveType.STRAIGHT:
                PlayerMoveStraight();
                break;
            case PlayerMoveType.ROTATE:
                PlayerMoveRotate();
                break;
            default:
                Debug.LogError("wrong moveType: " + moveType);
                break;
        }
    }
    private void PlayerMoveStraight(){
        // mtX, mtZ控制平移
        MovePlayer(mtX, mtY, mtZ);
        // 镜头跟随
        // Debug.Log("moveDis=" + moveDis.x + "," + moveDis.y + ", " + moveDis.z);
    }

    private void PlayerMoveRotate(){
        // mtX控制旋转, mtZ控制前进
        PlayerRotateWithKeyboard();
        PlayerMoveForward();
    }

    // 键盘控制player旋转
    private void PlayerRotateWithKeyboard(){
        if(0 == mtX){
            return;
        }
        // float x = 0;
        float y = rotateSpeed * Time.deltaTime * mtX;
        // float z = 0;
        rotateAngle.Set(0, y, 0);
        transform.Rotate(rotateAngle, Space.World);
        // 旋转镜头
        PlayerCameraController.Instance.RotateWithPlayer(rotateAngle); 
    }

    // 鼠标控制player旋转
    private void PlayerRotateWithMouse(Vector3 mouseMovement){
        rotateAngle.Set(0, mouseMovement.x, 0);
        transform.Rotate(rotateAngle, Space.World);
        // 旋转镜头
        PlayerCameraController.Instance.RotateWithPlayer(rotateAngle); 
    }
    // 控制player向前移动
    private void PlayerMoveForward(){
        if(0 == mtZ){
            return;
        }
        float dx = mtZ * Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad);
        float dy = mtZ * Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad);
        float dz = mtZ * Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad);
        MovePlayer(dx, dy, dz);
    }
    // player控制 end

    // Player工具 start
    private void MovePlayer(float dx, float dy, float dz){
        float x = moveSpeed * Time.deltaTime * dx;
        float y = moveSpeed * Time.deltaTime * dy;
        float z = moveSpeed * Time.deltaTime * dz;
        transform.Translate(x, y, z, Space.World);
        PlayerCameraController.Instance.Move(x, y, z);
        // cameraTf.Translate(x, y, z, Space.World);
    }
    // Player工具 end 

    // 按键响应 begin
    private void KeyPressDown(){
        if(Input.GetKeyDown(KeyCode.A)){
            pressLeft = true;
            if(1 != mtX) {
                mtX = -1;
            }
        }
        if(Input.GetKeyDown(KeyCode.D)){
            pressRight = true;
            if(-1 != mtX){
                mtX = 1;
            }
        }
        if(Input.GetKeyDown(KeyCode.W)){
            pressUp = true;
            if(-1 != mtY){
                mtZ = 1;
            }
        }
        if(Input.GetKeyDown(KeyCode.S)){
            pressDown = true;
            if(1 != mtY){
                mtZ = -1;
            }
        }
    }

    private void KeyReleaseUp(){
        if(Input.GetKeyUp(KeyCode.A)){
            pressLeft = false;
            mtX = pressRight ? 1 : 0;
        }
        if(Input.GetKeyUp(KeyCode.D)){
            pressRight = false;
            mtX = pressLeft ? -1 : 0;
        }
        if(Input.GetKeyUp(KeyCode.W)){
            pressUp = false;
            mtZ = pressDown ? -1 : 0;
        }
        if(Input.GetKeyUp(KeyCode.S)){
            pressDown = false;
            mtZ = pressUp ? 1 : 0;
        }
    }

    private void SetMoveDir(){
        // Debug.Log((int)keyDown + ", " + (char)keyDown);
        if(Input.anyKeyDown){
            KeyPressDown(); 
        }
        KeyReleaseUp();
    }
    // 按键响应 end

    // 初始化 start
    private void InitVars(){
        cameraTf = GameObject.Find("playerCamera").transform;
        if(null == cameraTf){
            Debug.LogError("can't find playerCamera");
        }
    }

    private void SetDefaultPos(){
        if(_setDefaultPos)
            return;
        _setDefaultPos = true;
        PlayerRotateWithMouse(Vector3.zero);
    }

    void Start(){
        InitVars(); 
    }

    void OnEnable(){
        RegisterEvent();
    }
    // 初始化 end

    void OnGUI(){
        SetMoveDir();
    }

    // Update is called once per frame
    void Update(){
        SetDefaultPos();
        PlayerMove(PlayerMoveType.ROTATE);
    }

    void LateUpdate(){
        // SetCameraPosRotation();
    }
}
