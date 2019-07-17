using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GCSeries
{
    /// <summary>
    /// 生成一个方形的遮挡面,给FView使用
    /// </summary>
    public class CullMesh : MonoBehaviour
    {
        /// <summary>
        /// 宽
        /// </summary>
        public int width = 20;

        /// <summary>
        /// 高
        /// </summary>
        public int height = 20;

        /// <summary>
        /// 深
        /// </summary>
        public int deep = 40;

        /// <summary>
        /// 遮挡面的mesh
        /// </summary>
        public Mesh mesh;

        /// <summary>
        /// 只在启动的时候设置了一下裁剪面
        /// </summary>
        void Start()
        {
            if (mesh == null)
            {
                mesh = new Mesh();
                mesh.vertices = new Vector3[] { new Vector3(-width, -height, 0), new Vector3(-width, height, 0),
                                        new Vector3(width, -height, 0), new Vector3(width, height, 0),
                                        new Vector3(-FCore.screenWidth/2, 0, 0),new Vector3(-FCore.screenWidth/2, FCore.screenHeight, 0),
                                        new Vector3(FCore.screenWidth/2, 0, 0),new Vector3(FCore.screenWidth/2, FCore.screenHeight, 0),
                                        new Vector3(-width, -height, -deep), new Vector3(-width, height, -deep),
                                        new Vector3(width, -height, -deep), new Vector3(width, height, -deep)};
                mesh.triangles = new int[] {//正面
                                     0,1,5,
                                     0,5,4,
                                     0,4,6,
                                     0,6,2,
                                     1,3,5,
                                     5,3,7,
                                     6,7,3,
                                     6,3,2,
                                     //左侧右侧
                                     8,9,1,
                                     8,1,0,
                                     2,3,11,
                                     2,11,10,
                                     //顶上,底下
                                     1,9,11,
                                     1,11,3,
                                     0,2,8,
                                     8,2,10};
            }
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = gameObject.AddComponent<MeshFilter>();
            }
            mf.mesh = mesh;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null)
            {
                mr = gameObject.AddComponent<MeshRenderer>();
            }
            Shader shaderHide = Shader.Find("Custom/Hide");
            if (shaderHide == null)
            {
                Debug.LogError("CullMesh.Start():找不到FView的shader->Custom/Hide");
            }
            mr.material = new Material(shaderHide);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

    }

}