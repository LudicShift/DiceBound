using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 반드시 필요함!

public class SceneChanger : MonoBehaviour
{
    // 버튼 이벤트에서 이 함수를 골라 쓸 수 있게 public으로 열어둡니다.
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}