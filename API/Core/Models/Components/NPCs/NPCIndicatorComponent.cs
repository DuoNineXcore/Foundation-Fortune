using System;
using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using PlayerRoles;
using UnityEngine;
using Light = Exiled.API.Features.Toys.Light;

namespace FoundationFortune.API.Core.Models.Components.NPCs
{
    public class NPCIndicatorComponent : MonoBehaviour
    {
        internal Light glowLight;
        internal Primitive circ;
        private bool _threw;

        private void Start() => Timing.RunCoroutine(MoveGlow().CancelWith(this).CancelWith(gameObject));

        private IEnumerator<float> MoveGlow()
        {
            var player = Player.Get(gameObject);
            
            while (true)
            {
                yield return Timing.WaitForSeconds(0.05f);
                try
                {
                    if (glowLight == null || glowLight.AdminToyBase == null) continue;
                    if (circ == null || circ.AdminToyBase == null) continue;
                    
                    if (player.Role == RoleTypeId.None || player.Role == RoleTypeId.Spectator || player.CurrentRoom.RoomLightController._flickerDuration > 0f)
                    {
                        glowLight.Position = Vector3.one * 6000f;
                        continue;
                    }

                    glowLight.Position = player.Position + new Vector3(0f, 0.7f, 0f);
                    circ.Position = glowLight.Position;
                    circ.Color = glowLight.Color;
                }
                catch (Exception e)
                {
                    if (!_threw)
                    {
                        Log.Error(e.ToString());
                        _threw = true;
                    }
                }
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void OnDestroy()
        {
            if (glowLight != null && glowLight.AdminToyBase != null) Destroy(glowLight.AdminToyBase.gameObject);
            if (circ != null && circ.AdminToyBase != null) Destroy(circ.AdminToyBase.gameObject);
        }
    }
}