/*
 
 This thing is closed source, i cant really DO an updater.
 
using System;
using System.Security.Policy;
using System.Text;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using UnityEngine;
using Utf8Json;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using FoundationFortune.API.Models.Classes;
using MEC;

namespace FoundationFortune.API
{
    public class Updater : MonoBehaviour
    {
    private const string Url = "https://github.com/DuoNineXcore/Foundation-Fortune-EXILED/releases";

        private Version currentVersion;
        private string dllName;

        private int State { get; set; } = 0;
        public bool IsSafeToStop => State != 1;
        public HttpClient Client { get; set; }
        
        public Updater(Version currentVersion, string url, string dllName, HttpClient client) {
            this.currentVersion = currentVersion;
            this.dllName = dllName;
            Client = client;
        }


        private IEnumerator<float> SearchForUpdates()
        {
            using UnityWebRequest request = UnityWebRequest.Get(Url);
            request.SetRequestHeader("Content-Type", "application/json");
            yield return Timing.WaitUntilDone(request.SendWebRequest());

            if (request.result == UnityWebRequest.Result.Success)
            {
                string content = request.downloadHandler.text;

                ExpectedResponse? expectedResponse = JsonSerializer.Deserialize<ExpectedResponse>(content);

                if (expectedResponse?.assets != null)
                {

                }
            } else
            {
                Log.Warn("[UPDATER] Unable to check for updates.");
            }
        }

        public async void Start(IPlugin<IConfig> plugin)
        {
            Log.Debug("Hi");
            using HttpClient client = Client;
            Log.Debug("Obtained client");
            HttpResponseMessage response = await client.GetAsync(Url).ConfigureAwait(false);
            Log.Debug("Got response");
            if (!response.IsSuccessStatusCode) Log.Warn("Unable to search for updates");

            string content = await response.Content.ReadAsStringAsync();
            Log.Debug(content);
            ExpectedResponse? expectedResponse = JsonSerializer.Deserialize<ExpectedResponse>(content);
            if (expectedResponse != null)
            {
                Log.Debug("Again");
                ExpectedResponse.Asset[]? assets = expectedResponse?.assets;
                if (assets != null)
                {
                    Log.Debug(assets.First().url);
                }
            }
        }
    }
}
*/