using System;
using System.Security.Cryptography;
using System.Text;
using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Bootstrap
{
    public sealed class StorySaveStore
    {
        [Serializable] private sealed class Envelope { public int revision; public string payload; public string checksum; }
        private readonly string prefix;
        private readonly Func<StoryRun, bool> validateContent;
        public StorySaveStore(string prefix = "iron.story.proof.v1", Func<StoryRun, bool> validateContent = null)
        { this.prefix = prefix; this.validateContent = validateContent; }
        public string LastError { get; private set; }

        public bool TryLoad(out StoryRun run)
        {
            run = null; LastError = null;
            var a = Read(".a"); var b = Read(".b");
            var best = a == null ? b : b == null || a.revision >= b.revision ? a : b;
            if (best == null) return false;
            run = JsonUtility.FromJson<StoryRun>(best.payload);
            run.MigrateLegacyGrave();
            return true;
        }

        public bool Save(StoryRun run)
        {
            try
            {
                run.Validate();
                if (validateContent != null && !validateContent(run)) throw new ArgumentException("Invalid story content.");
                var a = Read(".a"); var b = Read(".b");
                var next = Math.Max(a?.revision ?? 0, b?.revision ?? 0) + 1;
                var payload = JsonUtility.ToJson(run);
                var envelope = new Envelope { revision = next, payload = payload, checksum = Hash(payload) };
                // Alternate slots keep the previous valid checkpoint available after a partial write.
                var slot = a == null || (b != null && a.revision <= b.revision) ? ".a" : ".b";
                PlayerPrefs.SetString(prefix + slot, JsonUtility.ToJson(envelope)); PlayerPrefs.Save();
                LastError = null; return true;
            }
            catch (Exception exception) { LastError = "Não foi possível salvar neste navegador: " + exception.GetType().Name; return false; }
        }

        private Envelope Read(string slot)
        {
            var raw = PlayerPrefs.GetString(prefix + slot, "");
            if (raw.Length == 0) return null;
            try
            {
                if (raw.Length > 16000) throw new ArgumentException();
                var envelope = JsonUtility.FromJson<Envelope>(raw);
                if (envelope == null || envelope.revision <= 0 || string.IsNullOrEmpty(envelope.payload) ||
                    envelope.checksum != Hash(envelope.payload)) throw new ArgumentException();
                var run = JsonUtility.FromJson<StoryRun>(envelope.payload);
                if (run == null) throw new ArgumentException();
                run.Validate();
                if (validateContent != null && !validateContent(run)) throw new ArgumentException();
                return envelope;
            }
            catch (Exception) { LastError = "Um salvamento inválido foi ignorado."; return null; }
        }

        private static string Hash(string value)
        {
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
