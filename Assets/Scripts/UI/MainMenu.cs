using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Paradox
{
    // Menu-only routing. Gameplay progression remains owned by GameFlow.
    public sealed class MainMenu : MonoBehaviour
    {
        public static readonly string[] LevelScenes = { "Level01", "Level02_Library", "Level03_MirrorLake", "Level04_Observatory", "Level05_Heart" };
        public static readonly string[] LevelNames = { "Garden of Echoes", "Whispering Library", "Mirror Lake Palace", "Sun-Moon Observatory", "Heart of Paradox" };
        [SerializeField] private GameObject home;
        [SerializeField] private GameObject selection;
        [SerializeField] private Button play;
        [SerializeField] private Button select;
        [SerializeField] private Button quit;
        [SerializeField] private Button back;
        [SerializeField] private Button[] levels;
        [SerializeField] private Text status;
        private bool loading;
        public bool Selecting => selection.activeSelf;
        public void Configure(GameObject main, GameObject picker, Button start, Button choose, Button exit, Button returnButton, Button[] levelButtons, Text feedback)
        { home=main; selection=picker; play=start; select=choose; quit=exit; back=returnButton; levels=levelButtons; status=feedback; }
        private void Awake()
        {
            Time.timeScale=1;
            Cursor.lockState=CursorLockMode.None; Cursor.visible=true;
            play.onClick.AddListener(()=>LoadLevel(0)); select.onClick.AddListener(()=>ShowSelection(true));
            back.onClick.AddListener(()=>ShowSelection(false)); quit.onClick.AddListener(Quit);
            for(int i=0;i<levels.Length;i++){int index=i;levels[i].onClick.AddListener(()=>LoadLevel(index));}
            ShowSelection(false);
        }
        private void Update(){if(Input.GetKeyDown(KeyCode.Escape)&&Selecting)ShowSelection(false);}
        public void ShowSelection(bool show)
        {
            if(loading)return;
            home.SetActive(!show);selection.SetActive(show);status.text="";
            if(EventSystem.current)EventSystem.current.SetSelectedGameObject(show?levels[0].gameObject:play.gameObject);
        }
        public void LoadLevel(int index)
        {
            if(loading||index<0||index>=LevelScenes.Length)return;
            string scene=LevelScenes[index];
            if(!Application.CanStreamedLevelBeLoaded(scene)){status.text="This chapter is unavailable in this build.";Debug.LogError("Missing level: "+scene);return;}
            loading=true;status.text="Entering "+LevelNames[index]+"…";
            SceneManager.LoadSceneAsync(scene);
        }
        public void Quit()
        {
            if(loading)return;
#if UNITY_EDITOR
            status.text="Quit is available in the standalone game.";
#else
            Application.Quit();
#endif
        }
    }
}
