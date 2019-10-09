using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 鼠标事件回调函数类型
// 在鼠标按下和抬起事件中v3代表点击位置，在按住事件中代表鼠标在相邻帧的位移
public delegate void MeCallback(Vector3 v3);
public enum MouseEventType{
    rotatePlayer = 0
}

public class MouseEventMgr: MonoBehaviour{
    enum KeyActionType{
        Down = 0,                   // 按下
        Pressed = 1,                // 按住
        Up = 2                      // 松开
    }
    // 鼠标左键事件处理函数
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic00 = new Dictionary<MouseEventType, List<MeCallback>>(); // 左键按下
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic01 = new Dictionary<MouseEventType, List<MeCallback>>(); // 左键按住
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic02 = new Dictionary<MouseEventType, List<MeCallback>>(); // 左键松开
    // 鼠标右键处理函数
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic10 = new Dictionary<MouseEventType, List<MeCallback>>(); // 右键按下
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic11 = new Dictionary<MouseEventType, List<MeCallback>>(); // 右键按住
    private Dictionary<MouseEventType, List<MeCallback>> handlerDic12 = new Dictionary<MouseEventType, List<MeCallback>>(); // 右键松开
    // 鼠标中键处理函数
    // private Dictionary<MouseEventType, MeCack> handlerDic2 = new Dictionary<MouseEventType, MeCallback>();
    List<Dictionary<MouseEventType, List<MeCallback>>> dicPointer = new List<Dictionary<MouseEventType, List<MeCallback>>>();
    private bool _inited = false;           // 用于标记是否已经初始化
    private Vector3 lastMousePosition;      // 上一帧鼠标的位置
    private Vector3 mouseMovement = Vector3.zero;          // 上一帧到这一帧鼠标移动的距离
    private bool[] mousePressed = {false, false, false};    // 是否按住鼠标左键，右键，中键

    private static MouseEventMgr _instance = null;
    public static MouseEventMgr Instance{
        get{return _instance;}
    }
    private MouseEventMgr(){}

    // 注册鼠标事件处理函数
    // 注册鼠标按下回调
    public void OnMouseDownR(MouseEventType mouseEventType, KeyCode keyCode, MeCallback meCallback){
        int dicIndex = GetKeyEventIndex(keyCode, KeyActionType.Down);
        _RegisterMouseEvent(dicIndex, mouseEventType, meCallback);
    }
    // 取消鼠标按下注册
    public void OnMouseDownU(MouseEventType mouseEventType, KeyCode keyCode, MeCallback meCallback){
        int dicIndex = GetKeyEventIndex(keyCode, KeyActionType.Down);
        _UnRegisterMouseEvent(dicIndex, mouseEventType, meCallback);
    }

    // 注册鼠标按住
    public void OnMousePressedR(MouseEventType mouseEventType, KeyCode keyCode, MeCallback callback){
        int dicIndex = GetKeyEventIndex(keyCode, KeyActionType.Pressed);
        _RegisterMouseEvent(dicIndex, mouseEventType, callback);
    }

    public void OnMousePressedU(MouseEventType mouseEventType, KeyCode keyCode, MeCallback meCallback){
        int dicIndex = GetKeyEventIndex(keyCode, KeyActionType.Pressed);
        _UnRegisterMouseEvent(dicIndex, mouseEventType, meCallback);
    }

    // 注册鼠标事件回调函数
    private void _RegisterMouseEvent(int dicIndex, MouseEventType mouseEventType, MeCallback callback){
        // 同一个回调函数只能注册一次
        bool flag = false;
        foreach(MouseEventType _meType in dicPointer[dicIndex].Keys){
            if(_meType == mouseEventType){
                flag = true;
                break;
            }
        }
        if(!flag){
            dicPointer[dicIndex].Add(mouseEventType, new List<MeCallback>());
        }
        MeCallback meCallback = (MeCallback)callback;
        if(dicPointer[dicIndex][mouseEventType].Contains(meCallback)){
            Debug.LogError("Mouse event handler has already been registered");
            return;
        }
        dicPointer[dicIndex][mouseEventType].Add(meCallback);
    }

