#include "sh_Utils.h"
#include "sh_Masking.h"

layout(location = 0) out vec4 o_Colour;
layout(location = 2) in mediump vec2 v_TexCoord;

void main(void)
{
    float DistanceToLine = mod((v_TexCoord.x+v_TexCoord.y) / (v_TexRect[2] - v_TexRect[0]), 0.3);
    bool pixelLit = DistanceToLine < 0.15;
    o_Colour = getRoundedColor( vec4(1,1,1,pixelLit ? 1 : 0), v_TexCoord);
}
