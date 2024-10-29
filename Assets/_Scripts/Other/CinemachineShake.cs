using UnityEngine;
using Cinemachine;

public class CinemachineShake : MonoBehaviour
{
    public static CinemachineShake Instance { get; private set; }
    private CinemachineVirtualCamera cmVCam;
    private CinemachineBasicMultiChannelPerlin noiseComponent;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;
    private float startingFrequency;

    void Awake()
    {
        Instance = this;
        cmVCam = GetComponent<CinemachineVirtualCamera>();
        noiseComponent = cmVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity, float frequency, float time)
    {
        noiseComponent.m_AmplitudeGain = intensity;
        noiseComponent.m_FrequencyGain = frequency;

        startingIntensity = intensity;
        startingFrequency = frequency;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float progress = 1 - (shakeTimer / shakeTimerTotal);

            noiseComponent.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, progress);
            noiseComponent.m_FrequencyGain = Mathf.Lerp(startingFrequency, 0f, progress);

            if (shakeTimer <= 0)
            {
                noiseComponent.m_AmplitudeGain = 0f;
                noiseComponent.m_FrequencyGain = 0f;
            }
        }
    }
}