    // 取消鼠标事件回调函数
    private void _UnRegisterMouseEvent(int dicIndex, MouseEventType mouseEventType, MeCallback meCallback){
        bool flag = false;
        foreach(MouseEventType _meType in dicPointer[dicIndex].Keys){
            if(_meType == mouseEventType){
                flag = true;
                break;
            }
        }
        if(flag){
            dicPointer[dicIndex][mouseEventType].Remove(meCallback);
        }
        else{
            Debug.LogError("Mouse event handler has not been registered");
        }
    }

    // 执行鼠标事件回调函数
    private void ExecuteMouseEventCallback(int dicIndex, Vector3 clickPos, KeyActionType actionType){
        foreach(var callbackList in dicPointer[dicIndex].Values){
            foreach(MeCallback _callback in callbackList){
                switch(actionType){
                    case KeyActionType.Down:
                        _callback(clickPos);
                        break;
                    case KeyActionType.Pressed:
                        _callback(mouseMovement);
                        break;
                    case KeyActionType.Up:
                        _callback(clickPos);
                        break;
                }
            }
        }
    }

    // 获取按键事件对应的字典索引
    /// <summary>
    /// 获取鼠标按键对应的dic索引
    /// </summary>
    /// <param name="keyCode">哪个鼠标按键</param>
    /// <param name="actionType">动作</param>
    /// <returns></returns>
    private int GetKeyEventIndex(KeyCode keyCode, KeyActionType actionType){
        return 3 * ((int)keyCode - (int)KeyCode.Mouse0) + (int)actionType;
    }

    // 监听鼠标事件
    private void _MonitorMouseDownEvent(KeyCode keyCode, KeyActionType actionType){
        if(Input.GetKeyDown(keyCode)){
            mousePressed[keyCode - KeyCode.Mouse0] = true;
            ExecuteMouseEventCallback(GetKeyEventIndex(keyCode, actionType), Input.mousePosition, actionType);
        }
    }
    private void _MonitorMousePressedEvent(KeyCode keyCode, KeyActionType actionType){
        if(mousePressed[keyCode - KeyCode.Mouse0]){
            ExecuteMouseEventCallback(GetKeyEventIndex(keyCode, actionType), Input.mousePosition, actionType);
        }
    }
    private void _MonitorMouseUpEvent(KeyCode keyCode, KeyActionType actionType){
        if(Input.GetKeyUp(keyCode)){
            mousePressed[keyCode - KeyCode.Mouse0] = false;
            ExecuteMouseEventCallback(GetKeyEventIndex(keyCode, actionType), Input.mousePosition, actionType);
        }
    }
    private void MonitorMouseEvent(){
        if(null != lastMousePosition)
            mouseMovement = Input.mousePosition - lastMousePosition;

        _MonitorMouseDownEvent(KeyCode.Mouse0, KeyActionType.Down);
        _MonitorMousePressedEvent(KeyCode.Mouse0, KeyActionType.Pressed);
        _MonitorMouseUpEvent(KeyCode.Mouse0, KeyActionType.Up);

        _MonitorMouseDownEvent(KeyCode.Mouse1, KeyActionType.Down);
        _MonitorMousePressedEvent(KeyCode.Mouse1, KeyActionType.Pressed);
        _MonitorMouseUpEvent(KeyCode.Mouse1, KeyActionType.Up);
        lastMousePosition = Input.mousePosition;
    }

    // 初始化变量
    void InitDicPointer(){
        dicPointer.Add(handlerDic00);
        dicPointer.Add(handlerDic01);
        dicPointer.Add(handlerDic02);
        dicPointer.Add(handlerDic10);
        dicPointer.Add(handlerDic11);
        dicPointer.Add(handlerDic12);
    }

    void Init(){
        lock (this)
        {
            if (_inited)
                return;
            _inited = true;
            InitDicPointer();
        }
    }

    void Awake(){
        _instance = this;
        Init();
    }

    void Start(){
    }

    // Update is called once per frame
    void Update(){ 
        MonitorMouseEvent();
    }
}
