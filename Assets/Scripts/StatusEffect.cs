using UnityEngine;

public enum StatusEffectType
{
    None,
    Stunned,
    Rooted,
    Slowed
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public int duration; // in turns
    public Vector2Int sourcePosition; // for knockback/directional effects
    
    public StatusEffect(StatusEffectType type, int duration)
    {
        this.type = type;
        this.duration = duration;
        this.sourcePosition = Vector2Int.zero;
    }
    
    public StatusEffect(StatusEffectType type, int duration, Vector2Int sourcePosition)
    {
        this.type = type;
        this.duration = duration;
        this.sourcePosition = sourcePosition;
    }
    
    public void DecrementDuration()
    {
        if (duration > 0)
        {
            duration--;
        }
    }
    
    public bool IsExpired()
    {
        return duration <= 0;
    }
}

public class StatusEffectManager : MonoBehaviour
{
    private StatusEffect currentEffect;
    
    public bool HasStatusEffect()
    {
        return currentEffect != null && !currentEffect.IsExpired();
    }
    
    public StatusEffect GetCurrentEffect()
    {
        return currentEffect;
    }
    
    public void ApplyStatusEffect(StatusEffect effect)
    {
        // If already has same effect, refresh duration
        if (currentEffect != null && currentEffect.type == effect.type)
        {
            currentEffect.duration = Mathf.Max(currentEffect.duration, effect.duration);
        }
        else
        {
            currentEffect = effect;
        }
        
        Debug.Log($"{gameObject.name} afflicted with {effect.type} for {effect.duration} turns");
    }
    
    public void ClearStatusEffect()
    {
        if (currentEffect != null)
        {
            Debug.Log($"{gameObject.name} status {currentEffect.type} cleared");
        }
        currentEffect = null;
    }
    
    public void TickStatusEffect()
    {
        if (currentEffect != null)
        {
            currentEffect.DecrementDuration();
            
            if (currentEffect.IsExpired())
            {
                Debug.Log($"{gameObject.name} status {currentEffect.type} expired");
                currentEffect = null;
            }
        }
    }
    
    public bool IsStunned()
    {
        return HasStatusEffect() && currentEffect.type == StatusEffectType.Stunned;
    }
    
    public bool IsRooted()
    {
        return HasStatusEffect() && currentEffect.type == StatusEffectType.Rooted;
    }
    
    public bool IsSlowed()
    {
        return HasStatusEffect() && currentEffect.type == StatusEffectType.Slowed;
    }
    
    public bool CanMove()
    {
        return !IsStunned() && !IsRooted();
    }
    
    public bool CanAct()
    {
        return !IsStunned();
    }
}