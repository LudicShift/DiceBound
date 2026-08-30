using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using KCoreKit;
using UnityEngine;
using UnityEngine.Networking;

namespace DiceBound
{
    // Firebase 네이티브 Unity SDK는 WebGL을 지원하지 않아, REST API를 UnityWebRequest로 직접 호출한다.
    public class AsyncPvpBackendService
    {
        private const string WebApiKey = "AIzaSyC8akzlqbmKhs0nF8pgz56pR0jVEYprYtM";
        private const string FunctionsBaseUrl = "https://us-central1-dicebound-a95a2.cloudfunctions.net";

        private const string AuthSaveFileName = "auth.sav";
        private const string AuthSaveDirectory = "AsyncPvp/Auth";

        private string _idToken;
        private string _refreshToken;
        private string _uid;

        public string OwnerId => _uid;

        [System.Serializable]
        private class SignUpResponse
        {
            public string idToken;
            public string refreshToken;
            public string localId;
        }

        [System.Serializable]
        private class RefreshResponse
        {
            public string id_token;
            public string refresh_token;
            public string user_id;
        }
        
        public IEnumerator EnsureSignedIn()
        {
            if (SaveSystem.Exist(AuthSaveFileName, AuthSaveDirectory))
            {
                SaveSystem.Load<AsyncPvpAuthData>(AuthSaveFileName, AuthSaveDirectory, out var saved);
                _refreshToken = saved.refreshToken;
                _uid = saved.uid;
                yield return RefreshSession();
            }
            else
            {
                yield return SignUpAnonymously();
            }
        }

        private IEnumerator SignUpAnonymously()
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={WebApiKey}";
            const string bodyJson = "{\"returnSecureToken\":true}";

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AsyncPvp] 익명 로그인 실패: {request.error}\n{request.downloadHandler.text}");
                yield break;
            }

            var response = JsonUtility.FromJson<SignUpResponse>(request.downloadHandler.text);
            _idToken = response.idToken;
            _refreshToken = response.refreshToken;
            _uid = response.localId;
            PersistSession();

            Debug.Log($"[AsyncPvp] 새 익명 계정 생성: {_uid}");
        }

        private IEnumerator RefreshSession()
        {
            var url = $"https://securetoken.googleapis.com/v1/token?key={WebApiKey}";
            var form = $"grant_type=refresh_token&refresh_token={_refreshToken}";

            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(form));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[AsyncPvp] 세션 갱신 실패, 새로 로그인합니다: {request.error}");
                yield return SignUpAnonymously();
                yield break;
            }

            var response = JsonUtility.FromJson<RefreshResponse>(request.downloadHandler.text);
            _idToken = response.id_token;
            _refreshToken = response.refresh_token;
            _uid = response.user_id;
            PersistSession();

            Debug.Log($"[AsyncPvp] 세션 갱신 완료: {_uid}");
        }

        private void PersistSession()
        {
            var data = new AsyncPvpAuthData { refreshToken = _refreshToken, uid = _uid };
            SaveSystem.Save(data, AuthSaveFileName, AuthSaveDirectory);
        }

        [System.Serializable]
        private class UploadRequestBody
        {
            public int waveIndex;
            public string boardJson;
        }

        [System.Serializable]
        private class FetchRequestBody
        {
            public int waveIndex;
        }

        [System.Serializable]
        private class FetchResponseBody
        {
            public bool found;
            public string ownerId;
            public string boardJson;
        }

        // 내 보드 스냅샷을 uploadSnapshot Cloud Function으로 업로드한다.
        public IEnumerator UploadSnapshot(UnitAsyncBoardData snapshot)
        {
            var requestBody = new UploadRequestBody
            {
                waveIndex = snapshot.waveIndex,
                boardJson = JsonUtility.ToJson(snapshot)
            };

            using var request = new UnityWebRequest($"{FunctionsBaseUrl}/uploadSnapshot", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {_idToken}");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AsyncPvp] 스냅샷 업로드 실패: {request.error}\n{request.downloadHandler.text}");
            }
        }

        // getOpponentSnapshot Cloud Function을 호출해 상대 스냅샷을 조회한다.
        // 상대가 없거나 요청이 실패하면 onResult에 null을 넘긴다.
        public async Task<UnitAsyncBoardData> FetchOpponentSnapshot(int waveIndex)
        {
            var requestBody = new FetchRequestBody { waveIndex = waveIndex };

            using var request = new UnityWebRequest($"{FunctionsBaseUrl}/getOpponentSnapshot", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(requestBody)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {_idToken}");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[AsyncPvp] 상대 조회 실패: {request.error}\n{request.downloadHandler.text}");
                return null;
            }

            var response = JsonUtility.FromJson<FetchResponseBody>(request.downloadHandler.text);
            if (!response.found)
            {
                return null;
            }

            var board = JsonUtility.FromJson<UnitAsyncBoardData>(response.boardJson);
            return board;
        }
    }
}
