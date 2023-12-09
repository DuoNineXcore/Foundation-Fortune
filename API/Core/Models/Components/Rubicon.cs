/*using System.Collections;
using System.Collections.Generic;
using Discord;
using UnityEngine.Networking;
using Mirror;
using UnityEngine;

namespace FoundationFortune.API.Core.Models.Components;
public class Rubicon : NetworkBehaviour
{
    private AudioSource audioSource;
    private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();

    void Start() => audioSource = GetComponent<AudioSource>();

    [Command]
    public void CmdPlayAudioFromURL(string url) => RpcPlayAudioFromURL(url);

    [ClientRpc]
    public void RpcPlayAudioFromURL(string url) => StartCoroutine(LoadAudioFromURL(url));
    
    private IEnumerator LoadAudioFromURL(string url)
    {
        if (audioClipCache.TryGetValue(url, out var value))
        {
            audioSource.clip = value;
            audioSource.Play();
        }
        else
        {
            DirectoryIterator.Log("Loading audio from URL: " + url, LogLevel.Debug);

            using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(www);
                audioClipCache[url] = audioClip;
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else DirectoryIterator.Log("Error loading audio from URL: " + url + " - " + www.error, LogLevel.Error);
        }
        DirectoryIterator.Log("Audio loaded from URL: " + url, LogLevel.Debug);
    }
}
this would be so cool
*/