using UnityEngine;

namespace Nitou.TCC.Inputs
{
    /// <summary>
    /// This ScriptableObject contains all the names used as input actions by the human brain. The name of the action will matters depending on the input handler used.
    /// </summary>
    [CreateAssetMenu(
        fileName = "LevelActorActionsAsset",
        menuName = "Scriptable Objects/" + "LevelActor actions asset"
    )]
    public class ActorActionsAsset : ScriptableObject
    {
        [SerializeField] private string[] boolActions;
        [SerializeField] private string[] floatActions;
        [SerializeField] private string[] vector2Actions;
    }
}