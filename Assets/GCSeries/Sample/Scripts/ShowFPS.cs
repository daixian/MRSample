using UnityEngine;

public class ShowFPS : MonoBehaviour
{

    public float f_UpdateInterval = 0.5F;

    private float f_LastInterval;

    private int i_Frames = 0;

    private float f_Fps;

    void Start()
    {
        //Application.targetFrameRate=60;
        GameObject.DontDestroyOnLoad(this);
        f_LastInterval = Time.realtimeSinceStartup;
        i_Frames = 0;
    }

    void OnGUI()
    {
        string vSync = "";
        switch (QualitySettings.vSyncCount)
        {
            case 0:
                vSync = "Don't Sync";
                break;
            case 1:
                vSync = " Sync 60";
                break;
            case 2:
                vSync = " Sync 30";
                break;
        }

        string[] names = QualitySettings.names;
        int index = QualitySettings.GetQualityLevel();

        GUI.color = Color.red;
        GUILayout.Label(vSync + " FPS:" + f_Fps.ToString("f2"));
        GUILayout.Label("质量:" + names[index]);
    }

    void Update()
    {
        ++i_Frames;
        //realtimeSinceStartup是自游戏启动以来的时间
        if (Time.realtimeSinceStartup > f_LastInterval + f_UpdateInterval)
        {
            f_Fps = i_Frames / (Time.realtimeSinceStartup - f_LastInterval);

            i_Frames = 0;

            f_LastInterval = Time.realtimeSinceStartup;
        }
    }
}
