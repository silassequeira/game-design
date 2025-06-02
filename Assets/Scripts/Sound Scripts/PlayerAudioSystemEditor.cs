#if UNITY_EDITOR
using UnityEngine; // Add this line
using UnityEditor;

[CustomEditor(typeof(MonoBehaviour), true)]
public class PlayerAudioSystemEditor : Editor
{
    // Rest of your code remains unchanged
}
#endif