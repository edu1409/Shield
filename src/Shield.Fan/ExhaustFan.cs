using Microsoft.Extensions.Options;
using Shield.Common.Domain;

namespace Shield.Fan
{
    public class ExhaustFan(IOptions<FanOptions> fanOptions)
        : FanService<ExhaustFan>(fanOptions.Value.PwmChipNumber, FanPwmChannel.Channel1)
    { }
}
