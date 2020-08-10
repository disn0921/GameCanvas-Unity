/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCanvas.Engine
{
    public sealed class Network
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private readonly BehaviourBase cBehaviour;
        private readonly Graphic cGraphic;
        private readonly Dictionary<string, object> cCache;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Network(BehaviourBase behaviour, Graphic graphic)
        {
            cBehaviour = behaviour;
            cGraphic = graphic;
            cCache = new Dictionary<string, object>();
        }

        public void Clear()
        {
            cCache.Clear();
        }

        public EDownloadState DrawOnlineImage(in string url, in int x, in int y)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(DownloadImage(url));
                return EDownloadState.Begin;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null)
            {
                // ダウンロード中
                return EDownloadState.Downloading;
            }

            cGraphic.DrawTexture(cache, in x, in y);
            return EDownloadState.Complete;
        }

        public int GetOnlineImageWidth(in string url)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(DownloadImage(url));
                return 0;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null) return 0;
            return cache.width;
        }

        public int GetOnlineImageHeight(in string url)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(DownloadImage(url));
                return 0;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null) return 0;
            return cache.height;
        }

        public EDownloadState GetOnlineText(in string url, out string text)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(DownloadText(url));
                text = null;
                return EDownloadState.Begin;
            }

            var cache = cCache[url] as string;
            if (cache == null)
            {
                // ダウンロード中
                text = null;
                return EDownloadState.Downloading;
            }

            text = cache;
            return EDownloadState.Complete;
        }

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        private IEnumerator DownloadImage(string url)
        {
            var req = UnityWebRequestTexture.GetTexture(url, true);
            yield return req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogWarning(req.error);
                yield break;
            }

            cCache[url] = ((DownloadHandlerTexture)req.downloadHandler).texture;
        }

        private IEnumerator DownloadText(string url)
        {
            var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogWarning(req.error);
                cCache[url] = "";
                yield break;
            }

            var text = req.downloadHandler.text;
            cCache[url] = text;
        }

        #endregion
    }
}
