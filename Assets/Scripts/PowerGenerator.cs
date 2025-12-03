using UnityEngine;

public class PowerGenerator
{
    public PowerGeneratorConfig config;
    public int currentEnergy;
    private float regenTimer;
    private float delayTimer;

    public PowerGenerator(PowerGeneratorConfig cfg)
    {
        config = cfg;
        currentEnergy = cfg.maxEnergy;
    }

    public bool TryConsume(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            delayTimer = config.startRegenDelay; // сброс регена
            return true;
        }
        return false;
    }

    public void Update(float deltaTime)
    {
        if (delayTimer > 0)
        {
            delayTimer -= deltaTime;
            return;
        }

        regenTimer += deltaTime;
        if (regenTimer >= config.regenRate)
        {
            regenTimer = 0f;
            currentEnergy = Mathf.Min(
                currentEnergy + config.regenStepValue,
                config.maxEnergy
            );
        }
    }

    public int CurrentEnergy => currentEnergy;
}