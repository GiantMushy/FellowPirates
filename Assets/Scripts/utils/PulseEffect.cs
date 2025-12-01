using System.Collections;
using UnityEngine;

public static class PulseEffect
{
    public static IEnumerator sprite_pulse(SpriteRenderer spriteRenderer, int num_pulses = -1, float speed = 2.5f, float intensity = 1.3f)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("Missing sprite in sprite pulse function");
            yield break;
        }

        Color dark_sprite_color = spriteRenderer.color;
        Color bright_sprite_color = dark_sprite_color * intensity;

    
        int count = 0;
        while (num_pulses < 0 || count < num_pulses)
        {
            // LERP
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                spriteRenderer.color = Color.Lerp(dark_sprite_color, bright_sprite_color, t);
                yield return null; // to go to next frame;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                spriteRenderer.color = Color.Lerp(bright_sprite_color, dark_sprite_color, t);
                yield return null;
            }

            count++;

        }

        spriteRenderer.color = dark_sprite_color;
    }
}