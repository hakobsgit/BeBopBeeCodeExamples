using UnityEngine;
using Zenject;

namespace Controllers {
    public class LaunchController : ILaunchController {
        public void Launch() {
            string path;
            
#if UNITY_STANDALONE_OSX
            path = Helpers.PersistentDataPath + "/PandaMR_APP/PandaMR.app";
#else
            path = Helpers.PersistentDataPath + "/PandaMR_APP/PandaMRWindows/PandaMR.exe";
#endif
            Log.InfoColor("path - "+ path, "LaunchController", Color.green);
            System.Diagnostics.Process.Start(path);
            QuitLauncher();
        }
        
        public void QuitLauncher() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}