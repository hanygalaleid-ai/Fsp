using System;
using System.Threading.Tasks;
using Fsp.Lobby;
using UnityEngine;

namespace Fsp.Backend
{
    public sealed class LobbyProfileSync : MonoBehaviour
    {
        [SerializeField] private SupabaseProfileStore profileStore;

        public PlayerProfile CurrentProfile { get; private set; }
        public bool IsLoaded => CurrentProfile != null;

        public void ConfigureRuntime(SupabaseProfileStore store)
        {
            profileStore = store;
        }

        public async void LoadRuntimeProfile()
        {
            try { await LoadIntoLobbyAsync(); }
            catch (Exception ex) { Debug.LogWarning("FSP profile load failed: " + ex.Message); }
        }

        public async Task<bool> LoadIntoLobbyAsync()
        {
            if (!SupabaseSession.IsSignedIn || profileStore == null || LobbyState.Instance == null) return false;
            CurrentProfile = await profileStore.LoadAsync(SupabaseSession.UserId);
            if (CurrentProfile == null)
            {
                CurrentProfile = new PlayerProfile(SupabaseSession.UserId, LobbyState.Instance.DisplayName, LobbyState.Instance.SelectedCharacterId);
                await profileStore.SaveAsync(CurrentProfile);
            }

            LobbyState.Instance.SetDisplayName(CurrentProfile.DisplayName);
            LobbyState.Instance.SetCharacter(CurrentProfile.CharacterId);
            return true;
        }

        public async Task SaveLobbyAsync()
        {
            if (!SupabaseSession.IsSignedIn || profileStore == null || LobbyState.Instance == null) return;
            if (CurrentProfile == null)
                CurrentProfile = new PlayerProfile(SupabaseSession.UserId, LobbyState.Instance.DisplayName, LobbyState.Instance.SelectedCharacterId);

            CurrentProfile.SetDisplayName(LobbyState.Instance.DisplayName);
            CurrentProfile.SetCharacter(LobbyState.Instance.SelectedCharacterId);
            await profileStore.SaveAsync(CurrentProfile);
        }
    }
}
