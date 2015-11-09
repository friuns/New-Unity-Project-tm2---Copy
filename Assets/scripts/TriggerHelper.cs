using UnityEngine;

public class TriggerHelper:bs
{
    public WeaponFlameTower pl;
    void OnParticleCollision(GameObject other)
    {
        pl.particleSystem = this.particleSystem;
        pl.OnParticleCollision(other);
    }
}