using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// 弧形文字文本框组件
public class ArcText : Text 
{
    public float curvedRadius;
    public int curvedFontSpaceScale;

    /// <summary>
    /// 在Unity生成顶点数据后会调用这个函数，
    /// 在这个函数中改变顶点坐标，达到改变mesh形状的目的
    /// toFill保存了Mesh的所有信息
    /// </summary>
    /// <param name="toFill"></param>
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        curvedRadius = 1500f;
        curvedFontSpaceScale = 1;
        CurvedText(toFill);
        Debug.Log("SpaceScale =" + curvedFontSpaceScale + ", radius=" + curvedRadius);
    }

    private void PrintVertexInfo(VertexHelper helper)
    {
        if(cachedTextGenerator.lines.Count <= 0)
        {
            return;
        }
        int index = 0;
        UIVertex lb = new UIVertex();
        helper.PopulateUIVertex(ref lb, index * 4);

        UIVertex lt = new UIVertex();
        helper.PopulateUIVertex(ref lt, index * 4 + 1);          //lb 左下  lt左上  rt 右上 ，rb右下

        UIVertex rt = new UIVertex();
        helper.PopulateUIVertex(ref rt, index * 4 + 2);

        UIVertex rb = new UIVertex();
        helper.PopulateUIVertex(ref rb, index * 4 + 3);

        Debug.Log("vertex info" + lb.position.x + "," + lb.position.y);
        Debug.Log("vertex info" + lt.position.x + "," + lt.position.y);
        Debug.Log("vertex info" + rt.position.x + "," + rt.position.y);
        Debug.Log("vertex info" + rb.position.x + "," + rb.position.y);
    }

    private void CurvedText(VertexHelper toFill)
    {
        if (!IsActive())    //处于未激活状态
            return;
        for (int i = 0; i < cachedTextGenerator.lines.Count; i++)     //遍历所有行
        {
            UILineInfo line = cachedTextGenerator.lines[i];       //当前行
            if (i + 1 < cachedTextGenerator.lines.Count)       //不是最后一行
            {
                UILineInfo line2 = cachedTextGenerator.lines[i + 1];       //下一行
                int current = 0;   //一行的第几个字
                //遍历一行所有文字 ，下一行起点为界限
                for (int j = line.startCharIdx; j < line2.startCharIdx - 1; j++)
                {
                    CurvedText(toFill, j, current++, i, line2.startCharIdx - 1 - line.startCharIdx);
                }
            }
            else if (i + 1 == cachedTextGenerator.lines.Count)  //最后一行
            {
                int current = 0;
                for (int j = line.startCharIdx; j < cachedTextGenerator.characterCountVisible; j++)  //总字数为界限
                {
                    int index = current;
                    CurvedText(toFill, j, current++, i, cachedTextGenerator.characterCountVisible - line.startCharIdx);
                }
            }
        }
    }

    /// <summary>
    /// 变换字符坐标
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="index">本行当前字符在总文本中的位置索引</param>
    /// <param name="charXIndex">当前字符在当前行的位置, 从0开始</param>
    /// <param name="charYStep">本行行号索引</param>
    /// <param name="lineCount">本行字符个数</param>
    void CurvedText(VertexHelper helper, int index, int charXIndex, int charYStep, int lineCount)
    {
        //获取顶点的信息，一个文字4个顶点组成    
        UIVertex lb = new UIVertex();
        helper.PopulateUIVertex(ref lb, index * 4);

        UIVertex lt = new UIVertex();
        helper.PopulateUIVertex(ref lt, index * 4 + 1);          //lb 左下  lt左上  rt 右上 ，rb右下

        UIVertex rt = new UIVertex();
        helper.PopulateUIVertex(ref rt, index * 4 + 2);

        UIVertex rb = new UIVertex();
        helper.PopulateUIVertex(ref rb, index * 4 + 3);

        Debug.Log("vertex info" + lb.position.x + "," + lb.position.y);
        Debug.Log("vertex info" + lt.position.x + "," + lt.position.y);
        Debug.Log("vertex info" + rt.position.x + "," + rt.position.y);
        Debug.Log("vertex info" + rb.position.x + "," + rb.position.y);

        Vector3 center = Vector3.Lerp(lb.position, rt.position, 0.5f);   //文字的中心点


        float degree = GetAngle(helper, lineCount, text.Split('\n')[charYStep], index - cachedTextGenerator.lines[charYStep].startCharIdx, charYStep);  //获取文字旋转角度

        Matrix4x4 move = Matrix4x4.TRS(-center, Quaternion.identity, Vector3.one);  //变化前 坐标 旋转 缩放矩阵


        float y = curvedRadius - curvedRadius * Mathf.Cos(degree * Mathf.Deg2Rad);  //根据角度计算 y 坐标     curvedRadius文字绕成圆的弧度

        float x = curvedRadius * Mathf.Sin(degree * Mathf.Deg2Rad);   //根据角度计算 x 坐标

        Vector3 pos = new Vector3(x, y, 0);

        Vector3 charadir = (new Vector3(0, curvedRadius, 0) - pos).normalized;   //顶点指向圆中心方向
        pos += lb.position.y * charadir * 0.4f;  //向内移动  文字收缩


        Matrix4x4 place = Matrix4x4.TRS(pos, Quaternion.Euler(new Vector3(0, 0, degree)), Vector3.one);   //变化后 坐标 旋转 缩放矩阵

        Matrix4x4 transform = place * move;  //变化矩阵 之前矩阵基础上 旋转平移 缩放

        lb.position = transform.MultiplyPoint3x4(lb.position);    //根据矩阵旋转4个顶点
        lt.position = transform.MultiplyPoint3x4(lt.position);
        rt.position = transform.MultiplyPoint3x4(rt.position);
        rb.position = transform.MultiplyPoint3x4(rb.position);


        helper.SetUIVertex(lb, index * 4);    //刷新4个顶点
        helper.SetUIVertex(lt, index * 4 + 1);
        helper.SetUIVertex(rt, index * 4 + 2);
        helper.SetUIVertex(rb, index * 4 + 3);
    }

    /// <summary>
    /// 计算字符角度
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="charaCount">当前行字符总数</param>
    /// <param name="tex">当前行所有文字字符串</param>
    /// <param name="index">当前字符在当前行的位置，从0开始</param>
    /// <param name="step">当前行行号索引</param>
    /// <returns></returns>
    private float GetAngle(VertexHelper helper, int charaCount, string tex, int index, int step)
    {
        float totalangle = 0;
        int startindex = cachedTextGenerator.lines[step].startCharIdx;
        for (int i = 0; i < charaCount; i++)
        {
            UIVertex lb = new UIVertex();
            helper.PopulateUIVertex(ref lb, (i + startindex) * 4);

            UIVertex lt = new UIVertex();
            helper.PopulateUIVertex(ref lt, (i + startindex) * 4 + 1);

            float width = Vector3.Distance(lb.position, lt.position);
            float angle = Mathf.Asin(width / 2 / curvedRadius) * 2 * Mathf.Rad2Deg * curvedFontSpaceScale;
            totalangle += angle;
        }

        totalangle /= 2;

        float getAngle = 0;
        for (int i = 0; i <= index; i++)
        {
            float nowindexangle = 0;

            UIVertex lb = new UIVertex();
            helper.PopulateUIVertex(ref lb, (i + startindex) * 4);

            UIVertex lt = new UIVertex();
            helper.PopulateUIVertex(ref lt, (i + startindex) * 4 + 1);

            nowindexangle = Vector3.Distance(lb.position, lt.position);

            if (i == index)
            {
                getAngle += Mathf.Asin(nowindexangle / 2 / curvedRadius) * Mathf.Rad2Deg * curvedFontSpaceScale;
            }
            else
            {
                getAngle += Mathf.Asin(nowindexangle / 2 / curvedRadius) * 2 * Mathf.Rad2Deg * curvedFontSpaceScale;
            }
        }
        getAngle -= totalangle;

        if (step != 0)
            return getAngle / (step * 0.3f + 1);
        return getAngle;
    }

    /// <summary>
    /// 计算字符角度
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="charaCount">当前行字符总数</param>
    /// <param name="tex">当前行所有文字字符串</param>
    /// <param name="index">当前字符在当前行的位置，从0开始</param>
    /// <param name="step">当前行行号索引</param>
    /// <returns></returns>
    private float GetDegree(VertexHelper helper, int charaCount, string tex, int index, int step)
    {
        return 0f;
    }
}
