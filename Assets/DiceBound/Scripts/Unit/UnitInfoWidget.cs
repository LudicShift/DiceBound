using DiceBound;
using KCoreKit;
using UnityEngine;

public class UnitInfoWidget : WidgetBase
{
    [SerializeField]
    private GaugeWidget hpGauge;
    [SerializeField]
    private SkillCooldownGaugeWidget basicSkillGauge;  
    [SerializeField]
    private SkillCooldownGaugeWidget activeSkillGauge;
    [SerializeField]
    private TierLabelWidget tierLabel;
    
    public void SetMaxHp(float maxHp)
    {
        hpGauge.Setup(maxHp,maxHp);
    }
    public void SetHp(float hp)
    {
        hpGauge.OnChange(hp);
    }

    public void BindBasicSkill(Skill skill)
    {
        basicSkillGauge.Show();
        basicSkillGauge.Bind(skill) ;
    }  
    
    public void BindActiveSkill(Skill skill)
    {
        activeSkillGauge.Show();
        activeSkillGauge.Bind(skill) ;
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

    public void SetTier(int tier)
    {
        tierLabel.OnChange(tier);
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
}
