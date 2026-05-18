using UnityEngine;

public class UIButtonManager : MonoBehaviour
{
    public GameObject InGame;
    public GameObject Pause;

    public void NewGameButton()
    {
        if (InGame != null)
        {
            InGame.SetActive(true);
        }
        this.gameObject.SetActive(false);
    }

    public void PauseButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false);
        }
        this.gameObject.SetActive(false);
    }
}
