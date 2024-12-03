using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mono.CSharp;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float healthChangeDelay = .5f;

    private CharacterStatHandler statsHandler;
    private float timeSinceLastChange = float.MaxValue;
    private bool isAttacked = false;

    public event Action OnDamage;
    public event Action OnHeal;
    public event Action OnDeath;
    public event Action OnInvincibilityEnd;

    public float CurrentHealth { get; private set; }

    public float MaxHealth;

    public Slider HpBarSlider;

    [SerializeField]
    protected SkinnedMeshRenderer skinMesh;

    [Header("DamageText")]
    [SerializeField]
    private GameObject damageTextCanvasPrefab;
    private Canvas damageTextCanvasClone;
    private Camera damageTextCamera;
    private List<GameObject> damageTextList = new List<GameObject>();

    private void Awake()
    {
        statsHandler = GetComponent<CharacterStatHandler>();
        //MaxHealth = statsHandler.CurrentStat.attackSO.hp;
    }

    private void Start()
    {
        CurrentHealth = MaxHealth;

        if (this.gameObject.CompareTag(Data.PlayerTag) && (BattleSceneManager.Instance != null))
        {
            BattleSceneManager.Instance.OnHealGet += HealItemGet;
            BattleSceneManager.Instance.OnMaxHpGet += MaxHPItemGet;
        }

        damageTextCamera = Camera.main;
        damageTextCanvasClone = Instantiate(damageTextCanvasPrefab, this.transform).GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        //if(statsHandler.CurrentStat.attackSO != null)
        //{
        //    CurrentHealth = statsHandler.CurrentStat.attackSO.hp;
        //    CheckHp();
        //}
        StartCoroutine(InitCoroutine());
    }

    private void OnDisable()
    {
        foreach (GameObject damageText in damageTextList)
        {
            ObjectPoolManager.Instance.ReturnObject("BasicDamage", "UI", damageText);
        }

        damageTextList.Clear();
    }

    private IEnumerator InitCoroutine()
    {
        while (statsHandler.CurrentStat.attackSO == null || statsHandler.CurrentStat.attackSO.hp == 0)
        {
            yield return null;
        }
        MaxHealth = statsHandler.CurrentStat.attackSO.hp;
        CurrentHealth = statsHandler.CurrentStat.attackSO.hp;
        CheckHp();
    }

    private void Update()
    {
        if (isAttacked && timeSinceLastChange < healthChangeDelay)
        {
            timeSinceLastChange += Time.deltaTime;
            if (timeSinceLastChange >= healthChangeDelay)
            {
                OnInvincibilityEnd?.Invoke();
                isAttacked = false;
            }
        }
    }

    public void HealItemGet(int? value)
    {
        ChangeHealth(MaxHealth * 0.3f);//최대체력의 30퍼센트 회복
    }

    public void MaxHPItemGet(int? value)
    {
        MaxHealth += statsHandler.CurrentStat.attackSO.hp_delta ;//TODO : 최대체력 증가, 수치 변경필요
        ChangeHealth(statsHandler.CurrentStat.attackSO.hp_delta);

    }

    public void CheckHp()
    {
        if (HpBarSlider != null)
        {
            HpBarSlider.value = CurrentHealth / MaxHealth;
        }
    }

    public bool ChangeHealth(float change)
    {

        if (timeSinceLastChange < healthChangeDelay)
        {
            return false;
        }

        timeSinceLastChange = 0f;
        CurrentHealth += change;

        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, MaxHealth);

        if (CurrentHealth <= 0f)
        {
            CallDeath();
            return true;
        }

        ShowDamageText(change);

        if (change >= 0)
        {
            isAttacked = true;

            OnHeal?.Invoke();
        }
        else
        {
            OnDamage?.Invoke();

            if (skinMesh != null)
            { 
                StartCoroutine(HitFlashRed());
            }

            isAttacked = true;
        }

        CheckHp();
        return true;
    }

    private void CallDeath()
    {
        isAttacked = true;
        OnDeath?.Invoke();
        if (IsLastBoss()) BattleSceneManager.Instance.GameSet(true); // 보스일 경우, 게임 클리어
        if (IsPlayer()) BattleSceneManager.Instance.GameSet(false); // 플레이어일 경우, 게임 종료
    }
    private bool IsLastBoss()
    {
        if (TryGetComponent(out BossController _))
        {
            return true;
        }
        return false;
    }
    private bool IsPlayer()
    {
        if (TryGetComponent(out PlayerInputController _))
        {
            return true;
        }
        return false;
    }

    private IEnumerator HitFlashRed()
    {
        skinMesh.material.color = Color.red;

        yield return Data.WaitForSeconds(0.2f);

        skinMesh.material.color = Color.white;
    }

    private void ShowDamageText(float a_damage)
    {
        Vector3 initScreenPos = damageTextCamera.WorldToScreenPoint(this.transform.position);

        if (initScreenPos.z < -10.0f)
        {
            return;
        }

        GameObject damageTextObj = ObjectPoolManager.Instance.GetObject(Data.Pool_BasicDamage, Data.Pool_UI, damageTextCanvasClone.transform);
        damageTextList.Add(damageTextObj);

        damageTextObj.transform.position = initScreenPos;

        TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();

        if (a_damage >= 0)
        {
            damageText.color = Color.green;
        }

        if (damageText != null)
            damageText.text = Math.Abs(a_damage).ToString();

        StartCoroutine(AnimateDamageText(damageTextObj, damageText));
    }

    private IEnumerator AnimateDamageText(GameObject a_damageTextObj, TextMeshProUGUI a_damagetext)
    {
        float duration = 1.0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 startAddYPos = Vector3.up * 2.0f;
            Vector3 endAddYPos = Vector3.up * 3.0f;

            Vector3 curTxtPosToWorld = damageTextCamera.ScreenToWorldPoint(a_damageTextObj.transform.position);
            Vector3 gapObjAndtxtPos = this.transform.position - curTxtPosToWorld;
            Vector3 startPosition = curTxtPosToWorld + gapObjAndtxtPos + startAddYPos;
            Vector3 endPosition = startPosition + endAddYPos;

            Vector3 startPosToScreenPos = damageTextCamera.WorldToScreenPoint(startPosition);
            Vector3 endPosToScreenPos = damageTextCamera.WorldToScreenPoint(endPosition);

            a_damageTextObj.transform.position = Vector3.Lerp(startPosToScreenPos, endPosToScreenPos, elapsed / duration);

            if (a_damagetext != null)
                a_damagetext.alpha = Mathf.Lerp(1, 0.8f, elapsed / duration);

            yield return null;
        }

        damageTextList.Remove(a_damageTextObj);
        ObjectPoolManager.Instance.ReturnObject("BasicDamage", "UI", a_damageTextObj);
    }

}