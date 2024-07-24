public static class AudioManagerExt {

	public static float Remap(this float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo) {
		return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
	}
	public static float Remap(this int from, float fromMin, float fromMax, float toMin, float toMax) {
		return ((float)from).Remap(fromMin, fromMax, toMin, toMax);
	}

}