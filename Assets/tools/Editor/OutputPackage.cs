using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace GCSeries
{
    public class OutputPackage
    {
        static string outputPath = System.IO.Directory.GetParent(Application.dataPath).FullName + "/Output";
        [MenuItem("GCSeries/Export GCSeries SDK")]
        static void ExportGCSeriesPackage()
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }
            var assetPathNames = new string[] { "Assets/GCSeries", "Assets/StreamingAssets" };
            AssetDatabase.ExportPackage(assetPathNames, outputPath + "/GCSeries.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse);
        }
    }
}