using System.Collections.Generic;

namespace GridBlock.Common;

public class GridBlockUiAnimations {
    public readonly List<IAnimation> Active = [];

    public void Update() {
        if (Active.Count == 0)
            return;

        var shouldClearBuffer = false;
        foreach (var anim in Active) {
            anim.Update();
            anim.Lifetime++;

            if (anim.IsExpired) shouldClearBuffer = true;
        }

        if (shouldClearBuffer) Active.RemoveAll(a => a.IsExpired);
    }

    public void Draw() {
        if (Active.Count == 0)
            return;

        foreach (var anim in Active) {
            anim.Draw();
        }
    }
}
