#ifndef BLANK
#define BLANK

float4 _PW_SnowColor;

void GetSnowColor_float(out float4 color)
{
	color = _PW_SnowColor;
}

void GetSnowColor_half(out float4 color)
{
	color = _PW_SnowColor;
}

#endif



