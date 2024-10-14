namespace GridBlock.Common.UserInterface;

public interface IAnimation {
    /// <summary>
    /// Increments each tick.
    /// </summary>
    float Lifetime { get; set; }

    /// <summary>
    /// When <see langword="true"/>, animation will be cleared.
    /// </summary>
    bool IsExpired { get; }

    /// <summary>
    /// Update logic for this animation.
    /// </summary>
    void Update();

    /// <summary>
    /// Draw logic for this animation.
    /// </summary>
    void Draw();
}
