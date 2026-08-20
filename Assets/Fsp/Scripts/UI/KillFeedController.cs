using System.Collections.Generic;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.UI;
using Fsp.Localization;

namespace Fsp.UI
{
    public sealed class KillFeedController : MonoBehaviour
    {
        [SerializeField] private Text[] rows = new Text[4];
        [SerializeField, Min(1f)] private float entryLifetime = 5f;

        private readonly Queue<Entry> entries = new Queue<Entry>();

        private void OnEnable()
        {
            KillFeedBus.KillReported += OnKillReported;
            KillFeedBus.NetworkKillReported += OnNetworkKillReported;
            Refresh();
        }

        private void OnDisable()
        {
            KillFeedBus.KillReported -= OnKillReported;
            KillFeedBus.NetworkKillReported -= OnNetworkKillReported;
        }

        private void Update()
        {
            bool changed = false;
            while (entries.Count > 0 && Time.unscaledTime - entries.Peek().createdAt > entryLifetime)
            {
                entries.Dequeue();
                changed = true;
            }
            if (changed) Refresh();
        }

        private void OnKillReported(MatchParticipant killer, MatchParticipant victim)
        {
            string killerName = killer != null ? killer.DisplayName : FspLocalizationRuntime.T("ZONE");
            string victimName = victim != null ? victim.DisplayName : "Player";
            AddEntry(killerName, victimName);
        }

        private void OnNetworkKillReported(string killerName, string victimName)
        {
            AddEntry(string.IsNullOrWhiteSpace(killerName) ? "Player" : killerName,
                string.IsNullOrWhiteSpace(victimName) ? "Player" : victimName);
        }

        private void AddEntry(string killerName, string victimName)
        {
            entries.Enqueue(new Entry($"{killerName}  •  {victimName}", Time.unscaledTime));
            while (entries.Count > rows.Length) entries.Dequeue();
            Refresh();
        }

        private void Refresh()
        {
            var snapshot = entries.ToArray();
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null) continue;
                rows[i].text = i < snapshot.Length ? snapshot[snapshot.Length - 1 - i].text : string.Empty;
                rows[i].gameObject.SetActive(i < snapshot.Length);
            }
        }

        private readonly struct Entry
        {
            public readonly string text;
            public readonly float createdAt;

            public Entry(string text, float createdAt)
            {
                this.text = text;
                this.createdAt = createdAt;
            }
        }
    }
}
