using UnityEngine;

public class GameExit : MonoBehaviour
{
    public void ExitGame()
    {
        // Для выхода в редакторе во время Play Mode
        //#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        //#endif
        //Application.Quit();
    }
}