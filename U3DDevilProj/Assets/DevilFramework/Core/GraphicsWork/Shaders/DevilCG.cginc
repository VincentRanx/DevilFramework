#ifndef DEVIL_CG
#define DEVIL_CG

#define RANDOM_SEED 753.5453123
#define GRAY(color) dot(color.rgb, half3(0.375, 0.3125, 0.3125))

inline float smoothlerp(float x, float y, float amount)
{
	float t = amount * amount * amount * (amount * (amount * 6-15) + 10);
	return (y - x) * t + x;
}

inline float blend(float x, float y, float t)
{
	return x * t + y * (1 - t);
}

inline float random1(float seed)
{
	return frac(sin(seed) * RANDOM_SEED) * 2 - 1;
}

inline float2 randomGradient(float2 p)
{
	float rad = random1(p.x * 1.5 + p.y * 1.3) * RANDOM_SEED;
	return float2(sin(rad), cos(rad));
}

inline float noise1(float p)
{
	float sp = p - frac(p);
	float x = random1(sp);
	float y = random1(sp+1);
	return smoothlerp(x, y, frac(p));
}

inline float noise2(float2 pos)
{
	float2 p0 = pos - frac(pos);
	float2 p1 = p0 + float2(1, 1);
	float2 dir1 = randomGradient(p0);
	float2 dir2 = pos - p0;
	float4 w;
	w.x = dot(dir1, dir2);
	dir1 = randomGradient(float2(p1.x, p0.y));
	dir2 = pos - float2(p1.x, p0.y);
	w.y = dot(dir1, dir2);
	dir1 = randomGradient(float2(p0.x, p1.y));
	dir2 = pos - float2(p0.x, p1.y);
	w.z = dot(dir1, dir2);
	dir1 = randomGradient(p1);
	dir2 = pos - p1;
	w.w = dot(dir1, dir2);
	pos = frac(pos);
	float x = smoothlerp(w.x, w.y, pos.x);
	float y = smoothlerp(w.z, w.w, pos.x);
	return smoothlerp(x, y, pos.y);
}
#endif