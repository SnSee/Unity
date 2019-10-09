using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class FileDialog : MonoBehaviour
{
    private List<GameObject> dirGoList = new List<GameObject>();
    private void RegisterBtnClickEvent()
    {
        transform.GetComponent<Button>().onClick.AddListener(delegate(){
            GameObject textGo = transform.parent.Find("Dirs/Button").gameObject;
            dirGoList.Add(textGo);
            DirectoryInfo[] dirs = new DirectoryInfo("/tmp").GetDirectories();
            FileInfo[] files = new DirectoryInfo("/tmp").GetFiles();
            for(int i = 0; i < dirs.Length; ++i) {
                if(i >= dirGoList.Count){
                    dirGoList.Add(Instantiate(textGo, textGo.transform.parent));
                }
                dirGoList[i].transform.Find("Text").GetComponent<Text>().text = dirs[i].Name;
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        RegisterBtnClickEvent(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
