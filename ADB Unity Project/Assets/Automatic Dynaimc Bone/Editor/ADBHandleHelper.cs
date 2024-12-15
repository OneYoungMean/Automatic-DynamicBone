using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class ADBHandleHelper
{
    public const float BoneGizmosSize = 0.03f;
    public static Color defaultColor = Color.white;
    public static Vector3[][] Cube = new Vector3[][] {
        new Vector3[] {  new Vector3(-0.5f,-0.5f,-0.5f) ,new Vector3(0.5f,-0.5f,-0.5f),new Vector3(0.5f,0.5f,-0.5f),new Vector3(-0.5f,0.5f,-0.5f),// new Vector3(0f, 0f, -0.5f)
        } ,
        new Vector3[] {  new Vector3(-0.5f,-0.5f,0.5f) ,new Vector3(0.5f,-0.5f,0.5f),new Vector3(0.5f,0.5f,0.5f),new Vector3(-0.5f,0.5f,0.5f), // new Vector3(0f, 0f, 0.5f) 
        } ,
    };

    public static void DrawBone(Vector3 start, Vector3 end, float radius)
    {
        if (radius == 0)
        {
            Handles.DrawAAPolyLine(5f, start, end);
        }
        else
        {
            Matrix4x4 matrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(start + (end - start) / 2, Quaternion.LookRotation(end - start), new Vector3(radius, radius, (end - start).magnitude));

            for (int i = 0; i < Cube[0].Length; i++)
            {
                var begin = Cube[0][i];
                var tail = Cube[1][i];
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(5f, begin, tail);
            }
            Handles.matrix = matrix;
        }

    }
}
