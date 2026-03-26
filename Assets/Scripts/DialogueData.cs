using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    [TextArea(1, 3)]
    public string text = "";
}

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "Maze Game / NPC Dialogue")]
public class DialogueData : ScriptableObject
{
    [Header("Identity")]
    public string npcName = "???";

    [Header("Dialogue Lines")]
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Riddle Settings")]
    public bool hasRiddle = false;
    public bool useLLMRiddle = false;

    [TextArea(2, 4)]
    public string riddleQuestion = "";

    [TextArea(1, 2)]
    public string riddleHint = "";

    [Header("Answers")]
    public List<string> acceptedAnswers = new List<string>();

    [TextArea(1, 2)]
    public string wrongAnswerResponse = "That is not correct. Ponder deeper...";

    [TextArea(1, 2)]
    public string correctAnswerResponse = "You have answered wisely.";

    [Header("Reward")]
    public int keysReward = 1;
}