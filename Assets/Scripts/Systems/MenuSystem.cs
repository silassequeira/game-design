public class UIManager : MonoBehaviour {
    public GameObject pausePanel;
    public GameObject settingsPanel;
    
    public void TogglePauseMenu(bool state) {
        pausePanel.SetActive(state);
        Time.timeScale = state ? 0 : 1;
    }
}