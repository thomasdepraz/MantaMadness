using UnityEngine;
using CleverCrow.Fluid.Dialogues.Actions;
using CleverCrow.Fluid.Dialogues;
[CreateMenu("Custom/TestAction")]
public class TestAction : ActionDataBase
{
    public override void OnInit(IDialogueController dialogue)
    {
        // Run the first time the action is triggered
        Debug.Log("Salut c'est l'action");
    }

    public override void OnStart()
    {
        Debug.Log("Salut c'est l'action qui demarre");
    }

    public override ActionStatus OnUpdate()
    {
        // Runs when the action begins triggering

        // Return continue to span multiple frames
        return ActionStatus.Success;
    }

    public override void OnExit()
    {
        // Runs when the actions `OnUpdate()` returns `ActionStatus.Success`
        Debug.Log("Au revoir bozo");
    }

    public override void OnReset()
    {
        // Runs after a node has fully run through the start, update, and exit cycle
    }
}
