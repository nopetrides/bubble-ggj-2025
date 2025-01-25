using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TheraBytes.BetterUi
{
	[Serializable]
	public class DpiManager
	{
#if UNITY_WEBGL
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern double GetDPI();

#endif

		[Serializable]
		public class DpiOverride
		{
			[SerializeField] private float dpi = 96;
			[SerializeField] private string deviceModel;

			public DpiOverride(string deviceModel, float dpi)
			{
				this.deviceModel = deviceModel;
				this.dpi = dpi;
			}

			public float Dpi => dpi;
			public string DeviceModel => deviceModel;
		}

		[SerializeField] private List<DpiOverride> overrides = new();

		public float GetDpi()
		{
			var ov = overrides.FirstOrDefault(o => o.DeviceModel == SystemInfo.deviceModel);

			if (ov != null)
				return ov.Dpi;

#if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                // Fix Web GL Dpi bug (Unity thinks it is always 96 DPI)
                return (float)(GetDPI() * 96.0f);
            }
            catch
            {
                Debug.LogError("Could not retrieve real DPI. Is the WebGL-DPI-Plugin installed in the project?");
            }
#endif
			return Screen.dpi;
		}
	}
}