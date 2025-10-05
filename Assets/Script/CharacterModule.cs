using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModule : MonoBehaviour
{
    private static CharacterModule _instance;

    public static CharacterModule Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CharacterModule>();

                // If still not found, create a new GameObject with CharacterModule
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("CharacterModule");
                    _instance = singletonObject.AddComponent<CharacterModule>();
                    DontDestroyOnLoad(singletonObject);
                }
            }
            return _instance;
        }
    }
    // Private backing fields
    private int _skillRewind;
    private int _skillDash;
    private int _skillStop;
    private int _health;

    // Public properties with OnValueChange callback
    public int SkillRewind
    {
        get => _skillRewind;
        set
        {
            if (_skillRewind != value)
            {
                int oldValue = _skillRewind;
                _skillRewind = value;
                OnValueChange(nameof(SkillRewind), oldValue, value);
            }
        }
    }

    public int SkillDash
    {
        get => _skillDash;
        set
        {
            if (_skillDash != value)
            {
                int oldValue = _skillDash;
                _skillDash = value;
                OnValueChange(nameof(SkillDash), oldValue, value);
            }
        }
    }

    public int SkillStop
    {
        get => _skillStop;
        set
        {
            if (_skillStop != value)
            {
                int oldValue = _skillStop;
                _skillStop = value;
                OnValueChange(nameof(SkillStop), oldValue, value);
            }
        }
    }

    public int Health
    {
        get => _health;
        set
        {
            if (_health != value)
            {
                int oldValue = _health;
                _health = value;
                OnValueChange(nameof(Health), oldValue, value);
            }
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    // Value change callback function
    private void OnValueChange(string propertyName, int oldValue, int newValue)
    {
        Debug.Log($"[CharacterModule] {propertyName} changed: {oldValue} -> {newValue}");
        
        // Add your custom logic here based on which property changed
        switch (propertyName)
        {
            case nameof(SkillRewind):
                OnSkillRewindChanged(oldValue, newValue);
                break;
            case nameof(SkillDash):
                OnSkillDashChanged(oldValue, newValue);
                break;
            case nameof(SkillStop):
                OnSkillStopChanged(oldValue, newValue);
                break;
            case nameof(Health):
                OnHealthChanged(oldValue, newValue);
                break;
        }
    }

    // Individual change handlers (optional, implement as needed)
    private void OnSkillRewindChanged(int oldValue, int newValue)
    {
        // Custom logic for skill rewind changes
    }

    private void OnSkillDashChanged(int oldValue, int newValue)
    {
        // Custom logic for skill dash changes
    }

    private void OnSkillStopChanged(int oldValue, int newValue)
    {
        // Custom logic for skill stop changes
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        // Custom logic for health changes
        if (newValue <= 0)
        {
            // Handle death
        }
    }
}
