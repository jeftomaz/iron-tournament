using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace IronTournament.Core
{
    public enum ActionRejection
    {
        None,
        BattleOver,
        UnavailableAction
    }

    public sealed class ActionResult
    {
        private static readonly ReadOnlyCollection<BattleEvent> NoEvents =
            new ReadOnlyCollection<BattleEvent>(new List<BattleEvent>());

        private readonly ReadOnlyCollection<BattleEvent> events;

        private ActionResult(ActionRejection rejection, ReadOnlyCollection<BattleEvent> events)
        {
            Rejection = rejection;
            this.events = events;
        }

        public ActionRejection Rejection { get; }

        public bool IsAccepted => Rejection == ActionRejection.None;

        public IReadOnlyList<BattleEvent> Events => events;

        public static ActionResult Accepted(IList<BattleEvent> events)
        {
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            if (events.Count == 0)
            {
                throw new ArgumentException("An accepted action must produce events.", nameof(events));
            }

            for (var index = 0; index < events.Count; index++)
            {
                if (events[index] == null)
                {
                    throw new ArgumentException("An event is required.", nameof(events));
                }
            }

            return new ActionResult(
                ActionRejection.None,
                new ReadOnlyCollection<BattleEvent>(new List<BattleEvent>(events)));
        }

        public static ActionResult Rejected(ActionRejection rejection)
        {
            return new ActionResult(ConfigurationGuard.Defined(rejection, nameof(rejection)), NoEvents);
        }
    }
}
