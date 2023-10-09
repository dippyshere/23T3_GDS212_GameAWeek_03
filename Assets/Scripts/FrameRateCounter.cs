using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateCounter : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        StartCoroutine(UpdateFrameRate());
        Application.targetFrameRate = -1;
    }

    private IEnumerator UpdateFrameRate()
    {
        while (true)
        {
            int frameRate = (int)(1f / Time.unscaledDeltaTime);
            text.text = frameRate.ToString();
            yield return new WaitForSeconds(updateInterval);
        }
    }
}
