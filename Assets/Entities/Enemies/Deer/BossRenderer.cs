using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRenderer : EntityRenderer
{
    public BossRenderer(BossAI entity) : base(entity)
    {
    }


    protected override IEnumerator EnemyDeathAnimation()
    {
        BossAI entity = (BossAI)this.Entity;
        deathAnimTime = 2.0f; // Boss death animation is longer (why is this hardcoded in the other renderer as well seems stupid to me)
        float particleTimer = 0.1f;
        float timeProgressed = deathAnimTime;
        float particleTimeProgressed = particleTimer;
        Color normalColor = Color.white;
        CameraMover.Singleton.ScreenShake(1.3f, 0.03f, deathAnimTime);


        var shape = entity.shieldParticles.shape;
        shape.rotation = new Vector3(0, 0, 0);

        while (timeProgressed > 0)
        {
            normalColor.a = timeProgressed / deathAnimTime;
            spriteRenderer.color = normalColor;
            timeProgressed -= Time.deltaTime;
            particleTimeProgressed -= Time.deltaTime;
            var particleMain = entity.shieldParticles.main;
            if (particleTimeProgressed <= 0)
            {
                particleMain.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 1f, 1f, normalColor.a));
                particleTimeProgressed = particleTimer;
                entity.shieldParticles.Emit(30);
            }
            yield return null;
        }
        entity.enabled = false;

        yield return new WaitForSeconds(2f);

        GameManager.Singleton.EndRun(true);
    }
}
