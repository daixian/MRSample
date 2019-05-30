using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 切换UI事件的模式
/// </summary>
public class UIEventModeSwitch : MonoBehaviour
{
    /// <summary>
    /// 默认使用GCSerie的UI事件模式
    /// </summary>
    public bool UseEventGCSeries = true;

    /// <summary>
    /// 默认自动创建的EventSystem物体
    /// </summary>
    public GameObject eventSystem = null;

    /// <summary>
    /// 创建的GCSeries的物体
    /// </summary>
    public GameObject eventSystemGCSeries = null;

    /// <summary>
    /// 场景里所有的Canvas
    /// </summary>
    public Canvas[] canvas = null;

    /// <summary>
    /// 每个Canvas下的Raycasters
    /// </summary>
    public List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();

    /// <summary>
    /// 每个Canvas下的Raycasters
    /// </summary>
    public List<GCSeriesRaycaster> gcSeriesRaycasters = new List<GCSeriesRaycaster>();

    void Start()
    {
        if (eventSystem == null)
        {
            Debug.LogError("UIEventModeSwitch.Start():必须指定一个EventSystem物体。");
            return;
        }
        if (eventSystemGCSeries == null)
        {
            Debug.LogError("UIEventModeSwitch.Start():必须指定一个EventSystem GCSeries物体。");
            return;
        }

        //如果没指定这些canvas那就随便找一下所有的Canvas
        if (canvas == null || canvas.Length == 0)
            canvas = FindObjectsOfType<Canvas>();
        for (int i = 0; i < canvas.Length; i++)
        {
            //每个Canvas下找看是否有挂上GraphicRaycaster脚本和GCSeriesRaycaster脚本，分别对应两种UI事件模式
            var gr = canvas[i].gameObject.GetComponent<GraphicRaycaster>();
            if (gr != null)
            {
                graphicRaycasters.Add(gr);
            }
            var gcsr = canvas[i].gameObject.GetComponent<GCSeriesRaycaster>();
            if (gcsr != null)
            {
                gcSeriesRaycasters.Add(gcsr);
            }
        }
    }

    /// <summary>
    /// 设置成unity默认的UI事件模式
    /// </summary>
    void SetEventModeDefault()
    {
        //设置默认系统状态
        eventSystem.SetActive(true);
        for (int i = 0; i < graphicRaycasters.Count; i++)
        {
            graphicRaycasters[i].enabled = true;
        }

        //设置GCSeries系统状态
        eventSystemGCSeries.SetActive(false);
        for (int i = 0; i < gcSeriesRaycasters.Count; i++)
        {
            gcSeriesRaycasters[i].enabled = false;
        }

    }

    /// <summary>
    /// 设置成自己实现的GCSeries的UI事件模式
    /// </summary>
    void SetEventModeGCSeries()
    {
        //设置默认系统状态
        eventSystem.SetActive(false);
        for (int i = 0; i < graphicRaycasters.Count; i++)
        {
            graphicRaycasters[i].enabled = false;

        }

        //设置GCSeries系统状态
        eventSystemGCSeries.SetActive(true);
        for (int i = 0; i < gcSeriesRaycasters.Count; i++)
        {
            gcSeriesRaycasters[i].enabled = true;
        }
    }

    private void Update()
    {
        if (UseEventGCSeries)
        {
            SetEventModeGCSeries();
        }
        else
        {
            SetEventModeDefault();
        }
    }
}
