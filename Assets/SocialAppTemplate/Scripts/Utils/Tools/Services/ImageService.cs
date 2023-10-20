using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UniTools
{
	public interface IImageService
	{
		void DownloadOrLoadTexture(string url, Action<Texture2D> callback);
		void DownloadOrLoadTexture(string url, string fileName, Action<Texture2D> callback);
		Texture2D LoadTexture(string fileName);
		bool InCache(string fileName);
		bool InDisk(string fileName);
	}

	public class ImageService : IImageService
	{
		private const int TIMEOUT = 15;

		private readonly ICoroutineExecuter _coroutineExecuter;

		private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();
		private readonly Dictionary<string, List<Action<Texture2D>>> _downloadingQueu = new Dictionary<string, List<Action<Texture2D>>>();

		public ImageService(ICoroutineExecuter ce)
		{
			_coroutineExecuter = ce;
		}

		public Texture2D LoadTexture(string fileName)
		{
			if (InCache(fileName))
				return _cache[fileName];

			if (InDisk(fileName))
			{
				var texture = GetTexture(fileName);

				_cache.Add(fileName, texture);

				return _cache[fileName];
			}

			return null;
		}

		public void DownloadOrLoadTexture(string url, Action<Texture2D> callback)
		{
			DownloadOrLoadTexture(url, url.GetMd5Hash(), callback);
		}

		public void DownloadOrLoadTexture(string url, string fileName, Action<Texture2D> callback)
		{
			if (InCache(fileName))
			{
				if (callback != null)
					callback(_cache[fileName]);

				return;
			}

			if (InDisk(fileName))
			{
				var texture = GetTexture(fileName);

				_cache.Add(fileName, texture);

				if (callback != null)
					callback(_cache[fileName]);

				return;
			}

			if (_downloadingQueu.ContainsKey(fileName))
			{
				_downloadingQueu[fileName].Add(callback);
				return;
			}

			_downloadingQueu.Add(fileName, new List<Action<Texture2D>>());
			_downloadingQueu[fileName].Add(callback);
			_coroutineExecuter.Execute(LoadTexture(url, data =>
			{
				//error loading
				if (data == null)
				{
					LogError("Can'not load image data:" + fileName);

					ExecuteCallback(fileName, null);

					return;
				}

				var texture = GetTexture(data);

				if (texture == null)
				{
					LogError("Can'not parse image data:" + fileName);

					ExecuteCallback(fileName, null);

					return;
				}

				if (_cache.ContainsKey(fileName))
				{
					_cache[fileName] = texture;
				}
				else
				{
					_cache.Add(fileName, texture);
				}

				try
				{
					DiskStorage.Write(fileName, data);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}

				ExecuteCallback(fileName, texture);
			}));
		}

		private void ExecuteCallback(string file, Texture2D texture)
		{
			if (_downloadingQueu.ContainsKey(file))
			{
				var list = _downloadingQueu[file];

				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						var callback = list[i];

						if (callback != null)
							callback(texture);
					}
				}

				_downloadingQueu.Remove(file);
			}
		}

		public bool InCache(string file)
		{
			return _cache.ContainsKey(file);
		}

		public bool InDisk(string file)
		{
			return DiskStorage.Exists(file);
		}

		private Texture2D GetTexture(string file)
		{
			byte[] bytes = DiskStorage.Read(file);

			return GetTexture(bytes);
		}

		private Texture2D GetTexture(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
				return null;

			Texture2D texture = NewTexture(true);

			texture.LoadImage(bytes);

			return texture;
		}

		private Texture2D NewTexture(bool bAlpha)
		{
			Texture2D texure = new Texture2D(128, 128, bAlpha ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);

			texure.filterMode = FilterMode.Bilinear;

			texure.wrapMode = TextureWrapMode.Clamp;

			return texure;
		}

		private IEnumerator LoadTexture(string url, Action<byte[]> onFinished)
		{
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            float elapsedTime = 0.0f;

			while (!www.isDone)
			{
				elapsedTime += Time.deltaTime;

				if (elapsedTime >= TIMEOUT)
				{
					if (onFinished != null)
						onFinished(null);

					yield break;
				}

				yield return null;
			}

			if (!www.isDone || !string.IsNullOrEmpty(www.error) || www.downloadedBytes == 0)
			{
                if (onFinished != null)
					onFinished(null);

				yield break;
			}

			var response = www.downloadHandler.data;

			if (onFinished != null)
				onFinished(response);
		}

		private void LogError(string data)
		{
			Debug.LogError("[ImageService]" + data);
		}
	}
}