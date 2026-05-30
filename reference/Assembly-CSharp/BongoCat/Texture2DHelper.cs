using UnityEngine;

namespace BongoCat
{
	public static class Texture2DHelper
	{
		public static Texture2D CropTextureBackground(this Texture2D texture, Color backgroundColor)
		{
			Rect cropRectangle = GetCropRectangle(texture, backgroundColor);
			return CropTextureInternal(texture, cropRectangle);
		}

		private static Texture2D CropTextureInternal(Texture2D texture, Rect cropRect)
		{
			Texture2D texture2D = new Texture2D((int)cropRect.width, (int)cropRect.height);
			Color[] pixels = texture.GetPixels((int)cropRect.x, (int)cropRect.y, (int)cropRect.width, (int)cropRect.height);
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			if (!Application.isEditor)
			{
				Object.Destroy(texture);
				return texture2D;
			}
			Object.DestroyImmediate(texture, allowDestroyingAssets: true);
			return texture2D;
		}

		private static Rect GetCropRectangle(Texture2D texture, Color color)
		{
			int num = texture.width;
			int num2 = texture.height;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < texture.width; i++)
			{
				for (int j = 0; j < texture.height; j++)
				{
					Color pixel = texture.GetPixel(i, j);
					if (pixel.a > 0f && pixel != color)
					{
						if (i < num)
						{
							num = i;
						}
						if (j < num2)
						{
							num2 = j;
						}
						if (i > num3)
						{
							num3 = i;
						}
						if (j > num4)
						{
							num4 = j;
						}
					}
				}
			}
			return new Rect(num, num2, num3 - num, num4 - num2);
		}
	}
}
