using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FightingGameEngine.Data;
using FightingGameEngine.Gameplay;

public class UIPlayerCanvas : MonoBehaviour
{
    [UnityEngine.SerializeField] private RectTransform ReflectedHolder;
    [UnityEngine.SerializeField] private RectTransform HealthBar;
    [UnityEngine.SerializeField] private RectTransform Health;
    private int HealthValue;
    private int MaxHealth;
    [UnityEngine.SerializeField] private Image[] RoundCounters;
    private int RoundsWon = 0;
    [UnityEngine.SerializeField] private Text ComboCounter;
    private int ComboCount = 0;

    [UnityEngine.SerializeField] private FightingCharacterController CharController;
    private soCharacterStatus CharStatus;

    void Start()
    {
        CharStatus = CharController.Status;
        MaxHealth = CharController.Data.MaxHP;
        HealthValue = MaxHealth;
        //reset round counters
        foreach(Image counter in RoundCounters){
            counter.color = Color.grey;
        }
        /*
        if(CharStatus.PlayerID == 2){
            ReflectedHolder.localScale = new Vector3(-1,1,1);
        }*/ //doesn't work?
        
    }

    void Update()
    {
        HealthValue = CharStatus.CurrentHP;
        SetHealth(HealthValue);
        SetComboCount(CharStatus.CurrentComboCount);
        if(HealthValue <= 0){
            WinRound(); //temporary solution
        }
    }

    public void SetHealth(int newHealth)
    {
        if(HealthValue != newHealth){
            print(HealthValue);
            ShowComboCounter();
        }
        HealthValue = newHealth;
        Health.localScale = new Vector3(HealthValue / (float)MaxHealth,1,1);
        
    }

    public void AddHealth(int addHealth)
    {
        HealthValue += addHealth;
        Health.localScale = new Vector3(HealthValue /  (float)MaxHealth,1,1);
    }

    public void WinRound()
    {
        RoundCounters[RoundsWon].color = Color.yellow;
        RoundsWon++;
        if(RoundsWon >= RoundCounters.Length){
            // Game over?
        }
        CharStatus.CurrentHP = MaxHealth;
        SetHealth(MaxHealth);
    }

    public void ShowComboCounter()
    {
        ComboCounter.color = Color.white;
    }

    public void HideComboCounter()
    {
        //setting color to clear seems more consistent than disabling the object?
        ComboCounter.color = Color.clear;
    }

    public bool SetComboCount(int newComboCount)
    {
        if(ComboCount > 0){
            ComboCounter.text = ComboCount + " COMBO";
            ComboCounter.fontSize = 100 + (ComboCount * 4);
            ShowComboCounter();
            return true;
        }
        else{
            HideComboCounter();
            return false;
        }
        
    }
}
