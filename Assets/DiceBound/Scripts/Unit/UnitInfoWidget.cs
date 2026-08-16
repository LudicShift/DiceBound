using System;
using DiceBound;
using KCoreKit;
using UnityEngine;

public class UnitInfoWidget : WidgetBase
{
    [SerializeField] private GaugeWidget hpGauge;
    [SerializeField] private SkillCooldownGaugeWidget basicSkillGauge;
    [SerializeField] private SkillCooldownGaugeWidget activeSkillGauge;
    [SerializeField] private TierLabelWidget tierLabel;

    public bool flip;
    private Vector3 _changeBasicPos;
    private Vector3 _changeActivePos;

    public void Awake()
    {
        _changeBasicPos = basicSkillGauge.transform.localPosition;
        _changeActivePos = activeSkillGauge.transform.localPosition;
    }

    public void SetMaxHp(float maxHp)
    {
        hpGauge.Setup(maxHp, maxHp);
    }

    public void SetHp(float hp)
    {
        hpGauge.OnChange(hp);
    }

    public void BindBasicSkill(Skill skill)
    {
        basicSkillGauge.Show();
        basicSkillGauge.Bind(skill);
    }

    public void BindActiveSkill(Skill skill)
    {
        activeSkillGauge.Show();
        activeSkillGauge.Bind(skill);
    }

    public void OnUpdate()
    {
        basicSkillGauge.OnUpdate();
        activeSkillGauge.OnUpdate();
    }

    public void OnAppearBegin()
    {
        if (hpGauge)
        {
            hpGauge.canvasGroup.alpha = 0;
        }
    }

    public void OnAppearEnd()
    {
        if (hpGauge)
        {
            hpGauge.canvasGroup.alpha = 1;
        }
    }

    public void ReleaseSkills()
    {
        basicSkillGauge.Hide();
        activeSkillGauge.Hide();
        basicSkillGauge.Release();
        activeSkillGauge.Release();
    }

    public void OnUpgrade(int tier)
    {
        basicSkillGauge.OnUpgrade();
        activeSkillGauge.OnUpgrade();
        tierLabel.OnChange(tier);
    }

    public void SetFlip(bool value)
    {
        flip = value;
        var pos1 = basicSkillGauge.transform.localPosition;
        var pos2 = activeSkillGauge.transform.localPosition;
        pos1.x = _changeBasicPos.x * (flip ? -1 : 1);
        pos2.x = _changeActivePos.x * (flip ? -1 : 1);
        basicSkillGauge.transform.localPosition = pos1;
        activeSkillGauge.transform.localPosition = pos2;
    }
}