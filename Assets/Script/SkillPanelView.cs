using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SkillPanelView : MonoBehaviour
{
    public TMP_Text dash;
    public TMP_Text Rewind;
    public TMP_Text Pause;

    private void OnEnable()
    {
        CharacterModule.OnDashChanged += UpdateDash;
        CharacterModule.OnRewindChanged += UpdateRewind;
        CharacterModule.OnStopChanged += UpdatePause;
    }

    private void OnDisable()
    {
        CharacterModule.OnDashChanged -= UpdateDash;
        CharacterModule.OnRewindChanged -= UpdateRewind;
        CharacterModule.OnStopChanged -= UpdatePause;
    }

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
    void UpdateDash(int dashValue)
    {
        dash.text = "Dash : " + dashValue.ToString();
    }
    void UpdateRewind(int rewindValue)
    {
        Rewind.text = "Rewind : " + rewindValue.ToString();
    }
    void UpdatePause(int pauseValue)
    {
        Pause.text = "Pause : " + pauseValue.ToString();
    }
}
