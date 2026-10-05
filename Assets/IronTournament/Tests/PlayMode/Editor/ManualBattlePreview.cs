#if UNITY_EDITOR
using System;
using IronTournament.Content;
using IronTournament.Core;
using UnityEditor;
using UnityEngine;

namespace IronTournament.Presentation.Tests
{
    [InitializeOnLoad]
    internal static class ManualBattlePreview
    {
        private const string PendingKey = "IronTournament.ManualBattlePreview.Pending";

        static ManualBattlePreview()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(PendingKey, false)) return;
                SessionState.SetBool(PendingKey, false);
                EditorApplication.delayCall += InitializeSelected;
            };
        }

        [MenuItem("Iron Tournament/Testar duelo selecionado")]
        private static void Run()
        {
            if (EditorApplication.isPlaying) InitializeSelected();
            else
            {
                SessionState.SetBool(PendingKey, true);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("Iron Tournament/Testar duelo selecionado", true)]
        private static bool CanRun()
            => !EditorApplication.isCompiling && UnityEngine.Object.FindAnyObjectByType<BattleView>() != null;

        private static void InitializeSelected()
        {
            if (!EditorApplication.isPlaying) return;
            try { Initialize(UnityEngine.Object.FindAnyObjectByType<BattleView>()); }
            catch (Exception error) { Debug.LogException(error); }
        }

        internal static Battle Initialize(BattleView view)
        {
            if (view == null) throw new ArgumentNullException(nameof(view));
            var hero = ContentMapper.BuildCombatant(view.SelectedHero);
            var enemy = ContentMapper.BuildEncounter(view.SelectedEncounter).Opponent;
            var state = new BattleState(new CombatantState(hero, hero.BaseStats), new CombatantState(enemy, enemy.BaseStats));
            var battle = new Battle(state, new SeededRandomSource(17));
            view.GetComponent<BattlePresenter>().Initialize(battle);
            return battle;
        }
    }
}
#endif
