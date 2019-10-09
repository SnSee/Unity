using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController: MonoBehaviour{
    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log("WorldController.Start");
        // 初始化player镜头控制器
        PlayerCameraController.Instance.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
