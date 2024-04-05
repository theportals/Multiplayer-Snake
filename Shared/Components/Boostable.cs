namespace Shared.Components;

public class Boostable : Component
{
    public float maxStamina;
    public float stamina;
    public float regenRate;
    public float staminaDrain;
    public float speedModifier;
    public float penaltySpeed;
    public bool boosting = false;

    public Boostable(float maxStamina, float staminaDrain, float speedModifier, float regenRate, float penaltySpeed, float stamina=-42, bool boosting=false)
    {
        this.maxStamina = maxStamina;
        this.staminaDrain = staminaDrain;
        this.speedModifier = speedModifier;
        this.regenRate = regenRate;
        this.penaltySpeed = penaltySpeed;
        if (stamina < -40) this.stamina = maxStamina;
        else this.stamina = stamina;
        this.boosting = boosting;
    }
}