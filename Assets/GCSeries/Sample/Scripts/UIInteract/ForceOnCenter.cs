using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GCSeries
{
    //World Canvas Follow MRSystem
    public class ForceOnCenter : MonoBehaviour
    {
        void Start()
        {
            var screenWidth = Vector3.Distance(FCore.screenPointLeftTop, FCore.screenPointRightTop);
            var screenHight = Vector3.Distance(FCore.screenPointRightTop, FCore.screenPointRightBotton);
            var scale = screenHight / 1080;
            transform.localScale = new Vector3(scale, scale, 1);
        }

        void Update()
        {
            transform.position = FCore.screenCentre;
            transform.rotation = FCore.screenRotation;
        }
    }
}