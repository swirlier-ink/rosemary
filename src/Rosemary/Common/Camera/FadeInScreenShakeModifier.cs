using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics.CameraModifiers;

namespace Rosemary.Common;

internal class FadeInPunchCameraModifier(
    Vector2 startPosition,
    Vector2 direction,
    float strength,
    float vibrationCyclesPerSecond,
    int frames,
    PunchCameraCallback callback,
    float distanceFalloff = -1f,
    string? uniqueIdentity = null
) : CallbackPunchCameraModifier(startPosition, direction, strength, vibrationCyclesPerSecond, frames, callback, distanceFalloff, uniqueIdentity),
    ICameraModifier
{
    void ICameraModifier.Update(ref CameraInfo cameraInfo)
    {
        if (FocusHelper.PauseSounds)
        {
            return;
        }

        var wobble = MathF.Cos(_framesLasted / 60f * _vibrationCyclesPerSecond * MathF.Tau);

        var fadeIn = MathF.Pow(Utils.Remap(_framesLasted, 0f, _framesToLast, 0f, 1f), 2f);

        var distanceFalloff = Utils.Remap(Vector2.Distance(_startPosition, cameraInfo.OriginalCameraCenter), 0f, _distanceFalloff, 1f, 0f);

        if (_distanceFalloff <= -1)
        {
            distanceFalloff = 1f;
        }

        cameraInfo.CameraPosition += _direction * wobble * _strength * fadeIn * distanceFalloff;
        _framesLasted++;

        Finished = !Callback(this) || _framesLasted >= _framesToLast;
    }
}
