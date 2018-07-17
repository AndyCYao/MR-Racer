#include "UnityCG.cginc"

struct appdata
{
	half4 vertex : POSITION;
	half3 normal : NORMAL;
};

struct v2f
{
	half3 normal : NORMAL;
	half4 vertex : SV_POSITION;
	half4 uv : TEXCOORD1;
	half4 worldPos : TEXCOORD2;
	half4 avoidParams : TEXCOORD3;
};

fixed _u_alpha_;
half _avoid_k;

v2f vertCollAvoid (appdata v)
{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.worldPos.xyz = v.vertex.xyz;
	COMPUTE_EYEDEPTH(o.worldPos.w);
	o.normal = half3(1.0, 1.0, 1.0) - v.normal * v.normal;				// special normal to calc grid
	o.avoidParams = half4(_avoid_k, _avoid_k - 0.8, _avoid_k * 5.0, 0);	// just to calculate this in vertex shader
	return o;
}

half3 modGreenLine(half3 worldPos, half _gridSize_, half _radius_)
{
	half d_pos = length(fwidth(worldPos) /_gridSize_ );
	_radius_ *= d_pos;
	_radius_ = clamp(_radius_, 0.01, 0.2);
	half3 _p0_ = frac(worldPos / _gridSize_);
	
	// ramp and reversed ramp to make a bump
	return smoothstep(1. - _radius_, 1., _p0_) + (1. - smoothstep(0., _radius_, _p0_));
}

fixed4 fragGreenGrid (v2f i) : SV_Target
{
	fixed4 _majorGridColor_ = fixed4(0.1, 1.0, 0.7, 1.0);
	fixed4 _minorGridColor_ = fixed4(0.1, 1.0, 0.7, 0.8);
	
	// control the falloff of the lines as surface stops being parallel
	half3 powNormal = i.normal;
	
	// dot these so that we can see the grid when normal dir is ortho to the grid
	half _maj_grid_ = dot( modGreenLine(i.worldPos.xyz, 0.15,   2.0), powNormal);
	half _min_grid_ = dot( modGreenLine(i.worldPos.xyz, 0.0375, 1.0), powNormal);
	
	fixed4 _color_ = (1. - _maj_grid_ - _min_grid_) * fixed4(1,1,1,0) + _maj_grid_ * _majorGridColor_ + _min_grid_ * _minorGridColor_;
	_color_.a *= _u_alpha_;
	
	if( !IsGammaSpace() ){
        _color_.rgb = GammaToLinearSpace (_color_.rgb);
	}
	return _color_;
}

// ------------------------------------------------
// Pink Grid Methods
// ------------------------------------------------

half3 modPinkLine(half3 worldPos)
{
	half _gridSize_ = 0.1;
	half halfRadius = 0.025;
	// mod to repeat
	half3 p0 = frac(worldPos / _gridSize_);
	// ramp and reversed ramp to make a bump
	return smoothstep(1.0 - 0.1 - halfRadius , 1.0 - halfRadius, p0) + (1.0 - smoothstep(halfRadius, halfRadius + 0.1, p0));
}
    
fixed4 _hsv2rgb_(half h, fixed alpha)
{
	half3 K = half3(1.0, 2.0 / 3.0, 1.0 / 3.0);
	half3 p = abs(frac(h + K) * 6.0 - 3.0);
	return fixed4(saturate(p - 1.0), alpha);
}

fixed4 pinkGridFunction(half screenPosZ, half3 worldPos, half3 normal, half4 avoidParams, fixed alpha)
{
	half _v_depth_meters_ = screenPosZ;
	half v = smoothstep(avoidParams.y, avoidParams.x, _v_depth_meters_);
	
	if (v >= 0.95) {
		return fixed4(0.0,0.0,0.0,0.0);  // skip a lot of computation if it's fade out
	}

	v = 0.8 - v * v;

	half _inv_grid_ = 1.0 - dot(modPinkLine(worldPos), normal);
	
	fixed _opacity_ = 0.5 * v * alpha * _inv_grid_;
	
	half h = -_v_depth_meters_ / avoidParams.z;
	
	return _hsv2rgb_(h, _opacity_);
}

fixed4 fragPinkColl (v2f i) : SV_Target
{
	fixed4 _color_ = pinkGridFunction(i.worldPos.w, i.worldPos.xyz, i.normal, i.avoidParams, _u_alpha_);

	if( !IsGammaSpace() ){
        _color_.rgb = GammaToLinearSpace (_color_.rgb);
	}
	return _color_;
}