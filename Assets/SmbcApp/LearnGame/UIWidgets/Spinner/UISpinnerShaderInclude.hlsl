#define SMOOTH(p,r,s) smoothstep(-s, s, p-(r))
#define TPI 6.2831
#define HPI 1.570796

float get_bias(float x, float bias)
{
    return (x / ((((1.0 / bias) - 2.0) * (1.0 - x)) + 1.0));
}

float get_gain(float x, float gain)
{
    if (x < 0.5)
        return get_bias(x * 2.0, gain) / 2.0;
    return get_bias(x * 2.0 - 1.0, 1.0 - gain) / 2.0 + 0.5;
}

// from https://iquilezles.org/articles/smin
// polynomial smooth min (k = 0.1);
float s_min(float a, float b, float k)
{
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return lerp(b, a, h) - k * h * (1.0 - h);
}

float s_max(float a, float b, float k)
{
    return (-s_min(-a, -b, k));
}

float s_clamp(float f, float k)
{
    return s_min(1., s_max(0., f, k), k);
}


float hex(float2 pos)
{
    const float corner = .015;
    float2 q = abs(pos);
    return s_max(
        s_max((q.x * 0.866025 + pos.y * 0.5), q.y, corner),
        s_max((q.x * 0.866025 - pos.y * 0.5), q.y, corner), corner);
}

float hex_radius_factor(float time)
{
    time *= 2.;
    float s = s_clamp(sin(time) + .65, .25);

    return s;
}

float hex_fest(inout float3 col, in float2 uv, in float time)
{
    float3 hex_color = float3(0.294, 0.360, 0.478);

    float a = -PI / 3.;
    float sa = sin(a);
    float ca = cos(a);
    uv = mul(float2x2(sa, ca, ca, -sa), uv);

    float res_a = 0;

    //hexagons
    const float delta_time = 1. / 8. * 1.2;
    const float base_hex_radius = .1;
    float2 hex_delta = float2(.195, .21);

    float time_acc = 1.;

    //hex1
    time_acc += 1.;
    float rf = hex_radius_factor(time + delta_time * time_acc);
    float radius = base_hex_radius * rf;

    float f = hex(uv);
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;

    //hex2
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(1., .5));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;

    //hex3
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(1., -.5));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;

    //hex4
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(.0, -1.));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;

    //hex5
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(-1., -.5));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;

    //hex6
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(-1., .5));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;


    //hex7
    time_acc += 1.;
    rf = hex_radius_factor(time + delta_time * time_acc);
    radius = base_hex_radius * rf;

    f = hex(uv - hex_delta * float2(0., 1.));
    f = SMOOTH(radius, f, .0025);

    col = lerp(col, hex_color, f * rf);
    res_a += f * rf;
    return saturate(res_a);
}


#define CIRCLE(l,r,ht,s) SMOOTH(l,r-ht,s) - SMOOTH(l,r+ht,s)

float circle_fest(inout float3 col, in float2 uv, in float time)
{
    float res_a = 0;
    const float circle_s = .013;

    float len = length(uv);
    float ang = atan2(uv.y, uv.x);

    float3 circle_col = float3(0, 0, 0);

    float f = (CIRCLE(len, .45, .003, circle_s)) * .15;
    col = lerp(col, circle_col, f);
    res_a += f;

    time = -1.485 + time * 2.; // * 2. + 1.4;

    float a = (ang + time) / TPI;
    a = a - floor(a);

    f = (CIRCLE(len, .45, .006, circle_s)) * .05;

    float start_time = max(fmod(time + HPI,TPI),PI) + HPI;

    float start = sin(start_time) * .5 + .5;

    float end_time = min(fmod(time + HPI,TPI),PI) + HPI;

    float end = sin(end_time) * .5 + .5;

    f *= step(a, 1. - start) - step(a, end);
    col = lerp(col, circle_col, f * 3.5);
    res_a += f * 3.5;

    f = (CIRCLE(len, .45, .003, circle_s));
    f *= step(a, .04 + sin(time) * .01) - step(a, 0.);

    col = lerp(col, circle_col, f);
    res_a += f;

    f = (CIRCLE(len, .62, .003, circle_s));
    col = lerp(col, circle_col, f * .25);
    res_a += f * .25;

    f = CIRCLE(len, .62, .003, circle_s);

    time += 1.;
    time = get_gain(frac(time / TPI), .25) * TPI;
    a = (ang - time - 1.5) / TPI;
    a += sin(time) * .15;
    a = (a - floor(a));
    //a = GetBias(a,.65);
    f *= step(a, .03) - step(a, 0.);
    col = lerp(col, circle_col, f);
    res_a += f;
    return saturate(res_a);
}
