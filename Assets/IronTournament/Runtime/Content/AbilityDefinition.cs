using IronTournament.Core;
using UnityEngine;

namespace IronTournament.Content
{
    [CreateAssetMenu(menuName = "Iron Tournament/Ability", fileName = "Ability")]
    public sealed class AbilityDefinition : ScriptableObject
    {
        [SerializeField] private AbilityId id;
        [SerializeField] private string displayName;
        [SerializeField] private AbilityTarget target;
        [SerializeField] private bool consumesTurn = true;

        public AbilityId Id => id;

        public string DisplayName => displayName;

        public AbilityTarget Target => target;

        public bool ConsumesTurn => consumesTurn;
    }
}
