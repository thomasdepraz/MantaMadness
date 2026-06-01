using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CullableAudioEmitter : MonoBehaviour, IAudioCullable
{
    [SerializeField] private AudioEmitterSettings settings;

    private EventInstance instance;
    private bool isPlaying;

    public void OnAudioRangeEnter()
    {
        if (isPlaying)
            return;

        instance = RuntimeManager.CreateInstance(settings.eventReference);

        RuntimeManager.AttachInstanceToGameObject(instance, gameObject);

        if (settings.overrideAttenuation)
        {
            instance.setProperty(
                EVENT_PROPERTY.MINIMUM_DISTANCE,
                settings.minDistance);

            instance.setProperty(
                EVENT_PROPERTY.MAXIMUM_DISTANCE,
                settings.maxDistance);
        }

        foreach (var parameter in settings.parameters)
        {
            instance.setParameterByName(
                parameter.parameterName,
                parameter.value);
        }

        instance.start();

        isPlaying = true;
    }

    public void OnAudioRangeExit()
    {
        if (!isPlaying)
            return;

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();

        isPlaying = false;
    }

    private void OnDestroy()
    {
        if (!isPlaying)
            return;

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
    }
}
