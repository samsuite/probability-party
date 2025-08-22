// a collection of methods that measure & affect various properties of colors

#ifndef COLORMATH
#define COLORMATH


// get the percieved luminosity value of a particular color. purple is "darker" than yellow, etc
float GetLuminosity (float3 rgb) {
    return (rgb.r * 0.2126) + (rgb.g * 0.7152) + (rgb.b * 0.0722);
}

// convert an RGB color to HSV
float3 RGBtoHSV(in float3 rgb) {
	float3 hsv = 0;

	hsv.z = max(rgb.r, max(rgb.g, rgb.b));
	float m = min(rgb.r, min(rgb.g, rgb.b));
	float c = hsv.z - m;

	if (c != 0) {
		hsv.y = c / hsv.z;
		float3 delta = (hsv.z - rgb) / c;
		delta.rgb -= delta.brg;
		delta.rg += float2(2, 4);
		if (rgb.r >= hsv.z)
			hsv.x = delta.b;
		else if (rgb.g >= hsv.z)
			hsv.x = delta.r;
		else
			hsv.x = delta.g;
		hsv.x = frac(hsv.x / 6);
	}
	return hsv;
}

// convert an HSV color to RGB
float3 HSVtoRGB(in float3 hsv) {
	float h = hsv.x;
	float r = abs(h * 6 - 3) - 1;
	float g = 2 - abs(h * 6 - 2);
	float b = 2 - abs(h * 6 - 4);

	return ((saturate(float3(r, g, b)) - 1) * hsv.y + 1) * hsv.z;
}


// set the hue of a given color without changing saturation or value
float4 SetHue (float4 rgb, float hue) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // set hue
	hsv.r = hue;

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}

// set the saturation of a given color without changing hue or value
float4 SetSaturation (float4 rgb, float sat) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // set saturation
	hsv.g = sat;

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}

// set the value of a given color without changing hue or saturation
float4 SetValue (float4 rgb, float val) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // set value
	hsv.b = val;

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}



// shift the hue of a given color
float4 HueShift(float4 rgb, float shift) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // shift hue (wrapping around)
	hsv.r += shift;
	hsv.r += 1;
	hsv.r = hsv.r % 1;

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}

// shift the value of a given color
float4 ValShift(float4 rgb, float shift) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // shift value (no wrap)
    hsv.b = saturate(hsv.b + shift);

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}

// shift the saturation of a given color
float4 SatShift(float4 rgb, float shift) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // shift saturation (no wrap)
    hsv.g = saturate(hsv.g + shift);

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}


// shift the hue and saturation of a given color
float4 HueSatShift(float4 rgb, float HueShift, float SatShift) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // shift hue (wrapping around)
	hsv.r += HueShift;
	hsv.r += 1;
	hsv.r = hsv.r % 1;

    // shift saturation (no wrap)
    hsv.g = saturate(hsv.g + SatShift);

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}

// shift the hue, saturation, and value of a given color
float4 HueSatValShift(float4 rgb, float HueShift, float SatShift, float ValShift) {
    // convert to hsv
	float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));

    // shift hue (wrapping around)
	hsv.r += HueShift;
	hsv.r += 1;
	hsv.r = hsv.r % 1;

    // shift saturation (no wrap)
    hsv.g = saturate(hsv.g + SatShift);

    // shift value (no wrap)
    hsv.b = saturate(hsv.b + ValShift);

	float4 shifted = float4(HSVtoRGB(hsv), 1);
	return shifted;
}




// hueshift a color to be perceptually "lighter" without actually changing value or saturation.
// purples become blue, greens become yellow, etc.
float4 HueshiftLighter (float4 rgb, float shift_amount) {

	const float darkest_hue = 0.69;
    // this is purple (the hue with the lowest luminosity).
    // if our hue is above this, it's more blue. if it's lower, it's more red.
    // we want to hue-shift away from this color.

	const float lightest_hue = 0.18;
    // this is yellow (the hue with the highest luminosity).
	// we want to hue-shift towards this color.


    const float near_darkest_gradient_width = 0.05;
    const float hueshift_intensity = 0.1;

    float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));
    float my_hue = hsv.x;

    float hue_offset = darkest_hue - my_hue;
    hue_offset = hue_offset/near_darkest_gradient_width;
    hue_offset = min(hue_offset, 1);
    hue_offset = max(hue_offset, -1);

    float under_lightest_mask = saturate(ceil(my_hue - lightest_hue));
    under_lightest_mask = (under_lightest_mask * 2) - 1;

    hue_offset *= under_lightest_mask;
    hue_offset *= shift_amount;
    hue_offset *= -1;

    float4 shifted_color = HueShift(rgb, hue_offset);
	return shifted_color;
}


// hueshift a color to be perceptually "darker" without actually changing value or saturation.
// blues become purple, yellows become green, etc.
float4 HueshiftDarker (float4 rgb, float shift_amount) {

	const float darkest_hue = 0.69;
    // this is purple (the hue with the lowest luminosity).
    // if our hue is above this, it's more blue. if it's lower, it's more red.
    // we want to hue-shift towards this color.

	const float lightest_hue = 0.18;
    // this is yellow (the hue with the highest luminosity).
	// we want to hue-shift away from this color.


    const float near_darkest_gradient_width = 0.05;
    const float hueshift_intensity = 0.1;

    float3 hsv = RGBtoHSV(lerp(0.001, 0.999, saturate(rgb.rgb)));
    float my_hue = hsv.x;

    float hue_offset = darkest_hue - my_hue;
    hue_offset = hue_offset/near_darkest_gradient_width;
    hue_offset = min(hue_offset, 1);
    hue_offset = max(hue_offset, -1);

    float under_lightest_mask = saturate(ceil(my_hue - lightest_hue));
    under_lightest_mask = (under_lightest_mask * 2) - 1;

    hue_offset *= under_lightest_mask;
    hue_offset *= shift_amount;

    float4 shifted_color = HueShift(rgb, hue_offset);
	return shifted_color;
}


#endif