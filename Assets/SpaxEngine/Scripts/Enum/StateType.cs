using System;
namespace FightingGameEngine.Enum
{
    [Flags]
    public enum CancelConditions : uint
    {

        NEUTRAL = 1 << 0,
        JUMP = 1 << 1,
        DASH = 1 << 2,
        GUARD = 1 << 3,
        GRAB = 1 << 4,
        STANCED = 1 << 5,
        BURST = 1 << 6,
        SUPER_LV1 = 1 << 7,
        SUPER_LV2 = 1 << 8,
        SUPER_LV3 = 1 << 9,
        SUPER_LV4 = 1 << 10,
        SUPER_LV5 = 1 << 11,
        SUPER_LV6 = 1 << 12,
        SPCL_LV1 = 1 << 13,
        SPCL_LV2 = 1 << 14,
        SPCL_LV3 = 1 << 15,
        SPCL_LV4 = 1 << 16,
        SPCL_LV5 = 1 << 17,
        SPCL_LV6 = 1 << 18,
        CMDNORM_LV1 = 1 << 19,
        CMDNORM_LV2 = 1 << 20,
        CMDNORM_LV3 = 1 << 21,
        CMDNORM_LV4 = 1 << 22,
        CMDNORM_LV5 = 1 << 23,
        CMDNORM_LV6 = 1 << 24,
        NORM_LV1 = 1 << 25,
        NORM_LV2 = 1 << 26,
        NORM_LV3 = 1 << 27,
        NORM_LV4 = 1 << 28,
        NORM_LV5 = 1 << 29,
        NORM_LV6 = 1 << 30,
        SUPER = SUPER_LV1 | SUPER_LV2,
        SPECIAL = SPCL_LV1 | SPCL_LV2 | SPCL_LV3 | SPCL_LV4 | SPCL_LV5 | SPCL_LV6,
        COMMAND_NORMAL = CMDNORM_LV1 | CMDNORM_LV2 | CMDNORM_LV3 | CMDNORM_LV4 | CMDNORM_LV5 | CMDNORM_LV6,
        NORMAL = NORM_LV1 | NORM_LV2 | NORM_LV3 | NORM_LV4 | NORM_LV5 | NORM_LV6,
    }
}