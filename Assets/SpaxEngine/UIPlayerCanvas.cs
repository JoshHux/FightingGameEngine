using UnityEngine;
using UnityEngine.UI;
using FightingGameEngine.Data;
using FightingGameEngine.BattleUI;
using Spax;

public class UIPlayerCanvas : MonoBehaviour
{
    [UnityEngine.SerializeField] private RectTransform ReflectedHolder;
    [UnityEngine.SerializeField] private RectTransform HealthBar;
    [UnityEngine.SerializeField] private RectTransform Health;
    private int HealthValue;
    private int MaxHealth;
    [UnityEngine.SerializeField] private GameObject[] RoundCounters;
    private int m_roundsWon = 0;
    [UnityEngine.SerializeField] private Text ComboCounter;
    private int ComboCount = 0;

    [UnityEngine.SerializeField] private soCharacterData CharData;
    [UnityEngine.SerializeField] private soCharacterStatus CharStatus;
    [UnityEngine.SerializeField] private SpaxManager GameManager;
    [SerializeField] private Material _uiMaterial;

    public soCharacterData CharacterData { set { this.CharData = value; } }
    public soCharacterStatus CharacterStatus { set { this.CharStatus = value; } }

    public int RoundsWon { get { return this.m_roundsWon; } }

    void Start()
    {
        this.m_roundsWon = 0;
        MaxHealth = CharData.MaxResources.Health;
        HealthValue = MaxHealth;
        //var chipUI1 = RoundCounters[0].GetComponent<BattleUIChip>();
        //Debug.Log(chipUI1 + " " + this._uiMaterial);
        //chipUI1.SetMaterial(this._uiMaterial);
        ////reset round counters
        //foreach (GameObject counter in RoundCounters)
        //{
        //    //counter.color = Color.grey;
        //    var chipUI = counter.GetComponent<BattleUIChip>();
        //    Debug.Log(chipUI + " " + this._uiMaterial);
        //    chipUI.SetMaterial(this._uiMaterial);
        //}
        /*
        if(CharStatus.PlayerID == 2){
            ReflectedHolder.localScale = new Vector3(-1,1,1);
        }*/ //doesn't work?

    }

    void Update()
    {
        HealthValue = CharStatus.CurrentHP;
        //SetHealth(HealthValue);
        //SetComboCount(CharStatus.CurrentComboCount);
        if (HealthValue <= 0)
        {
            GameManager.StartRound(CharStatus.PlayerID);
            //WinRound(); //temporary solution
        }

    }

    public void SetHealth(int newHealth)
    {
        if (HealthValue != newHealth)
        {
            print(HealthValue);
            ShowComboCounter();
        }
        HealthValue = newHealth;
        Health.localScale = new Vector3(HealthValue / (float)MaxHealth, 1, 1);

    }

    public void AddHealth(int addHealth)
    {
        HealthValue += addHealth;
        Health.localScale = new Vector3(HealthValue / (float)MaxHealth, 1, 1);
    }

    public void WinRound()
    {
        //RoundCounters[m_roundsWon].color = Color.yellow;

        Debug.Log(m_roundsWon);
        var chipUI = RoundCounters[m_roundsWon].GetComponent<BattleUIChip>();
        chipUI.CashOutChip();

        m_roundsWon++;
        if (m_roundsWon >= RoundCounters.Length)
        {
            // Game over?
        }
        //CharStatus.CurrentHP = MaxHealth;
        //SetHealth(MaxHealth); //redundant
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
        if (ComboCount > 0)
        {
            ComboCounter.text = ComboCount + " COMBO";
            ComboCounter.fontSize = 100 + (ComboCount * 4);
            ShowComboCounter();
            return true;
        }
        else
        {
            HideComboCounter();
            return false;
        }

    }
}
