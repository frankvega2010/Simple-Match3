using UnityEngine;

namespace SimpleMatch3.Utils
{
    public class QuitApp : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            }
        }
    }
}