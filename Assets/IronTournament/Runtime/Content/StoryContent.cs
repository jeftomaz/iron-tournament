using System;
using UnityEngine;

namespace IronTournament.Content
{
    [Serializable]
    public sealed class StoryLine { public string speaker; [TextArea] public string text; }
    [Serializable]
    public sealed class StoryDialogue { public string id; public StoryLine[] lines; }
    [Serializable]
    public sealed class StoryScript { public StoryDialogue[] dialogues; public string[] houses; public string[] bedrooms; public string[] graves; public string skeletonLetter; }

    [CreateAssetMenu(menuName = "Iron Tournament/Story proof")]
    public sealed class StoryContent : ScriptableObject
    {
        public CombatantDefinition warrior;
        public CombatantDefinition mage;
        public EncounterDefinition goblin;
        public Sprite opening;
        public Sprite campaignTitle;
        public EncounterDefinition skeleton;
        public Sprite village;
        public Sprite distantVillage;
        public Sprite forestPath;
        public Sprite swamp;
        public Sprite coldForest;
        public Sprite coldClearing;
        public Sprite skeletonHand;
        public Sprite coffin;
        public Sprite mountain;
        public Sprite grave;
        public Sprite[] houses;
        public Sprite[] bedrooms;
        public Sprite child;
        public Sprite childSaved;
        public Sprite childHurt;
        public Sprite childDead;
        public Sprite goblinDead;
        public Sprite skeletonGraveyard;
        public Sprite skeletonDead;
        public Sprite[] warriorPoses;
        public Sprite[] magePoses;
        public Sprite[] goblinPoses;
        public Sprite skeletonPose;
        public TextAsset script;
        [NonSerialized] private StoryScript parsed;
        public StoryScript Script => parsed ??= JsonUtility.FromJson<StoryScript>(script.text);
        public StoryLine[] Dialogue(string id)
        {
            foreach (var dialogue in Script.dialogues) if (dialogue.id == id) return dialogue.lines;
            throw new ArgumentException("Missing story dialogue: " + id);
        }
    }
}
