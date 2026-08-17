using System;
using System.Collections.Generic;
using Fsp.BattleRoyale;
using UnityEngine;
using UnityEngine.UI;

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
            Refresh();
        }

        private void OnDisable()
        {
            KillFeedBus.KillReported -= OnKillReported;
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
            string killerName = killer != null ? killer.DisplayName : "المنطقة";
            string victimName = victim != null ? victim.DisplayName : "Player";
            entries.Enqueue(new Entry($"{killerName}  •  {victimName}", Time.unscaledTime));

            while (entries.Count > rows.Length)
                entries.Dequeue();

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

    public static class KillFeedBus
    {
        public static event Action<MatchParticipant, MatchParticipant> KillReported;
        public static void Report(MatchParticipant killer, MatchParticipant victim) => KillReported?.Invoke(killer, victim);
    }
}
