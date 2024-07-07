using Microsoft.Extensions.Options;
using Shield.Common.Domain;

namespace Shield.Fan
{
    public class IntakeFan(IOptions<FanOptions> fanOptions) 
        : FanService<IntakeFan>(fanOptions.Value.PwmChipNumber, FanPwmChannel.Channel0)
    { }
}
