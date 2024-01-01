using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class _SoundService : MonoBehaviour
{
    public static _SoundService Instance;

    public int soundLayer = 30;
    public float DefaultAudioRange = 5;
    public static float SOUND_MASTER_VOLUME = 1.0f;
    public static int SOUND_DUPLICATES_ALLOWED = 5;
    private static Dictionary<string, AudioClip> SOUND_CLIPS = new Dictionary<string, AudioClip>();
    private Dictionary<AudioClip, List<AudioSource>> soundSources = new Dictionary<AudioClip, List<AudioSource>>();
    public delegate void soundSpawnDelegate(GameObject newSound);

    void Start()
    {
        Instance = this;
        Instance.StartCoroutine(audioUpdate());
    }

    void Update()
    {
        
    }



    /***** PUBLIC *****/

    public static GameObject PlayAmbientSound(string path, Vector3 position, float pitch, float volume, float range = -1, soundSpawnDelegate soundSpawnCallback = null)
    {
        if (Instance && FindObjectOfType(typeof(AudioListener)))
        {
            if (!SOUND_CLIPS.ContainsKey(path))
            {
                SOUND_CLIPS[path] = Resources.Load<AudioClip>(path);

            }
            AudioClip clip = SOUND_CLIPS[path];
            return PlayAmbientSound(SOUND_CLIPS[path], position, pitch, volume, range, soundSpawnCallback);
            //if (!Instance.soundSources.ContainsKey(clip))
            //{
            //    Instance.soundSources[clip] = new List<AudioSource>();
            //}
            //AudioSource audioSource = CREATE_NEW_AUDIO_SOURCE(clip, position, pitch, volume, range);
            //audioSource.transform.position = position;
            //audioSource.Play();
            //Instance.soundSources[clip].Add(audioSource);
            //if (Instance.soundSources[clip].Count > SOUND_DUPLICATES_ALLOWED)
            //{
            //    Destroy(Instance.soundSources[clip].First(x => x).gameObject);
            //}
            //if (onSoundSpawn != null)
            //{
            //    onSoundSpawn(audioSource.gameObject);
            //}
            //return audioSource.gameObject;
        }
        else
        {
            return null;
        }
    }


    public static GameObject PlayAmbientSound(AudioClip clip, Vector3 position, float pitch, float volume, float range = -1, soundSpawnDelegate onSoundSpawn = null)
    {
        if (Instance && FindObjectOfType(typeof(AudioListener)))
        {
            if (!SOUND_CLIPS.ContainsKey(clip.name))
            {
                SOUND_CLIPS[clip.name] = clip;
            }
            if (!Instance.soundSources.ContainsKey(clip))
            {
                Instance.soundSources[clip] = new List<AudioSource>();
            }
            AudioSource audioSource = CREATE_NEW_AUDIO_SOURCE(clip, position, pitch, volume, range);
            audioSource.transform.position = position;
            audioSource.Play();
            Instance.soundSources[clip].Add(audioSource);
            if (Instance.soundSources[clip].Count > SOUND_DUPLICATES_ALLOWED)
            {
                Destroy(Instance.soundSources[clip].First(x => x).gameObject);
            }
            if (onSoundSpawn != null)
            {
                onSoundSpawn(audioSource.gameObject);
            }
            return audioSource.gameObject;
        }
        else
        {
            return null;
        }
    }    //overloaded method above



    /***** PRIVATE *****/
    private IEnumerator audioUpdate()
    {
        yield return null;
        while (Instance)
        {
            yield return new WaitUntil(() => soundSources.Values.Count(x => x.Count > 0) > 0);
            for (int i = 0; i < soundSources.Count; i++)
            {
                yield return null;
                foreach (AudioSource source in soundSources.ElementAt(i).Value.Where(x => x ? !x.isPlaying || !x.clip || x.pitch <= 0 : false))
                {
                    Destroy(source.gameObject);
                }
                if (soundSources.ElementAt(i).Value.Count(x => x) > SOUND_DUPLICATES_ALLOWED)
                {
                    for (int j = 0; j < soundSources.ElementAt(i).Value.Count(x => x) - SOUND_DUPLICATES_ALLOWED; j++)
                    {
                        Destroy(soundSources.ElementAt(i).Value.First(x => x).gameObject);
                    }
                }
                soundSources.ElementAt(i).Value.RemoveAll(x => !x);
            }
        }
    }

    private static AudioSource CREATE_NEW_AUDIO_SOURCE(AudioClip clip, Vector3 position, float pitch, float volume, float range = -1)
    {
        AudioSource audioSource = new GameObject(clip.name).AddComponent<AudioSource>();
        audioSource.gameObject.layer = Instance.soundLayer;
        audioSource.playOnAwake = false;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 0;
        audioSource.maxDistance = range >= 0 ? range : Instance.DefaultAudioRange;
        audioSource.spatialBlend = 1.0f;
        audioSource.spread = 60f;
        audioSource.clip = clip;
        audioSource.volume = volume * SOUND_MASTER_VOLUME;
        audioSource.pitch = pitch;
        return audioSource;
    }


}
