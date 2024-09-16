using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridBlock;

/// <summary>
/// Generic delegate for easing functions.
/// </summary>
/// <param name="x"> Always assumed to be in 0 - 1 range. </param>
/// <returns> Interpolation value. </returns>
public delegate float EasingFunction(float x);

public static class EaseHelper {
    public static float Linear(float x) => x;
    public static float ExpoIn(float x) => x <= 0 ? 0 : MathF.Pow(2, 10 * x - 10);
    public static float ExpoOut(float x) => x >= 1 ? 1 : 1.0f - MathF.Pow(2, -10 * x);
    public static float QuartOut(float x) => 1 - MathF.Pow(1 - x, 4);

    public static float QuintIn(float x) => x * x * x * x * x;
    public static float QuintOut(float x) => 1 - MathF.Pow(1 - x, 5);
    public static float QuintInOut(float x) => x < 0.5f ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2;

    public static float SineIn(float t) => -MathF.Cos(t * MathF.PI / 2);
    public static float SineOut(float t) => MathF.Sin(t * MathF.PI / 2);
    public static float SineInOut(float t) => (MathF.Cos(t * MathF.PI) - 1) / -2;


    /// <summary>
    /// Calculates linear fade based on progress.
    /// </summary>
    /// <param name="progress"> Progress of the fade in <c>[0.0, 1.0]</c>. </param>
    /// <param name="fadeIn"> Fade In time. Should be a value between 0 and 1 and result in &lt;= 1 sum with <paramref name="fadeOut"/>. </param>
    /// <param name="fadeOut"> Fade Out time. Should be a value between 0 and 1 and result in &lt;= 1 sum with <paramref name="fadeIn"/>. </param>
    /// <returns> Faded value. </returns>
    public static float Fade(float progress, float fadeIn = 0.5f, float fadeOut = 0.5f) {
        if (progress <= 0f)
            return 0f;

        if (progress <= fadeIn) {
            return progress / fadeIn;
        }

        if (progress > (1f - fadeOut)) {
            return 1f - (progress - (1f - fadeOut)) / fadeOut;
        }

        return 1f;
    }

    /// <summary>
    /// Calculates fade based on progress with support for fadeIn/fadeOut easings.
    /// </summary>
    /// <param name="progress"> Progress of the fade in <c>[0.0, 1.0]</c>. </param>
    /// <param name="fadeIn"> Fade In time. Should be a value between 0 and 1 and result in &lt;= 1 sum with <paramref name="fadeOut"/>. </param>
    /// <param name="fadeOut"> Fade Out time. Should be a value between 0 and 1 and result in &lt;= 1 sum with <paramref name="fadeIn"/>. </param>
    /// <param name="easeIn"> EaseIn function to apply to fadeIn porting of the fade. </param>
    /// <param name="easeOut"> EaseOut function to apply to fadeOut porting of the fade.</param>
    /// <returns> Interpolated value. </returns>
    public static float Fade(float progress, float fadeIn = 0.5f, float fadeOut = 0.5f, EasingFunction easeIn = default, EasingFunction easeOut = default) {
        if (progress <= fadeIn) {
            return easeIn?.Invoke(progress / fadeIn) ?? (progress / fadeIn);
        }

        if (progress > (1f - fadeOut)) {
            var value = 1f - (progress - (1f - fadeOut)) / fadeOut;
            return easeOut?.Invoke(value) ?? value;
        }

        return 1f;
    }

    public static Vector2 SampleCubicBezier(Vector2 a, Vector2 b, Vector2 c, float t) {
        t = MathHelper.Clamp(t, 0, 1);
        var oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * a +
               2f * oneMinusT * t * b +
               t * t * c;
    }
}

